package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;

import java.util.List;
import java.util.Map;

/** Result of {@code domains.diagnose}. Issue shapes vary and are treated as opaque maps. */
@JsonIgnoreProperties(ignoreUnknown = true)
public final class DomainDiagnosis {
    public String domain;
    public List<Map<String, Object>> issues;
    @JsonProperty("health_score") public int healthScore;
}
