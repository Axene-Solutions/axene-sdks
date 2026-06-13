package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;

import java.util.Map;

/** A delivery, open, click, or bounce event for a message. */
@JsonIgnoreProperties(ignoreUnknown = true)
public final class EmailEvent {
    public String id;
    /** The kind of event (for example {@code delivered}, {@code opened}, {@code clicked}). */
    @JsonProperty("event_type") public String eventType;
    /** Event metadata; the wire name is {@code metadata}. */
    public Map<String, Object> metadata;
    @JsonProperty("created_at") public String createdAt;
}
