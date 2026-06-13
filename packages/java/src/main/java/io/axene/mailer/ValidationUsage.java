package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;

/** Sending-quota usage returned alongside a validation. */
@JsonIgnoreProperties(ignoreUnknown = true)
public final class ValidationUsage {
    /** Messages sent today. */
    public int daily;
    /** Daily send limit on the current plan. */
    @JsonProperty("daily_limit") public int dailyLimit;
    /** Messages sent this month. */
    public int monthly;
    /** Monthly send limit on the current plan. */
    @JsonProperty("monthly_limit") public int monthlyLimit;
}
