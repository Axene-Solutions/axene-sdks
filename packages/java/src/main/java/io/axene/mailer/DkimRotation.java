package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;

/** Result of {@code domains.rotateDkim}: the new DKIM record plus the updated domain. */
@JsonIgnoreProperties(ignoreUnknown = true)
public final class DkimRotation {
    @JsonProperty("dkim_record_host") public String dkimRecordHost;
    @JsonProperty("dkim_record_value") public String dkimRecordValue;
    public DomainDetailRecord domain;
}
