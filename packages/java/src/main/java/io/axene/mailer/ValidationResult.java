package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;

/** Result of an address validation. */
@JsonIgnoreProperties(ignoreUnknown = true)
public final class ValidationResult {
    public String email;
    public boolean valid;
    public String reason;
}
