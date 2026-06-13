package io.axene.mailer;

import java.util.Map;

/**
 * Parameters for creating or updating a template. For updates only the fields
 * you set are sent (partial update). {@code html} maps to {@code html_body} and
 * {@code text} maps to {@code text_body} on the wire. Build with {@link #builder()}.
 */
public final class TemplateParams {
    private final String name;
    private final String subject;
    private final String html;
    private final String text;
    private final Map<String, Object> blocksJson;

    private TemplateParams(Builder b) {
        this.name = b.name;
        this.subject = b.subject;
        this.html = b.html;
        this.text = b.text;
        this.blocksJson = b.blocksJson;
    }

    public static Builder builder() {
        return new Builder();
    }

    /** Build the JSON wire body, mapping html/text to html_body/text_body and omitting unset fields. */
    Map<String, Object> toWire() {
        Map<String, Object> m = Wire.map();
        Wire.putIfNotNull(m, "name", name);
        Wire.putIfNotNull(m, "subject", subject);
        Wire.putIfNotNull(m, "html_body", html);
        Wire.putIfNotNull(m, "text_body", text);
        Wire.putIfNotNull(m, "blocks_json", blocksJson);
        return m;
    }

    /** Fluent builder for {@link TemplateParams}. */
    public static final class Builder {
        private String name;
        private String subject;
        private String html;
        private String text;
        private Map<String, Object> blocksJson;

        public Builder name(String name) { this.name = name; return this; }
        public Builder subject(String subject) { this.subject = subject; return this; }
        public Builder html(String html) { this.html = html; return this; }
        public Builder text(String text) { this.text = text; return this; }
        public Builder blocksJson(Map<String, Object> blocksJson) { this.blocksJson = blocksJson; return this; }

        public TemplateParams build() { return new TemplateParams(this); }
    }
}
