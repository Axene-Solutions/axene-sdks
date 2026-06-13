package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;

import java.util.List;
import java.util.Map;

/** A reusable email template. {@code variables} is derived server-side and read-only. */
@JsonIgnoreProperties(ignoreUnknown = true)
public final class Template {
    public String id;
    public String name;
    public String subject;
    @JsonProperty("html_body") public String htmlBody;
    @JsonProperty("text_body") public String textBody;
    /** Placeholders the server derived from {@code {{word}}} tokens in the bodies. */
    public List<String> variables;
    @JsonProperty("blocks_json") public Map<String, Object> blocksJson;
    @JsonProperty("created_at") public String createdAt;
    @JsonProperty("updated_at") public String updatedAt;
}
