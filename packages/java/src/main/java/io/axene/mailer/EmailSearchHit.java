package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;

import java.util.List;

/** A single search hit from {@code emails.search}. */
@JsonIgnoreProperties(ignoreUnknown = true)
public final class EmailSearchHit {
    public String id;
    public String status;
    public String subject;
    public String source;
    @JsonProperty("from_address") public String fromAddress;
    @JsonProperty("to_addresses") public List<String> toAddresses;
    public List<String> tags;
    @JsonProperty("created_at") public String createdAt;
    @JsonProperty("delivered_at") public String deliveredAt;
}
