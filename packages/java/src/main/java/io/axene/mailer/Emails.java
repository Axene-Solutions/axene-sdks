package io.axene.mailer;

import com.fasterxml.jackson.databind.JavaType;

import java.time.Instant;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;

/** Send, look up, search, schedule, and inspect messages. Accessed as {@code client.emails()}. */
public final class Emails {

    private final ApiTransport transport;

    Emails(ApiTransport transport) {
        this.transport = transport;
    }

    /**
     * Send a single email.
     *
     * @param email the message to send.
     * @return the queued message id and its initial status.
     */
    public SendEmailResult send(SendEmail email) {
        return transport.request("POST", "/v1/emails/", email.toWire(), transport.type(SendEmailResult.class));
    }

    /**
     * Send up to your plan's batch limit in one call. The API accepts a bare
     * array of messages and returns a per-message result set.
     *
     * @param emails the messages to send.
     * @return the batch totals and one result per message.
     */
    public BatchResult sendBatch(List<SendEmail> emails) {
        List<Map<String, Object>> wire = new ArrayList<>();
        for (SendEmail e : emails) {
            wire.add(e.toWire());
        }
        return transport.request("POST", "/v1/emails/batch", wire, transport.type(BatchResult.class));
    }

    /**
     * Dry-run a send: check whether {@code message} would be accepted (sender
     * registered, domain verified, plan limits, account not restricted) without
     * actually sending it.
     *
     * @param message the message to validate.
     * @return the validation outcome and current quota usage.
     */
    public ValidationResult validate(SendEmail message) {
        return transport.request("POST", "/v1/emails/validate", message.toWire(), transport.type(ValidationResult.class));
    }

    /**
     * List recent emails, newest first.
     *
     * @param status optional status filter, or null.
     * @param page   zero-based page index.
     * @param limit  page size (1-100).
     * @return a page of email records.
     */
    public List<EmailRecord> list(String status, int page, int limit) {
        String qs = Query.of().add("status", status).add("page", page).add("limit", limit).build();
        return transport.request("GET", "/v1/emails/" + qs, null, transport.listType(EmailRecord.class));
    }

    /**
     * Fetch a single email with its bodies and events.
     *
     * @param id the email id.
     * @return the full email record.
     */
    public EmailDetail get(String id) {
        return transport.request("GET", "/v1/emails/" + Query.enc(id), null, transport.type(EmailDetail.class));
    }

    /**
     * List delivery, open, click, and bounce events for an email.
     *
     * @param id the email id.
     * @return the events, oldest first.
     */
    public List<EmailEvent> events(String id) {
        return transport.request("GET", "/v1/emails/" + Query.enc(id) + "/events", null, transport.listType(EmailEvent.class));
    }

    /**
     * Re-send a bounced, rejected, or failed email as a new message.
     *
     * @param id the original email id.
     * @return the new queued message.
     */
    public SendEmailResult retry(String id) {
        return transport.request("POST", "/v1/emails/" + Query.enc(id) + "/retry", null, transport.type(SendEmailResult.class));
    }

    /**
     * Search emails. {@code q} supports inline tokens ({@code to:}, {@code from:},
     * {@code status:}, {@code domain:}, {@code tag:}); leftover words match as free text.
     *
     * @param q      query string, or null.
     * @param status optional status filter, or null.
     * @param tag    optional tag filter, or null.
     * @param page   zero-based page index.
     * @param limit  page size.
     * @return the matching search hits.
     */
    public List<EmailSearchHit> search(String q, String status, String tag, int page, int limit) {
        String qs = Query.of().add("q", q).add("status", status).add("tag", tag)
                .add("page", page).add("limit", limit).build();
        return transport.request("GET", "/v1/emails/search" + qs, null, transport.listType(EmailSearchHit.class));
    }

    /**
     * List emails scheduled for future delivery, soonest first.
     *
     * @return the scheduled emails.
     */
    public List<ScheduledEmail> listScheduled() {
        return transport.request("GET", "/v1/emails/scheduled", null, transport.listType(ScheduledEmail.class));
    }

    /**
     * Cancel a scheduled email.
     *
     * @param id the scheduled email id.
     * @return the id and its new {@code cancelled} status.
     */
    public StatusResult cancelScheduled(String id) {
        return transport.request("DELETE", "/v1/emails/scheduled/" + Query.enc(id), null, transport.type(StatusResult.class));
    }

    /**
     * Send a scheduled email immediately instead of waiting.
     *
     * @param id the scheduled email id.
     * @return the id and its new {@code queued} status.
     */
    public StatusResult sendScheduledNow(String id) {
        return transport.request("POST", "/v1/emails/scheduled/" + Query.enc(id) + "/send-now", null, transport.type(StatusResult.class));
    }

    /**
     * Poll for emails whose status changed at or after {@code since}. Capped at
     * 50 rows; use for live status updates.
     *
     * @param since required ISO 8601 instant.
     * @return the changed email records.
     */
    public List<EmailRecord> updates(Instant since) {
        String qs = Query.of().add("since", since.toString()).build();
        return transport.request("GET", "/v1/emails/updates" + qs, null, transport.listType(EmailRecord.class));
    }

    /**
     * Get the caller's saved searches.
     *
     * @return the saved searches as opaque maps.
     */
    @SuppressWarnings("unchecked")
    public List<Map<String, Object>> getSavedSearches() {
        JavaType type = transport.mapper().getTypeFactory().constructMapType(
                java.util.LinkedHashMap.class, String.class, Object.class);
        Map<String, Object> r = transport.request("GET", "/v1/emails/saved-searches", null, type);
        Object searches = r.get("searches");
        return searches == null ? new ArrayList<>() : (List<Map<String, Object>>) searches;
    }

    /**
     * Replace the caller's saved searches (max 50).
     *
     * @param searches the saved searches to store.
     * @return the normalized saved searches.
     */
    @SuppressWarnings("unchecked")
    public List<Map<String, Object>> setSavedSearches(List<Map<String, Object>> searches) {
        Map<String, Object> body = Wire.map();
        body.put("searches", searches);
        JavaType type = transport.mapper().getTypeFactory().constructMapType(
                java.util.LinkedHashMap.class, String.class, Object.class);
        Map<String, Object> r = transport.request("PUT", "/v1/emails/saved-searches", body, type);
        Object out = r.get("searches");
        return out == null ? new ArrayList<>() : (List<Map<String, Object>>) out;
    }
}
