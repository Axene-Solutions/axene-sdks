using System.Text.Json.Serialization;

namespace Axene.Mailer
{
    /// <summary>A file attachment.</summary>
    public sealed class Attachment
    {
        /// <summary>The file name shown to the recipient.</summary>
        [JsonPropertyName("filename")] public string Filename { get; set; } = "";

        /// <summary>Base64-encoded file content.</summary>
        [JsonPropertyName("content_base64")] public string ContentBase64 { get; set; } = "";

        /// <summary>Optional MIME type (inferred from the filename if omitted).</summary>
        [JsonPropertyName("content_type")] public string? ContentType { get; set; }
    }
}
