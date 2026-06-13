using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Axene.Mailer
{
    /// <summary>A DNS record the API expects you to publish for a domain.</summary>
    public sealed class DnsRecord
    {
        /// <summary>The record id.</summary>
        [JsonPropertyName("id")] public string Id { get; set; } = "";
        /// <summary>The DNS record type (for example <c>TXT</c>, <c>CNAME</c>, <c>MX</c>).</summary>
        [JsonPropertyName("record_type")] public string RecordType { get; set; } = "";
        /// <summary>What the record is for (for example DKIM, SPF, DMARC).</summary>
        [JsonPropertyName("purpose")] public string Purpose { get; set; } = "";
        /// <summary>The host / name to publish the record at.</summary>
        [JsonPropertyName("host")] public string Host { get; set; } = "";
        /// <summary>The record value to publish.</summary>
        [JsonPropertyName("value")] public string Value { get; set; } = "";
        /// <summary>Whether the record has been observed in public DNS.</summary>
        [JsonPropertyName("is_verified")] public bool IsVerified { get; set; }
        /// <summary>When the record was last checked (ISO 8601).</summary>
        [JsonPropertyName("last_checked_at")] public string? LastCheckedAt { get; set; }
    }

    /// <summary>A sending domain with its DKIM selector and DNS records.</summary>
    public sealed class DomainDetail
    {
        /// <summary>The domain id.</summary>
        [JsonPropertyName("id")] public string Id { get; set; } = "";
        /// <summary>The domain name.</summary>
        [JsonPropertyName("name")] public string Name { get; set; } = "";
        /// <summary>Verification status.</summary>
        [JsonPropertyName("status")] public string Status { get; set; } = "";
        /// <summary>The DKIM selector used for this domain.</summary>
        [JsonPropertyName("dkim_selector")] public string? DkimSelector { get; set; }
        /// <summary>When the domain was verified (ISO 8601), if it is.</summary>
        [JsonPropertyName("verified_at")] public string? VerifiedAt { get; set; }
        /// <summary>Creation timestamp (ISO 8601).</summary>
        [JsonPropertyName("created_at")] public string? CreatedAt { get; set; }
        /// <summary>The DNS records to publish for this domain.</summary>
        [JsonPropertyName("dns_records")] public List<DnsRecord> DnsRecords { get; set; } = new List<DnsRecord>();
        /// <summary>A deliverability or configuration warning, if any.</summary>
        [JsonPropertyName("platform_warning")] public string? PlatformWarning { get; set; }
    }

    /// <summary>A single DNS record reference within a health check.</summary>
    public sealed class DomainHealthRecord
    {
        /// <summary>The record type.</summary>
        [JsonPropertyName("type")] public string? Type { get; set; }
        /// <summary>The host the record is published at.</summary>
        [JsonPropertyName("host")] public string? Host { get; set; }
        /// <summary>The record value.</summary>
        [JsonPropertyName("value")] public string? Value { get; set; }
    }

    /// <summary>One row of a domain health report.</summary>
    public sealed class DomainHealthCheck
    {
        /// <summary>A stable key identifying the check.</summary>
        [JsonPropertyName("key")] public string Key { get; set; } = "";
        /// <summary>A human-readable label for the check.</summary>
        [JsonPropertyName("label")] public string Label { get; set; } = "";
        /// <summary>The check outcome: <c>ok</c>, <c>warn</c>, <c>error</c>, or <c>info</c>.</summary>
        [JsonPropertyName("status")] public string Status { get; set; } = "";
        /// <summary>A description of the current state.</summary>
        [JsonPropertyName("detail")] public string? Detail { get; set; }
        /// <summary>A suggested fix, when applicable.</summary>
        [JsonPropertyName("recommendation")] public string? Recommendation { get; set; }
        /// <summary>The DNS record the check refers to, if any.</summary>
        [JsonPropertyName("record")] public DomainHealthRecord? Record { get; set; }
    }

    /// <summary>A tally of health-check outcomes by severity.</summary>
    public sealed class DomainHealthSummary
    {
        /// <summary>Number of checks that passed.</summary>
        [JsonPropertyName("ok")] public int Ok { get; set; }
        /// <summary>Number of checks that warned.</summary>
        [JsonPropertyName("warn")] public int Warn { get; set; }
        /// <summary>Number of checks that errored.</summary>
        [JsonPropertyName("error")] public int Error { get; set; }
        /// <summary>Number of informational checks.</summary>
        [JsonPropertyName("info")] public int Info { get; set; }
    }

    /// <summary>Result of a domain health check: per-record checks plus a summary tally.</summary>
    public sealed class DomainHealth
    {
        /// <summary>The domain name.</summary>
        [JsonPropertyName("domain")] public string Domain { get; set; } = "";
        /// <summary>The individual checks.</summary>
        [JsonPropertyName("checks")] public List<DomainHealthCheck> Checks { get; set; } = new List<DomainHealthCheck>();
        /// <summary>A tally of outcomes by severity.</summary>
        [JsonPropertyName("summary")] public DomainHealthSummary? Summary { get; set; }
    }

    /// <summary>Result of a domain diagnosis. Issue shapes vary; treated as opaque.</summary>
    public sealed class DomainDiagnosis
    {
        /// <summary>The domain name.</summary>
        [JsonPropertyName("domain")] public string Domain { get; set; } = "";
        /// <summary>The detected issues (opaque objects).</summary>
        [JsonPropertyName("issues")] public List<JsonElement> Issues { get; set; } = new List<JsonElement>();
        /// <summary>An overall health score.</summary>
        [JsonPropertyName("health_score")] public int HealthScore { get; set; }
    }

    /// <summary>Result of rotating a DKIM key: the new record plus the updated domain.</summary>
    public sealed class DkimRotation
    {
        /// <summary>The host to publish the new DKIM record at.</summary>
        [JsonPropertyName("dkim_record_host")] public string? DkimRecordHost { get; set; }
        /// <summary>The new DKIM record value to publish.</summary>
        [JsonPropertyName("dkim_record_value")] public string? DkimRecordValue { get; set; }
        /// <summary>The updated domain.</summary>
        [JsonPropertyName("domain")] public DomainDetail? Domain { get; set; }
    }

    /// <summary>A domain transfer record.</summary>
    public sealed class DomainTransfer
    {
        /// <summary>The transfer id.</summary>
        [JsonPropertyName("id")] public string Id { get; set; } = "";
        /// <summary>The id of the domain being transferred.</summary>
        [JsonPropertyName("domain_id")] public string? DomainId { get; set; }
        /// <summary>The name of the domain being transferred.</summary>
        [JsonPropertyName("domain_name")] public string? DomainName { get; set; }
        /// <summary>A label for the source account.</summary>
        [JsonPropertyName("source_label")] public string? SourceLabel { get; set; }
        /// <summary>The email of the account the domain is being transferred to.</summary>
        [JsonPropertyName("target_email")] public string? TargetEmail { get; set; }
        /// <summary>The transfer status.</summary>
        [JsonPropertyName("status")] public string Status { get; set; } = "";
        /// <summary>An optional note attached to the transfer.</summary>
        [JsonPropertyName("note")] public string? Note { get; set; }
        /// <summary>When the cool-off period ends (ISO 8601), if any.</summary>
        [JsonPropertyName("cooloff_until")] public string? CooloffUntil { get; set; }
        /// <summary>When the transfer was initiated (ISO 8601).</summary>
        [JsonPropertyName("initiated_at")] public string? InitiatedAt { get; set; }
        /// <summary>When the transfer was accepted (ISO 8601), if it was.</summary>
        [JsonPropertyName("accepted_at")] public string? AcceptedAt { get; set; }
        /// <summary>When the transfer completed (ISO 8601), if it did.</summary>
        [JsonPropertyName("completed_at")] public string? CompletedAt { get; set; }
        /// <summary>When the transfer offer expires (ISO 8601).</summary>
        [JsonPropertyName("expires_at")] public string? ExpiresAt { get; set; }
    }

    /// <summary>Result of a domain availability check.</summary>
    public sealed class DomainAvailability
    {
        /// <summary>Whether the domain can be added.</summary>
        [JsonPropertyName("available")] public bool Available { get; set; }
        /// <summary>A machine-readable reason when unavailable.</summary>
        [JsonPropertyName("reason")] public string? Reason { get; set; }
        /// <summary>A human-readable explanation.</summary>
        [JsonPropertyName("detail")] public string? Detail { get; set; }
        /// <summary>Count of stale verification tokens, if any.</summary>
        [JsonPropertyName("stale_tokens")] public int? StaleTokens { get; set; }
    }

    /// <summary>Result of checking whether a domain name already exists in your account.</summary>
    public sealed class DomainCheck
    {
        /// <summary>Whether the domain exists in your account.</summary>
        [JsonPropertyName("exists")] public bool Exists { get; set; }
        /// <summary>Whether the domain is verified.</summary>
        [JsonPropertyName("verified")] public bool Verified { get; set; }
        /// <summary>The domain status, if it exists.</summary>
        [JsonPropertyName("status")] public string? Status { get; set; }
        /// <summary>The domain name that was checked.</summary>
        [JsonPropertyName("domain")] public string Domain { get; set; } = "";
        /// <summary>The domain id, if it exists.</summary>
        [JsonPropertyName("id")] public string? Id { get; set; }
    }
}
