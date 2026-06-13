package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;

import java.util.List;

/** Result of {@code contacts.bulkSend}. */
@JsonIgnoreProperties(ignoreUnknown = true)
public final class BulkSendResult {
    public int queued;
    public int skipped;
    public List<String> errors;
}
