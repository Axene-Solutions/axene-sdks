package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonInclude;
import com.fasterxml.jackson.databind.JavaType;
import com.fasterxml.jackson.databind.ObjectMapper;

import java.net.URI;
import java.net.URLEncoder;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.nio.charset.StandardCharsets;
import java.time.Duration;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 * Official Java client for Axene Mailer.
 *
 * <pre>{@code
 * AxeneMailerClient axene = new AxeneMailerClient("axm_k_your_api_key");
 * SendEmailResult res = axene.send(SendEmail.builder()
 *     .from("hello@yourdomain.com", "Your Shop")
 *     .to("customer@example.com")
 *     .subject("Your receipt")
 *     .html("<p>Thanks for your order.</p>")
 *     .build());
 * }</pre>
 */
public final class AxeneMailerClient {

    private static final String DEFAULT_BASE = "https://mail.axene.io";

    private final HttpClient http;
    private final ObjectMapper mapper;
    private final String apiKey;
    private final String baseUrl;
    private final int maxRetries;

    /** @param apiKey API key from your dashboard (starts with {@code axm_k_}). */
    public AxeneMailerClient(String apiKey) {
        this(apiKey, DEFAULT_BASE, 3);
    }

    /**
     * @param apiKey     API key from your dashboard.
     * @param baseUrl    override the API base URL (defaults to https://mail.axene.io).
     * @param maxRetries total attempts on 429 / 5xx (defaults to 3).
     */
    public AxeneMailerClient(String apiKey, String baseUrl, int maxRetries) {
        if (apiKey == null || apiKey.trim().isEmpty()) {
            throw new IllegalArgumentException("apiKey is required");
        }
        this.apiKey = apiKey;
        this.baseUrl = (baseUrl == null ? DEFAULT_BASE : baseUrl).replaceAll("/+$", "");
        this.maxRetries = Math.max(1, maxRetries);
        this.http = HttpClient.newBuilder().connectTimeout(Duration.ofSeconds(15)).build();
        this.mapper = new ObjectMapper().setSerializationInclusion(JsonInclude.Include.NON_NULL);
    }

    /** Send a single email. */
    public SendEmailResult send(SendEmail email) {
        return request("POST", "/v1/emails/", email.toWire(), type(SendEmailResult.class));
    }

    /** Send up to your plan's batch limit in one call. */
    public BatchResult sendBatch(List<SendEmail> emails) {
        List<Map<String, Object>> wire = new ArrayList<>();
        for (SendEmail e : emails) {
            wire.add(e.toWire());
        }
        Map<String, Object> body = new HashMap<>();
        body.put("emails", wire);
        return request("POST", "/v1/emails/batch", body, type(BatchResult.class));
    }

    /** Fetch a single email and its current status. */
    public EmailRecord get(String id) {
        return request("GET", "/v1/emails/" + enc(id), null, type(EmailRecord.class));
    }

    /** Validate an address is well-formed and its domain can receive mail. */
    public ValidationResult validate(String email) {
        Map<String, Object> body = new HashMap<>();
        body.put("email", email);
        return request("POST", "/v1/emails/validate", body, type(ValidationResult.class));
    }

    /** List your sending domains and their verification status. */
    public List<DomainRecord> listDomains() {
        JavaType listType = mapper.getTypeFactory().constructCollectionType(List.class, DomainRecord.class);
        return request("GET", "/v1/domains/", null, listType);
    }

    // -- internals ---------------------------------------------------------

    private JavaType type(Class<?> c) {
        return mapper.getTypeFactory().constructType(c);
    }

    private <T> T request(String method, String path, Object body, JavaType type) {
        RuntimeException last = null;
        for (int attempt = 1; attempt <= maxRetries; attempt++) {
            try {
                HttpRequest.Builder rb = HttpRequest.newBuilder(URI.create(baseUrl + path))
                        .timeout(Duration.ofSeconds(30))
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
                    sleepBackoff(attempt);
                    continue;
                }
                if (code < 200 || code >= 300) {
                    throw AxeneException.fromResponse(code, res.body());
                }
                if (type.getRawClass() == Void.class) {
                    return null;
                }
                return mapper.readValue(res.body(), type);
            } catch (AxeneException e) {
                throw e;
            } catch (Exception e) {
                last = new AxeneException(0, "Axene request failed: " + e.getMessage(), null);
                if (attempt < maxRetries) {
                    sleepBackoff(attempt);
                    continue;
                }
            }
        }
        throw last != null ? last : new AxeneException(0, "Axene request failed", null);
    }

    private static void sleepBackoff(int attempt) {
        try {
            Thread.sleep((long) (250 * Math.pow(2, attempt - 1)));
        } catch (InterruptedException ie) {
            Thread.currentThread().interrupt();
        }
    }

    private static String enc(String s) {
        return URLEncoder.encode(s, StandardCharsets.UTF_8);
    }
}
