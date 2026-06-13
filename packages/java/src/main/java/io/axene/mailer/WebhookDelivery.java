package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;

/** A summary of one webhook delivery attempt. */
@JsonIgnoreProperties(ignoreUnknown = true)
public class WebhookDelivery {
    public String id;
    @JsonProperty("webhook_id") public String webhookId;
    @JsonProperty("event_type") public String eventType;
    public String status;
    @JsonProperty("response_status") public Integer responseStatus;
    public int attempt;
    @JsonProperty("next_retry_at") public String nextRetryAt;
    @JsonProperty("created_at") public String createdAt;
}
