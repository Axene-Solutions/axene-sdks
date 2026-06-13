package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;

import java.util.List;

/** An email scheduled for future delivery, from {@code emails.listScheduled}. */
@JsonIgnoreProperties(ignoreUnknown = true)
public final class ScheduledEmail {
    public String id;
    public String status;
    public String subject;
    @JsonProperty("from_address") public String fromAddress;
    @JsonProperty("to_addresses") public List<String> toAddresses;
    public List<String> tags;
    @JsonProperty("scheduled_at") public String scheduledAt;
    /** Seconds remaining until this email is sent. */
    @JsonProperty("seconds_until_send") public Integer secondsUntilSend;
    @JsonProperty("created_at") public String createdAt;
}
