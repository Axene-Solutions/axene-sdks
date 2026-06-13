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

    /// <summary>Result of an address validation.</summary>
    public sealed class ValidationResult
    {
        /// <summary>The address that was checked.</summary>
        [JsonPropertyName("email")] public string Email { get; set; } = "";
        /// <summary>Whether the address is well-formed and deliverable.</summary>
        [JsonPropertyName("valid")] public bool Valid { get; set; }
        /// <summary>Reason the address is invalid, if it is.</summary>
        [JsonPropertyName("reason")] public string? Reason { get; set; }
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
