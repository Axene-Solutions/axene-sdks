package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;
import java.util.List;

/** A stored email and its current status, as returned by list and lookup endpoints. */
@JsonIgnoreProperties(ignoreUnknown = true)
public class EmailRecord {
    public String id;
    public String status;
    public String subject;
    public String source;
    @JsonProperty("from_address") public String fromAddress;
    @JsonProperty("to_addresses") public List<String> toAddresses;
    @JsonProperty("opened_count") public Integer openedCount;
    @JsonProperty("clicked_count") public Integer clickedCount;
    public List<String> tags;
    @JsonProperty("scheduled_at") public String scheduledAt;
    @JsonProperty("created_at") public String createdAt;
    @JsonProperty("sent_at") public String sentAt;
    @JsonProperty("delivered_at") public String deliveredAt;
    @JsonProperty("retry_of_id") public String retryOfId;
}
