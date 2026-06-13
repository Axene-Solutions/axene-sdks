package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;

/** Result of {@code domains.checkAvailability}. */
@JsonIgnoreProperties(ignoreUnknown = true)
public final class DomainAvailability {
    public boolean available;
    public String reason;
    public String detail;
    @JsonProperty("stale_tokens") public Integer staleTokens;
}
