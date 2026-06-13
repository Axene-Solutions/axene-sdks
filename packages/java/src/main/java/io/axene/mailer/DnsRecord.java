package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;

/** A DNS record the API expects you to publish for a domain. */
@JsonIgnoreProperties(ignoreUnknown = true)
public final class DnsRecord {
    public String id;
    @JsonProperty("record_type") public String recordType;
    public String purpose;
    public String host;
    public String value;
    @JsonProperty("is_verified") public boolean isVerified;
    @JsonProperty("last_checked_at") public String lastCheckedAt;
}
