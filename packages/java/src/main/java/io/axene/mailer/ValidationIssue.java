package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;

/** A single reason a message would not send. */
@JsonIgnoreProperties(ignoreUnknown = true)
public final class ValidationIssue {
    /** The offending field (for example {@code from} or {@code account}). */
    public String field;
    /** A human-readable description of the problem. */
    public String error;
}
