package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import java.util.List;

/** Result of a batch send. */
@JsonIgnoreProperties(ignoreUnknown = true)
public final class BatchResult {
    public List<SendEmailResult> results;
}
