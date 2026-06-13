package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonInclude;
import com.fasterxml.jackson.databind.JavaType;
import com.fasterxml.jackson.databind.ObjectMapper;

import java.io.ByteArrayOutputStream;
import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.nio.charset.StandardCharsets;
import java.time.Duration;
import java.util.UUID;

/**
 * The single place that talks to the network. Owns bearer authentication, JSON
 * encode/decode, retries on 429/5xx with backoff (honouring {@code Retry-After}),
 * and mapping non-2xx responses to {@link AxeneException}. Resources are thin and
 * delegate to this transport; they never touch {@link HttpClient} directly.
 */
final class ApiTransport {

    private final HttpClient http;
    private final ObjectMapper mapper;
    private final String apiKey;
    private final String baseUrl;
    private final int maxRetries;
    private final Duration timeout;

    ApiTransport(String apiKey, String baseUrl, int maxRetries, Duration timeout) {
        this.apiKey = apiKey;
        this.baseUrl = baseUrl;
        this.maxRetries = maxRetries;
        this.timeout = timeout;
        this.http = HttpClient.newBuilder().connectTimeout(Duration.ofSeconds(15)).build();
        this.mapper = new ObjectMapper().setSerializationInclusion(JsonInclude.Include.NON_NULL);
    }

    /** Shared Jackson mapper, exposed so resources can build {@link JavaType}s for generic results. */
    ObjectMapper mapper() {
        return mapper;
    }

    /** Construct a {@link JavaType} for a simple class. */
    JavaType type(Class<?> c) {
        return mapper.getTypeFactory().constructType(c);
    }

    /** Construct a {@link JavaType} for a {@code List<T>}. */
    JavaType listType(Class<?> element) {
        return mapper.getTypeFactory().constructCollectionType(java.util.List.class, element);
    }

    /**
     * Perform a JSON request and parse the response.
     *
     * <p>Retries {@code 429} and {@code 5xx} with exponential backoff (honouring
     * {@code Retry-After} when present). Throws {@link AxeneException} on a final
     * non-2xx response or on a transport failure that survives all attempts.
     *
     * @param method HTTP method.
     * @param path   request path, appended to the base URL.
     * @param body   request body to JSON-encode, or {@code null} for no body.
     * @param type   the target type to decode the response into ({@code Void} returns {@code null}).
     * @param <T>    the decoded response type.
     * @return the decoded response, or {@code null} for a {@code Void} type.
     */
    <T> T request(String method, String path, Object body, JavaType type) {
        AxeneException last = null;
        for (int attempt = 1; attempt <= maxRetries; attempt++) {
            try {
                HttpRequest.Builder rb = HttpRequest.newBuilder(URI.create(baseUrl + path))
                        .timeout(timeout)
                        .header("Authorization", "Bearer " + apiKey)
                        .header("Content-Type", "application/json")
                        .header("User-Agent", "axene-mailer-java/0.1.0");
                if (body == null) {
                    rb.method(method, HttpRequest.BodyPublishers.noBody());
                } else {
                    rb.method(method, HttpRequest.BodyPublishers.ofString(mapper.writeValueAsString(body)));
                }

                HttpResponse<String> res = http.send(rb.build(), HttpResponse.BodyHandlers.ofString());
                int code = res.statusCode();

                if ((code == 429 || code >= 500) && attempt < maxRetries) {
                    sleepBackoff(res, attempt);
                    continue;
                }
                return finish(code, res.body(), type);
            } catch (AxeneException e) {
                throw e;
            } catch (Exception e) {
                last = new AxeneException(0, "Axene request failed: " + e.getMessage(), null);
                if (attempt < maxRetries) {
                    sleepBackoff(null, attempt);
                    continue;
                }
            }
        }
        throw last != null ? last : new AxeneException(0, "Axene request failed", null);
    }

    /**
     * Upload a single file as {@code multipart/form-data} under the field name
     * {@code file}. The multipart body and boundary are built by hand so this
     * works on a plain {@link HttpClient}. Not retried (uploads are not idempotent).
     *
     * @param path     request path, appended to the base URL.
     * @param file     raw file bytes.
     * @param filename the filename to advertise in the multipart part.
     * @param type     the target type to decode the response into.
     * @param <T>      the decoded response type.
     * @return the decoded response.
     */
    <T> T upload(String path, byte[] file, String filename, JavaType type) {
        String boundary = "----axene" + UUID.randomUUID().toString().replace("-", "");
        byte[] payload = multipartBody(boundary, file, filename);
        try {
            HttpRequest req = HttpRequest.newBuilder(URI.create(baseUrl + path))
                    .timeout(timeout)
                    .header("Authorization", "Bearer " + apiKey)
                    .header("Content-Type", "multipart/form-data; boundary=" + boundary)
                    .header("User-Agent", "axene-mailer-java/0.1.0")
                    .POST(HttpRequest.BodyPublishers.ofByteArray(payload))
                    .build();
            HttpResponse<String> res = http.send(req, HttpResponse.BodyHandlers.ofString());
            return finish(res.statusCode(), res.body(), type);
        } catch (AxeneException e) {
            throw e;
        } catch (Exception e) {
            throw new AxeneException(0, "Axene upload failed: " + e.getMessage(), null);
        }
    }

    private static byte[] multipartBody(String boundary, byte[] file, String filename) {
        String header = "--" + boundary + "\r\n"
                + "Content-Disposition: form-data; name=\"file\"; filename=\"" + filename + "\"\r\n"
                + "Content-Type: application/octet-stream\r\n\r\n";
        String footer = "\r\n--" + boundary + "--\r\n";
        ByteArrayOutputStream out = new ByteArrayOutputStream();
        byte[] h = header.getBytes(StandardCharsets.UTF_8);
        byte[] f = footer.getBytes(StandardCharsets.UTF_8);
        out.write(h, 0, h.length);
        out.write(file, 0, file.length);
        out.write(f, 0, f.length);
        return out.toByteArray();
    }

    private <T> T finish(int code, String body, JavaType type) throws Exception {
        if (code < 200 || code >= 300) {
            throw AxeneException.fromResponse(code, body);
        }
        if (type.getRawClass() == Void.class || body == null || body.isEmpty()) {
            return null;
        }
        return mapper.readValue(body, type);
    }

    private void sleepBackoff(HttpResponse<String> res, int attempt) {
        long ms = 250L * (long) Math.pow(2, attempt - 1);
        if (res != null) {
            String ra = res.headers().firstValue("retry-after").orElse(null);
            if (ra != null) {
                try {
                    long secs = Long.parseLong(ra.trim());
                    if (secs > 0) ms = secs * 1000L;
                } catch (NumberFormatException ignored) {
                    // non-numeric Retry-After: fall back to exponential backoff
                }
            }
        }
        try {
            Thread.sleep(ms);
        } catch (InterruptedException ie) {
            Thread.currentThread().interrupt();
        }
    }
}
