package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;
import java.util.List;

/** A stored email and its current status. */
@JsonIgnoreProperties(ignoreUnknown = true)
public final class EmailRecord {
    public String id;
    public String status;
    public String subject;
    @JsonProperty("from_address") public String fromAddress;
    @JsonProperty("to_addresses") public List<String> toAddresses;
    @JsonProperty("created_at") public String createdAt;
    @JsonProperty("delivered_at") public String deliveredAt;
}
