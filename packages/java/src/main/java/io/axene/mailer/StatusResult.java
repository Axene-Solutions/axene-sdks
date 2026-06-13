package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;

/** A minimal {@code {id, status}} response, returned by scheduled-email actions. */
@JsonIgnoreProperties(ignoreUnknown = true)
public final class StatusResult {
    public String id;
    public String status;
}
