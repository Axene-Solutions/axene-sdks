package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;

import java.util.List;
import java.util.Map;

/** Result of {@code domains.health}: per-record checks plus a summary tally. */
@JsonIgnoreProperties(ignoreUnknown = true)
public final class DomainHealth {
    public String domain;
    public List<DomainHealthCheck> checks;
    /** Counts keyed by {@code ok}/{@code warn}/{@code error}/{@code info}. */
    public Map<String, Integer> summary;
}
