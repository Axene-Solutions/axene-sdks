package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;

import java.util.List;
import java.util.Map;

/** A stored email with its bodies, headers, and event history. Returned by {@code emails.get}. */
@JsonIgnoreProperties(ignoreUnknown = true)
public final class EmailDetail extends EmailRecord {
    @JsonProperty("cc_addresses") public List<String> ccAddresses;
    @JsonProperty("bcc_addresses") public List<String> bccAddresses;
    @JsonProperty("text_body") public String textBody;
    @JsonProperty("html_body") public String htmlBody;
    public Map<String, Object> headers;
    @JsonProperty("message_id") public String messageId;
    /** Delivery, open, click, and bounce events for this message. */
    public List<EmailEvent> events;
}
