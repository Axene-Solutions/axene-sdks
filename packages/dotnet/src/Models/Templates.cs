using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Axene.Mailer
{
    /// <summary>
    /// A reusable email template. <see cref="Variables"/> is derived server-side
    /// from <c>{{name}}</c> placeholders and is read-only.
    /// </summary>
    public sealed class Template
    {
        /// <summary>The template id.</summary>
        [JsonPropertyName("id")] public string Id { get; set; } = "";
        /// <summary>The template name.</summary>
        [JsonPropertyName("name")] public string Name { get; set; } = "";
        /// <summary>The default subject line.</summary>
        [JsonPropertyName("subject")] public string? Subject { get; set; }
        /// <summary>The HTML body (wire name <c>html_body</c>).</summary>
        [JsonPropertyName("html_body")] public string? HtmlBody { get; set; }
        /// <summary>The plain-text body (wire name <c>text_body</c>).</summary>
        [JsonPropertyName("text_body")] public string? TextBody { get; set; }
        /// <summary>Variables derived from the bodies (read-only).</summary>
        [JsonPropertyName("variables")] public List<string>? Variables { get; set; }
        /// <summary>Structured block-editor content, if any.</summary>
        [JsonPropertyName("blocks_json")] public JsonElement? BlocksJson { get; set; }
        /// <summary>Creation timestamp (ISO 8601).</summary>
        [JsonPropertyName("created_at")] public string? CreatedAt { get; set; }
        /// <summary>Last-updated timestamp (ISO 8601).</summary>
        [JsonPropertyName("updated_at")] public string? UpdatedAt { get; set; }
    }
}
