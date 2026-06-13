package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import java.util.List;

/** Result of a batch send. */
@JsonIgnoreProperties(ignoreUnknown = true)
public final class BatchResult {
    /** Number of messages submitted. */
    public int total;
    /** Number accepted for delivery. */
    public int sent;
    /** Number rejected. */
    public int failed;
    /** One result per submitted message, in order. */
    public List<SendEmailResult> results;
}
