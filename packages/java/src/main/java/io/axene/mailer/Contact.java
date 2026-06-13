package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;

import java.util.Map;

/** A single contact in a list. */
@JsonIgnoreProperties(ignoreUnknown = true)
public final class Contact {
    public String id;
    public String email;
    public String name;
    /** Free-form metadata; the wire name is {@code metadata}. */
    public Map<String, Object> metadata;
    @JsonProperty("created_at") public String createdAt;
}
