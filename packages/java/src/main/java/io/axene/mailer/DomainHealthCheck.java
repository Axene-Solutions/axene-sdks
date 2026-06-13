package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;

import java.util.Map;

/** One row of a domain health report. */
@JsonIgnoreProperties(ignoreUnknown = true)
public final class DomainHealthCheck {
    public String key;
    public String label;
    /** One of {@code ok}, {@code warn}, {@code error}, {@code info}. */
    public String status;
    public String detail;
    public String recommendation;
    /** The associated DNS record ({@code type}/{@code host}/{@code value}), or null. */
    public Map<String, Object> record;
}
