package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;

import java.util.List;

/** A configured webhook endpoint. {@code secret} is returned in plaintext on every read. */
@JsonIgnoreProperties(ignoreUnknown = true)
public final class Webhook {
    public String id;
    public String url;
    public List<String> events;
    public String secret;
    @JsonProperty("is_active") public boolean isActive;
    @JsonProperty("created_at") public String createdAt;
}
