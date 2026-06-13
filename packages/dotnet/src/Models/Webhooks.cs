using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Axene.Mailer
{
    /// <summary>A configured webhook endpoint. <see cref="Secret"/> is returned in plaintext.</summary>
    public sealed class Webhook
    {
        /// <summary>The webhook id.</summary>
        [JsonPropertyName("id")] public string Id { get; set; } = "";
        /// <summary>The endpoint URL events are delivered to.</summary>
        [JsonPropertyName("url")] public string Url { get; set; } = "";
        /// <summary>The events this webhook subscribes to (for example <c>email.delivered</c>).</summary>
        [JsonPropertyName("events")] public List<string> Events { get; set; } = new List<string>();
        /// <summary>The HMAC signing secret (plaintext on every read).</summary>
        [JsonPropertyName("secret")] public string? Secret { get; set; }
        /// <summary>Whether the webhook is currently active.</summary>
        [JsonPropertyName("is_active")] public bool IsActive { get; set; }
        /// <summary>Creation timestamp (ISO 8601).</summary>
        [JsonPropertyName("created_at")] public string? CreatedAt { get; set; }
    }

    /// <summary>A summary of one webhook delivery attempt.</summary>
    public sealed class WebhookDelivery
    {
        /// <summary>The delivery id.</summary>
        [JsonPropertyName("id")] public string Id { get; set; } = "";
        /// <summary>The id of the webhook this delivery belongs to.</summary>
        [JsonPropertyName("webhook_id")] public string? WebhookId { get; set; }
        /// <summary>The event type delivered.</summary>
        [JsonPropertyName("event_type")] public string? EventType { get; set; }
        /// <summary>The delivery status.</summary>
        [JsonPropertyName("status")] public string Status { get; set; } = "";
        /// <summary>The HTTP status the endpoint returned, if any.</summary>
        [JsonPropertyName("response_status")] public int? ResponseStatus { get; set; }
        /// <summary>The attempt number.</summary>
        [JsonPropertyName("attempt")] public int Attempt { get; set; }
        /// <summary>When the next retry is scheduled (ISO 8601), if any.</summary>
        [JsonPropertyName("next_retry_at")] public string? NextRetryAt { get; set; }
        /// <summary>Creation timestamp (ISO 8601).</summary>
        [JsonPropertyName("created_at")] public string? CreatedAt { get; set; }
    }

    /// <summary>A webhook delivery with its full payload and the endpoint's response.</summary>
    public sealed class WebhookDeliveryDetail
    {
        /// <summary>The delivery id.</summary>
        [JsonPropertyName("id")] public string Id { get; set; } = "";
        /// <summary>The id of the webhook this delivery belongs to.</summary>
        [JsonPropertyName("webhook_id")] public string? WebhookId { get; set; }
        /// <summary>The event type delivered.</summary>
        [JsonPropertyName("event_type")] public string? EventType { get; set; }
        /// <summary>The delivery status.</summary>
        [JsonPropertyName("status")] public string Status { get; set; } = "";
        /// <summary>The HTTP status the endpoint returned, if any.</summary>
        [JsonPropertyName("response_status")] public int? ResponseStatus { get; set; }
        /// <summary>The attempt number.</summary>
        [JsonPropertyName("attempt")] public int Attempt { get; set; }
        /// <summary>When the next retry is scheduled (ISO 8601), if any.</summary>
        [JsonPropertyName("next_retry_at")] public string? NextRetryAt { get; set; }
        /// <summary>Creation timestamp (ISO 8601).</summary>
        [JsonPropertyName("created_at")] public string? CreatedAt { get; set; }
        /// <summary>The full event payload that was (or will be) sent.</summary>
        [JsonPropertyName("payload")] public JsonElement? Payload { get; set; }
        /// <summary>The body the endpoint returned, if any.</summary>
        [JsonPropertyName("response_body")] public string? ResponseBody { get; set; }
        /// <summary>The endpoint URL the delivery targeted.</summary>
        [JsonPropertyName("endpoint_url")] public string? EndpointUrl { get; set; }
    }

    /// <summary>The result of queuing a webhook test delivery.</summary>
    public sealed class WebhookTestResult
    {
        /// <summary>Whether a test delivery was queued.</summary>
        [JsonPropertyName("queued")] public bool Queued { get; set; }
        /// <summary>The endpoint URL the test was queued for.</summary>
        [JsonPropertyName("url")] public string? Url { get; set; }
    }
}
