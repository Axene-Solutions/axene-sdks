package io.axene.mailer;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

/**
 * Parameters for a templated bulk send to a contact list. Subject/html/text may
 * use {@code {{email}}}, {@code {{name}}}, and {@code {{metadata_key}}}
 * placeholders. Build with {@link #builder()}.
 */
public final class BulkSendParams {
    private final String senderAddressId;
    private final String subject;
    private final String html;
    private final String text;
    private final List<String> tags;

    private BulkSendParams(Builder b) {
        this.senderAddressId = b.senderAddressId;
        this.subject = b.subject;
        this.html = b.html;
        this.text = b.text;
        this.tags = b.tags;
    }

    public static Builder builder() {
        return new Builder();
    }

    /**
     * Build the JSON wire body. The caller injects {@code contact_list_id}; here
     * we map {@code senderAddressId} to {@code sender_address_id}.
     */
    Map<String, Object> toWire(String listId) {
        if (senderAddressId == null) throw new IllegalStateException("senderAddressId is required");
        if (subject == null) throw new IllegalStateException("subject is required");
        Map<String, Object> m = Wire.map();
        m.put("contact_list_id", listId);
        m.put("sender_address_id", senderAddressId);
        m.put("subject", subject);
        Wire.putIfNotNull(m, "html", html);
        Wire.putIfNotNull(m, "text", text);
        Wire.putIfNotNull(m, "tags", tags);
        return m;
    }

    /** Fluent builder for {@link BulkSendParams}. */
    public static final class Builder {
        private String senderAddressId;
        private String subject;
        private String html;
        private String text;
        private List<String> tags;

        public Builder senderAddressId(String id) { this.senderAddressId = id; return this; }
        public Builder subject(String subject) { this.subject = subject; return this; }
        public Builder html(String html) { this.html = html; return this; }
        public Builder text(String text) { this.text = text; return this; }
        public Builder tag(String tag) { (tags = tags == null ? new ArrayList<>() : tags).add(tag); return this; }
        public Builder tags(List<String> tags) { this.tags = tags; return this; }

        public BulkSendParams build() { return new BulkSendParams(this); }
    }
}
