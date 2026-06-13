package io.axene.mailer;

import com.sun.net.httpserver.HttpExchange;
import com.sun.net.httpserver.HttpServer;
import org.junit.jupiter.api.AfterEach;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;

import java.io.IOException;
import java.io.OutputStream;
import java.net.InetSocketAddress;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.atomic.AtomicInteger;

import static org.junit.jupiter.api.Assertions.*;

/** Integration-style tests against a tiny in-JVM HTTP server (no external deps). */
class ClientTest {

    private HttpServer server;
    private String baseUrl;
    private final List<String> bodies = new ArrayList<>();
    private final List<String> auths = new ArrayList<>();
    private final List<String> paths = new ArrayList<>();
    private volatile int[] statusScript = { 202 };
    private volatile String responseBody = "{\"id\":\"em_1\",\"status\":\"queued\"}";
    private final AtomicInteger call = new AtomicInteger(0);

    @BeforeEach
    void setUp() throws IOException {
        server = HttpServer.create(new InetSocketAddress("127.0.0.1", 0), 0);
        server.createContext("/", this::handle);
        server.start();
        baseUrl = "http://127.0.0.1:" + server.getAddress().getPort();
    }

    @AfterEach
    void tearDown() {
        server.stop(0);
    }

    private void handle(HttpExchange ex) throws IOException {
        paths.add(ex.getRequestURI().getPath());
        auths.add(ex.getRequestHeaders().getFirst("Authorization"));
        bodies.add(new String(ex.getRequestBody().readAllBytes(), StandardCharsets.UTF_8));
        int i = Math.min(call.getAndIncrement(), statusScript.length - 1);
        int status = statusScript[i];
        byte[] out = (status >= 200 && status < 300 ? responseBody : "{\"detail\":{\"code\":\"invalid\",\"message\":\"bad from\"}}")
                .getBytes(StandardCharsets.UTF_8);
        ex.getResponseHeaders().add("Content-Type", "application/json");
        ex.sendResponseHeaders(status, out.length);
        try (OutputStream os = ex.getResponseBody()) {
            os.write(out);
        }
    }

    private AxeneMailerClient client() {
        return new AxeneMailerClient("axm_k_test", baseUrl, 3);
    }

    @Test
    void send_maps_from_and_sets_bearer() {
        SendEmailResult res = client().send(SendEmail.builder()
                .from("hello@shop.co", "Shop")
                .to("a@example.com")
                .subject("Hi")
                .html("<p>x</p>")
                .build());

        assertEquals("em_1", res.id);
        assertEquals("/v1/emails/", paths.get(0));
        assertEquals("Bearer axm_k_test", auths.get(0));
        String body = bodies.get(0);
        assertTrue(body.contains("\"from_\""), "from must map to from_");
        assertFalse(body.contains("\"from\":"), "raw `from` must not be sent");
        assertTrue(body.contains("\"email\":\"hello@shop.co\""));
        assertTrue(body.contains("\"to\":["), "to must be an array");
        assertFalse(body.contains("\"text\""), "null fields pruned");
    }

    @Test
    void non2xx_throws_with_code() {
        statusScript = new int[] { 422 };
        AxeneException ex = assertThrows(AxeneException.class, () ->
                client().send(SendEmail.builder().from("f@x.co").to("a@x.co").subject("s").build()));
        assertEquals(422, ex.getStatus());
        assertEquals("invalid", ex.getCode());
        assertEquals("bad from", ex.getMessage());
    }

    @Test
    void retries_5xx_then_succeeds() {
        statusScript = new int[] { 503, 503, 202 };
        SendEmailResult res = client().send(SendEmail.builder().from("f@x.co").to("a@x.co").subject("s").build());
        assertEquals("em_1", res.id);
        assertEquals(3, paths.size());
    }

    @Test
    void list_domains_parses() {
        responseBody = "[{\"id\":\"d1\",\"name\":\"shop.co\",\"status\":\"verified\",\"created_at\":\"2026-01-01\"}]";
        statusScript = new int[] { 200 };
        List<DomainRecord> domains = client().listDomains();
        assertEquals(1, domains.size());
        assertEquals("shop.co", domains.get(0).name);
        assertEquals("verified", domains.get(0).status);
        assertEquals("/v1/domains/", paths.get(0));
    }
}
