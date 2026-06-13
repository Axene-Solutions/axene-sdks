using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Axene.Mailer
{
    /// <summary>Result of a send: the queued message id and its initial status.</summary>
    public sealed class SendEmailResult
    {
        /// <summary>The message id.</summary>
        [JsonPropertyName("id")] public string Id { get; set; } = "";
        /// <summary>Initial status (for example <c>queued</c> or <c>scheduled</c>).</summary>
        [JsonPropertyName("status")] public string Status { get; set; } = "";
        /// <summary>RFC 5322 Message-ID, once assigned.</summary>
        [JsonPropertyName("message_id")] public string? MessageId { get; set; }
        /// <summary>Reason the message was rejected, if it was.</summary>
        [JsonPropertyName("rejection_reason")] public string? RejectionReason { get; set; }
    }

    /// <summary>Result of a batch send.</summary>
    public sealed class BatchResult
    {
        /// <summary>Number of messages submitted.</summary>
        [JsonPropertyName("total")] public int Total { get; set; }
        /// <summary>Number accepted for delivery.</summary>
        [JsonPropertyName("sent")] public int Sent { get; set; }
        /// <summary>Number rejected.</summary>
        [JsonPropertyName("failed")] public int Failed { get; set; }
        /// <summary>One result per submitted message, in order.</summary>
        [JsonPropertyName("results")] public List<SendEmailResult> Results { get; set; } = new List<SendEmailResult>();
    }

    /// <summary>A stored email and its current status.</summary>
    public sealed class EmailRecord
    {
        /// <summary>The message id.</summary>
        [JsonPropertyName("id")] public string Id { get; set; } = "";
        /// <summary>Current status.</summary>
        [JsonPropertyName("status")] public string Status { get; set; } = "";
        /// <summary>Subject line.</summary>
        [JsonPropertyName("subject")] public string? Subject { get; set; }
        /// <summary>Sender address.</summary>
        [JsonPropertyName("from_address")] public string? FromAddress { get; set; }
        /// <summary>Recipient addresses.</summary>
        [JsonPropertyName("to_addresses")] public List<string>? ToAddresses { get; set; }
        /// <summary>Creation timestamp (ISO 8601).</summary>
        [JsonPropertyName("created_at")] public string? CreatedAt { get; set; }
        /// <summary>Delivery timestamp (ISO 8601), if delivered.</summary>
        [JsonPropertyName("delivered_at")] public string? DeliveredAt { get; set; }
    }

    /// <summary>A single reason a message would not send.</summary>
    public sealed class ValidationIssue
    {
        /// <summary>The offending field (for example <c>from</c> or <c>account</c>).</summary>
        [JsonPropertyName("field")] public string Field { get; set; } = "";
        /// <summary>A human-readable description of the problem.</summary>
        [JsonPropertyName("error")] public string Error { get; set; } = "";
    }

    /// <summary>Sending-quota usage returned alongside a validation.</summary>
    public sealed class ValidationUsage
    {
        /// <summary>Messages sent today.</summary>
        [JsonPropertyName("daily")] public int Daily { get; set; }
        /// <summary>Daily send limit on the current plan.</summary>
        [JsonPropertyName("daily_limit")] public int DailyLimit { get; set; }
        /// <summary>Messages sent this month.</summary>
        [JsonPropertyName("monthly")] public int Monthly { get; set; }
        /// <summary>Monthly send limit on the current plan.</summary>
        [JsonPropertyName("monthly_limit")] public int MonthlyLimit { get; set; }
    }

    /// <summary>
    /// Result of a dry-run validation: whether a message would send (sender
    /// registered, domain verified, plan limits, restrictions) without sending it.
    /// </summary>
    public sealed class ValidationResult
    {
        /// <summary>True when there are no blocking issues.</summary>
        [JsonPropertyName("valid")] public bool Valid { get; set; }
        /// <summary>True when the message can be sent right now.</summary>
        [JsonPropertyName("can_send")] public bool CanSend { get; set; }
        /// <summary>Blocking issues, if any.</summary>
        [JsonPropertyName("issues")] public List<ValidationIssue> Issues { get; set; } = new List<ValidationIssue>();
        /// <summary>The account's current plan tier.</summary>
        [JsonPropertyName("plan")] public string Plan { get; set; } = "";
        /// <summary>Current send-quota usage.</summary>
        [JsonPropertyName("usage")] public ValidationUsage? Usage { get; set; }
    }

    /// <summary>A sending domain and its verification status.</summary>
    public sealed class DomainRecord
    {
        /// <summary>The domain id.</summary>
        [JsonPropertyName("id")] public string Id { get; set; } = "";
        /// <summary>The domain name.</summary>
        [JsonPropertyName("name")] public string Name { get; set; } = "";
        /// <summary>Verification status (for example <c>verified</c>).</summary>
        [JsonPropertyName("status")] public string Status { get; set; } = "";
        /// <summary>Creation timestamp (ISO 8601).</summary>
        [JsonPropertyName("created_at")] public string? CreatedAt { get; set; }
        /// <summary>A deliverability or configuration warning, if any.</summary>
        [JsonPropertyName("platform_warning")] public string? PlatformWarning { get; set; }
    }
}
