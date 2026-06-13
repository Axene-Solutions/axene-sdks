package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;
import java.util.List;

/**
 * Result of a dry-run validation: whether a message would send (sender
 * registered, domain verified, plan limits, restrictions) without sending it.
 */
@JsonIgnoreProperties(ignoreUnknown = true)
public final class ValidationResult {
    /** True when there are no blocking issues. */
    public boolean valid;
    /** True when the message can be sent right now. */
    @JsonProperty("can_send") public boolean canSend;
    /** Blocking issues, if any. */
    public List<ValidationIssue> issues;
    /** The account's current plan tier. */
    public String plan;
    /** Current send-quota usage. */
    public ValidationUsage usage;
}
