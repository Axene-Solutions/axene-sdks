using System.Text.Json.Serialization;

namespace Axene.Mailer
{
    /// <summary>A suppressed recipient address (on the do-not-send list).</summary>
    public sealed class Suppression
    {
        /// <summary>The suppression id.</summary>
        [JsonPropertyName("id")] public string Id { get; set; } = "";
        /// <summary>The suppressed email address.</summary>
        [JsonPropertyName("email_address")] public string EmailAddress { get; set; } = "";
        /// <summary>Why the address was suppressed (for example <c>manual</c>, <c>bounce</c>).</summary>
        [JsonPropertyName("reason")] public string Reason { get; set; } = "";
        /// <summary>Creation timestamp (ISO 8601).</summary>
        [JsonPropertyName("created_at")] public string? CreatedAt { get; set; }
    }

    /// <summary>Result of a bulk suppression import.</summary>
    public sealed class BulkSuppressionResult
    {
        /// <summary>Number of addresses added.</summary>
        [JsonPropertyName("added")] public int Added { get; set; }
        /// <summary>Number of addresses skipped (duplicates or invalid).</summary>
        [JsonPropertyName("skipped")] public int Skipped { get; set; }
        /// <summary>Total number of lines processed.</summary>
        [JsonPropertyName("total_processed")] public int TotalProcessed { get; set; }
    }
}
