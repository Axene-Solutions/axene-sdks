package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;

/** A sending domain and its verification status. */
@JsonIgnoreProperties(ignoreUnknown = true)
public final class DomainRecord {
    public String id;
    public String name;
    public String status;
    @JsonProperty("created_at") public String createdAt;
    @JsonProperty("platform_warning") public String platformWarning;
}
