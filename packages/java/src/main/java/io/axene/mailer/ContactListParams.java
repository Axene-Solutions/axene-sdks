package io.axene.mailer;

import java.util.Map;

/**
 * Parameters for creating or updating a contact list. For updates only the
 * fields you set are sent (partial update). Build with {@link #builder()}.
 */
public final class ContactListParams {
    private final String name;
    private final String description;
    private final String iconSeed;

    private ContactListParams(Builder b) {
        this.name = b.name;
        this.description = b.description;
        this.iconSeed = b.iconSeed;
    }

    public static Builder builder() {
        return new Builder();
    }

    /** Build the JSON wire body, mapping {@code iconSeed} to {@code icon_seed} and omitting unset fields. */
    Map<String, Object> toWire() {
        Map<String, Object> m = Wire.map();
        Wire.putIfNotNull(m, "name", name);
        Wire.putIfNotNull(m, "description", description);
        Wire.putIfNotNull(m, "icon_seed", iconSeed);
        return m;
    }

    /** Fluent builder for {@link ContactListParams}. */
    public static final class Builder {
        private String name;
        private String description;
        private String iconSeed;

        public Builder name(String name) { this.name = name; return this; }
        public Builder description(String description) { this.description = description; return this; }
        public Builder iconSeed(String iconSeed) { this.iconSeed = iconSeed; return this; }

        public ContactListParams build() { return new ContactListParams(this); }
    }
}
