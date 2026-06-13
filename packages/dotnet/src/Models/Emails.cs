using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Axene.Mailer
{
    /// <summary>A delivery / open / click / bounce event for a message.</summary>
    public sealed class EmailEvent
    {
        /// <summary>The event id.</summary>
        [JsonPropertyName("id")] public string Id { get; set; } = "";
        /// <summary>The event type (for example <c>delivered</c>, <c>opened</c>, <c>clicked</c>).</summary>
        [JsonPropertyName("event_type")] public string EventType { get; set; } = "";
        /// <summary>Free-form event metadata, if any.</summary>
        [JsonPropertyName("metadata")] public JsonElement? Metadata { get; set; }
        /// <summary>Creation timestamp (ISO 8601).</summary>
        [JsonPropertyName("created_at")] public string? CreatedAt { get; set; }
    }

    /// <summary>A stored email with its bodies and events, from a detail fetch.</summary>
    public sealed class EmailDetail
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
        /// <summary>Carbon-copy addresses.</summary>
        [JsonPropertyName("cc_addresses")] public List<string>? CcAddresses { get; set; }
        /// <summary>Blind carbon-copy addresses.</summary>
        [JsonPropertyName("bcc_addresses")] public List<string>? BccAddresses { get; set; }
        /// <summary>Plain-text body.</summary>
        [JsonPropertyName("text_body")] public string? TextBody { get; set; }
        /// <summary>HTML body.</summary>
        [JsonPropertyName("html_body")] public string? HtmlBody { get; set; }
        /// <summary>Custom headers attached to the message.</summary>
        [JsonPropertyName("headers")] public JsonElement? Headers { get; set; }
        /// <summary>RFC 5322 Message-ID, once assigned.</summary>
        [JsonPropertyName("message_id")] public string? MessageId { get; set; }
        /// <summary>Source of the send (for example <c>api</c> or <c>campaign</c>).</summary>
        [JsonPropertyName("source")] public string? Source { get; set; }
        /// <summary>Number of unique opens recorded.</summary>
        [JsonPropertyName("opened_count")] public int OpenedCount { get; set; }
        /// <summary>Number of unique clicks recorded.</summary>
        [JsonPropertyName("clicked_count")] public int ClickedCount { get; set; }
        /// <summary>Tags attached to the message.</summary>
        [JsonPropertyName("tags")] public List<string>? Tags { get; set; }
        /// <summary>Scheduled send timestamp (ISO 8601), if scheduled.</summary>
        [JsonPropertyName("scheduled_at")] public string? ScheduledAt { get; set; }
        /// <summary>Creation timestamp (ISO 8601).</summary>
        [JsonPropertyName("created_at")] public string? CreatedAt { get; set; }
        /// <summary>Send timestamp (ISO 8601), once sent.</summary>
        [JsonPropertyName("sent_at")] public string? SentAt { get; set; }
        /// <summary>Delivery timestamp (ISO 8601), if delivered.</summary>
        [JsonPropertyName("delivered_at")] public string? DeliveredAt { get; set; }
        /// <summary>If this is a retry, the id of the original message.</summary>
        [JsonPropertyName("retry_of_id")] public string? RetryOfId { get; set; }
        /// <summary>Delivery / open / click / bounce events for this message.</summary>
        [JsonPropertyName("events")] public List<EmailEvent> Events { get; set; } = new List<EmailEvent>();
    }

    /// <summary>An email scheduled for future delivery.</summary>
    public sealed class ScheduledEmail
    {
        /// <summary>The message id.</summary>
        [JsonPropertyName("id")] public string Id { get; set; } = "";
        /// <summary>Sender address.</summary>
        [JsonPropertyName("from_address")] public string? FromAddress { get; set; }
        /// <summary>Recipient addresses.</summary>
        [JsonPropertyName("to_addresses")] public List<string>? ToAddresses { get; set; }
        /// <summary>Subject line.</summary>
        [JsonPropertyName("subject")] public string? Subject { get; set; }
        /// <summary>Current status (<c>scheduled</c>).</summary>
        [JsonPropertyName("status")] public string Status { get; set; } = "";
        /// <summary>Tags attached to the message.</summary>
        [JsonPropertyName("tags")] public List<string>? Tags { get; set; }
        /// <summary>Scheduled send timestamp (ISO 8601).</summary>
        [JsonPropertyName("scheduled_at")] public string? ScheduledAt { get; set; }
        /// <summary>Seconds remaining until the message is sent.</summary>
        [JsonPropertyName("seconds_until_send")] public int SecondsUntilSend { get; set; }
        /// <summary>Creation timestamp (ISO 8601).</summary>
        [JsonPropertyName("created_at")] public string? CreatedAt { get; set; }
    }

    /// <summary>A search hit returned by an emails search.</summary>
    public sealed class EmailSearchHit
    {
        /// <summary>The message id.</summary>
        [JsonPropertyName("id")] public string Id { get; set; } = "";
        /// <summary>Sender address.</summary>
        [JsonPropertyName("from_address")] public string? FromAddress { get; set; }
        /// <summary>Recipient addresses.</summary>
        [JsonPropertyName("to_addresses")] public List<string>? ToAddresses { get; set; }
        /// <summary>Subject line.</summary>
        [JsonPropertyName("subject")] public string? Subject { get; set; }
        /// <summary>Current status.</summary>
        [JsonPropertyName("status")] public string Status { get; set; } = "";
        /// <summary>Tags attached to the message.</summary>
        [JsonPropertyName("tags")] public List<string>? Tags { get; set; }
        /// <summary>Source of the send.</summary>
        [JsonPropertyName("source")] public string? Source { get; set; }
        /// <summary>Creation timestamp (ISO 8601).</summary>
        [JsonPropertyName("created_at")] public string? CreatedAt { get; set; }
        /// <summary>Delivery timestamp (ISO 8601), if delivered.</summary>
        [JsonPropertyName("delivered_at")] public string? DeliveredAt { get; set; }
    }

    /// <summary>The id and new status returned when a scheduled email is cancelled or sent now.</summary>
    public sealed class ScheduledActionResult
    {
        /// <summary>The message id.</summary>
        [JsonPropertyName("id")] public string Id { get; set; } = "";
        /// <summary>The resulting status (for example <c>cancelled</c> or <c>queued</c>).</summary>
        [JsonPropertyName("status")] public string Status { get; set; } = "";
    }
}
