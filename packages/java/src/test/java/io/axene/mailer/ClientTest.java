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
    @Test
    void send_batch_posts_bare_array() {
        responseBody = "{\"total\":1,\"sent\":1,\"failed\":0,\"results\":[{\"id\":\"a\",\"status\":\"queued\"}]}";
        statusScript = new int[] { 202 };
        BatchResult r = client().sendBatch(java.util.List.of(
                SendEmail.builder().from("f@x.co").to("a@x.co").subject("s").build()));
        assertEquals(1, r.total);
        assertEquals("a", r.results.get(0).id);
        String body = bodies.get(0);
        assertTrue(body.trim().startsWith("["), "batch body must be a bare array, not an object");
        assertTrue(body.contains("\"from_\""));
    }

    @Test
    void validate_sends_full_body_and_parses() {
        responseBody = "{\"valid\":true,\"can_send\":true,\"issues\":[],\"plan\":\"starter\","
                + "\"usage\":{\"daily\":1,\"daily_limit\":100,\"monthly\":2,\"monthly_limit\":1000}}";
        statusScript = new int[] { 200 };
        ValidationResult v = client().emails().validate(SendEmail.builder()
                .from("hello@shop.co", "Shop")
                .to("a@example.com")
                .subject("Hi")
                .html("<p>x</p>")
                .tag("welcome")
                .build());
        assertTrue(v.valid);
        assertTrue(v.canSend);
        assertEquals("starter", v.plan);
        assertEquals(100, v.usage.dailyLimit);
        assertEquals("/v1/emails/validate", paths.get(0));
        String body = bodies.get(0);
        assertTrue(body.contains("\"from_\""), "validate must send the full send body with from_");
        assertTrue(body.contains("\"subject\":\"Hi\""));
        assertTrue(body.contains("\"tags\":[\"welcome\"]"));
    }

    @Test
    void upload_csv_sends_multipart_with_file_field() {
        responseBody = "{\"imported\":3,\"skipped\":1,\"errors\":[]}";
        statusScript = new int[] { 200 };
        byte[] csv = "email,name\na@x.co,A\n".getBytes(StandardCharsets.UTF_8);
        CsvImportResult r = client().contacts().uploadCsv("list_1", csv, "contacts.csv");
        assertEquals(3, r.imported);
        assertEquals(1, r.skipped);
        assertEquals("/v1/contacts/list_1/upload", paths.get(0));
        assertEquals("Bearer axm_k_test", auths.get(0));
        String body = bodies.get(0);
        assertTrue(body.contains("----axene"), "multipart body must carry the boundary marker");
        assertTrue(body.contains("name=\"file\""), "the single multipart field must be named file");
        assertTrue(body.contains("filename=\"contacts.csv\""));
        assertTrue(body.contains("a@x.co"), "the file contents must be in the body");
    }

    @Test
    void suppressions_list_parses_envelope() {
        responseBody = "{\"items\":[{\"id\":\"s1\",\"email_address\":\"bad@x.co\",\"reason\":\"bounce\","
                + "\"created_at\":\"2026-01-01\"}],\"total\":1,\"page\":0,\"limit\":50}";
        statusScript = new int[] { 200 };
        Page<Suppression> page = client().suppressions().list(0, 50, null);
        assertEquals(1, page.total);
        assertEquals(0, page.page);
        assertEquals(50, page.limit);
        assertEquals(1, page.items.size());
        assertEquals("bad@x.co", page.items.get(0).emailAddress);
        assertEquals("bounce", page.items.get(0).reason);
        assertTrue(paths.get(0).startsWith("/v1/suppressions"));
    }

    @Test
    void templates_create_maps_html_to_html_body() {
        responseBody = "{\"id\":\"t1\",\"name\":\"Welcome\",\"html_body\":\"<p>hi</p>\","
                + "\"text_body\":\"hi\",\"created_at\":\"2026-01-01\",\"updated_at\":\"2026-01-01\"}";
        statusScript = new int[] { 201 };
        Template t = client().templates().create(TemplateParams.builder()
                .name("Welcome")
                .html("<p>hi</p>")
                .text("hi")
                .build());
        assertEquals("t1", t.id);
        assertEquals("<p>hi</p>", t.htmlBody);
        assertEquals("/v1/templates/", paths.get(0));
        String body = bodies.get(0);
        assertTrue(body.contains("\"html_body\":\"<p>hi</p>\""), "html must map to html_body");
        assertTrue(body.contains("\"text_body\":\"hi\""), "text must map to text_body");
        assertFalse(body.contains("\"html\":"), "raw html must not be sent for templates");
    }

    @Test
    void webhooks_update_maps_is_active() {
        responseBody = "{\"id\":\"w1\",\"url\":\"https://h.co/x\",\"events\":[\"email.delivered\"],"
                + "\"secret\":\"sec\",\"is_active\":false,\"created_at\":\"2026-01-01\"}";
        statusScript = new int[] { 200 };
        Webhook w = client().webhooks().update("w1", WebhookParams.builder()
                .isActive(false)
                .build());
        assertEquals("w1", w.id);
        assertFalse(w.isActive);
        assertEquals("/v1/webhooks/w1", paths.get(0));
        String body = bodies.get(0);
        assertTrue(body.contains("\"is_active\":false"), "isActive must map to is_active");
        assertFalse(body.contains("\"isActive\""), "camelCase isActive must not be sent");
    }
}
