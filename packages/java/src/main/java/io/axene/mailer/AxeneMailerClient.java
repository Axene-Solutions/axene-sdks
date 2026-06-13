package io.axene.mailer;

import java.time.Duration;
import java.util.List;

/**
 * Official Java client for Axene Mailer.
 *
 * <p>Resources are exposed as accessor methods that share a single transport:
 * {@code client.emails()}, {@code client.domains()}, {@code client.contacts()},
 * {@code client.suppressions()}, {@code client.templates()},
 * {@code client.webhooks()}.
 *
 * <pre>{@code
 * AxeneMailerClient axene = new AxeneMailerClient("axm_k_your_api_key");
 * SendEmailResult res = axene.emails().send(SendEmail.builder()
 *     .from("hello@yourdomain.com", "Your Shop")
 *     .to("customer@example.com")
 *     .subject("Your receipt")
 *     .html("<p>Thanks for your order.</p>")
 *     .build());
 * }</pre>
 */
public final class AxeneMailerClient {

    private static final String DEFAULT_BASE = "https://mail.axene.io";

    private final Emails emails;
    private final Domains domains;
    private final Contacts contacts;
    private final Suppressions suppressions;
    private final Templates templates;
    private final Webhooks webhooks;

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
        String base = (baseUrl == null ? DEFAULT_BASE : baseUrl).replaceAll("/+$", "");
        ApiTransport transport = new ApiTransport(apiKey, base, Math.max(1, maxRetries), Duration.ofSeconds(30));
        this.emails = new Emails(transport);
        this.domains = new Domains(transport);
        this.contacts = new Contacts(transport);
        this.suppressions = new Suppressions(transport);
        this.templates = new Templates(transport);
        this.webhooks = new Webhooks(transport);
    }

    /** Send, search, schedule, and inspect emails. */
    public Emails emails() {
        return emails;
    }

    /** Register, verify, and transfer sending domains. */
    public Domains domains() {
        return domains;
    }

    /** Manage subscriber lists, contacts, and bulk sends. */
    public Contacts contacts() {
        return contacts;
    }

    /** Manage the do-not-send suppression list. */
    public Suppressions suppressions() {
        return suppressions;
    }

    /** Manage reusable email templates. */
    public Templates templates() {
        return templates;
    }

    /** Manage event webhooks and inspect deliveries. */
    public Webhooks webhooks() {
        return webhooks;
    }

    // -- convenience shortcuts (delegate to resources) ---------------------

    /** Shortcut for {@code emails().send(...)}. */
    public SendEmailResult send(SendEmail email) {
        return emails.send(email);
    }

    /** Shortcut for {@code emails().sendBatch(...)}. */
    public BatchResult sendBatch(List<SendEmail> emails) {
        return this.emails.sendBatch(emails);
    }

    /** Shortcut for {@code emails().validate(...)}. */
    public ValidationResult validate(SendEmail message) {
        return emails.validate(message);
    }

    /** Shortcut for {@code domains().list()}. */
    public List<DomainRecord> listDomains() {
        return domains.list();
    }
}
