package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;

/** Result of a send. */
@JsonIgnoreProperties(ignoreUnknown = true)
public final class SendEmailResult {
    public String id;
    public String status;
    @JsonProperty("message_id") public String messageId;
    @JsonProperty("rejection_reason") public String rejectionReason;
}
