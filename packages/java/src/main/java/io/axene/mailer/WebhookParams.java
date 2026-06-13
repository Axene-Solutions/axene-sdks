package io.axene.mailer;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

/**
 * Parameters for creating or updating a webhook. For updates only the fields you
 * set are sent (partial update); {@code isActive} maps to {@code is_active} on
 * the wire. Build with {@link #builder()}.
 */
public final class WebhookParams {
    private final String url;
    private final List<String> events;
    private final Boolean isActive;

    private WebhookParams(Builder b) {
        this.url = b.url;
        this.events = b.events;
        this.isActive = b.isActive;
    }

    public static Builder builder() {
        return new Builder();
    }

    /** Build the JSON wire body, mapping {@code isActive} to {@code is_active} and omitting unset fields. */
    Map<String, Object> toWire() {
        Map<String, Object> m = Wire.map();
        Wire.putIfNotNull(m, "url", url);
        Wire.putIfNotNull(m, "events", events);
        Wire.putIfNotNull(m, "is_active", isActive);
        return m;
    }

    /** Fluent builder for {@link WebhookParams}. */
    public static final class Builder {
        private String url;
        private List<String> events;
        private Boolean isActive;

        public Builder url(String url) { this.url = url; return this; }
        public Builder event(String event) { (events = events == null ? new ArrayList<>() : events).add(event); return this; }
        public Builder events(List<String> events) { this.events = events; return this; }
        public Builder isActive(boolean isActive) { this.isActive = isActive; return this; }

        public WebhookParams build() { return new WebhookParams(this); }
    }
}
