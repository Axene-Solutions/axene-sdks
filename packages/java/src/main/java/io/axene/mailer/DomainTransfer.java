package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;

/** A domain transfer record returned by {@code domains.transfer}. */
@JsonIgnoreProperties(ignoreUnknown = true)
public final class DomainTransfer {
    public String id;
    @JsonProperty("domain_id") public String domainId;
    @JsonProperty("domain_name") public String domainName;
    @JsonProperty("source_user_id") public String sourceUserId;
    @JsonProperty("source_org_id") public String sourceOrgId;
    @JsonProperty("source_label") public String sourceLabel;
    @JsonProperty("target_email") public String targetEmail;
    @JsonProperty("target_user_id") public String targetUserId;
    @JsonProperty("target_org_id") public String targetOrgId;
    public String status;
    public String note;
    @JsonProperty("cooloff_until") public String cooloffUntil;
    @JsonProperty("initiated_at") public String initiatedAt;
    @JsonProperty("accepted_at") public String acceptedAt;
    @JsonProperty("completed_at") public String completedAt;
    @JsonProperty("expires_at") public String expiresAt;
}
