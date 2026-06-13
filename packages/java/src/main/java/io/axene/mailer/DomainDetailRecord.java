package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;

import java.util.List;

/** A sending domain with its DKIM selector and the DNS records to publish. */
@JsonIgnoreProperties(ignoreUnknown = true)
public final class DomainDetailRecord {
    public String id;
    public String name;
    public String status;
    @JsonProperty("dkim_selector") public String dkimSelector;
    @JsonProperty("verified_at") public String verifiedAt;
    @JsonProperty("created_at") public String createdAt;
    @JsonProperty("dns_records") public List<DnsRecord> dnsRecords;
    @JsonProperty("platform_warning") public String platformWarning;
}
