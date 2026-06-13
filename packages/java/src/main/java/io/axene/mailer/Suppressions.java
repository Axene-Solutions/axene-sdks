package io.axene.mailer;

import com.fasterxml.jackson.databind.JavaType;

import java.util.Map;

/** Manage the do-not-send suppression list. Accessed as {@code client.suppressions()}. */
public final class Suppressions {

    private final ApiTransport transport;

    Suppressions(ApiTransport transport) {
        this.transport = transport;
    }

    /**
     * List suppressed addresses (paginated envelope; zero-based {@code page}).
     *
     * @param page   zero-based page index.
     * @param limit  page size (1-200).
     * @param search optional search filter, or null.
     * @return a page of suppressions.
     */
    public Page<Suppression> list(int page, int limit, String search) {
        String qs = Query.of().add("page", page).add("limit", limit).add("search", search).build();
        JavaType type = transport.mapper().getTypeFactory()
                .constructParametricType(Page.class, Suppression.class);
        return transport.request("GET", "/v1/suppressions" + qs, null, type);
    }

    /**
     * Suppress a single address. The {@code email} value is sent as the wire
     * field {@code email_address}.
     *
     * @param email  the address to suppress.
     * @param reason the suppression reason (defaults to {@code manual} when null).
     * @return the created suppression.
     */
    public Suppression add(String email, String reason) {
        Map<String, Object> body = Wire.map();
        body.put("email_address", email);
        body.put("reason", reason == null ? "manual" : reason);
        return transport.request("POST", "/v1/suppressions", body, transport.type(Suppression.class));
    }

    /**
     * Bulk-import suppressions from a file (one email per line).
     *
     * @param file     the raw file bytes.
     * @param filename the filename to advertise.
     * @return the bulk-import result.
     */
    public BulkSuppressionResult bulkUpload(byte[] file, String filename) {
        return transport.upload("/v1/suppressions/bulk", file, filename, transport.type(BulkSuppressionResult.class));
    }

    /**
     * Remove an address from the suppression list.
     *
     * @param id the suppression id.
     */
    public void remove(String id) {
        transport.request("DELETE", "/v1/suppressions/" + Query.enc(id), null, transport.type(Void.class));
    }
}
