package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;

/** A suppressed recipient address on the do-not-send list. */
@JsonIgnoreProperties(ignoreUnknown = true)
public final class Suppression {
    public String id;
    /** The suppressed address; the wire name is {@code email_address}. */
    @JsonProperty("email_address") public String emailAddress;
    public String reason;
    @JsonProperty("created_at") public String createdAt;
}
