package io.axene.mailer;

import java.util.Map;

/** Parameters for adding a contact to a list. Build with {@link #builder()}. */
public final class ContactParams {
    private final String email;
    private final String name;
    private final Map<String, Object> metadata;

    private ContactParams(Builder b) {
        this.email = b.email;
        this.name = b.name;
        this.metadata = b.metadata;
    }

    public static Builder builder() {
        return new Builder();
    }

    Map<String, Object> toWire() {
        if (email == null) throw new IllegalStateException("email is required");
        Map<String, Object> m = Wire.map();
        m.put("email", email);
        Wire.putIfNotNull(m, "name", name);
        Wire.putIfNotNull(m, "metadata", metadata);
        return m;
    }

    /** Fluent builder for {@link ContactParams}. */
    public static final class Builder {
        private String email;
        private String name;
        private Map<String, Object> metadata;

        public Builder email(String email) { this.email = email; return this; }
        public Builder name(String name) { this.name = name; return this; }
        public Builder metadata(Map<String, Object> metadata) { this.metadata = metadata; return this; }

        public ContactParams build() { return new ContactParams(this); }
    }
}
