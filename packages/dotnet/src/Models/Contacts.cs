using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Axene.Mailer
{
    /// <summary>A subscriber list.</summary>
    public sealed class ContactList
    {
        /// <summary>The list id.</summary>
        [JsonPropertyName("id")] public string Id { get; set; } = "";
        /// <summary>The list name.</summary>
        [JsonPropertyName("name")] public string Name { get; set; } = "";
        /// <summary>An optional description.</summary>
        [JsonPropertyName("description")] public string? Description { get; set; }
        /// <summary>A seed used to render the list's generated icon.</summary>
        [JsonPropertyName("icon_seed")] public string? IconSeed { get; set; }
        /// <summary>The number of contacts in the list.</summary>
        [JsonPropertyName("contact_count")] public int ContactCount { get; set; }
        /// <summary>Creation timestamp (ISO 8601).</summary>
        [JsonPropertyName("created_at")] public string? CreatedAt { get; set; }
    }

    /// <summary>A single contact in a list.</summary>
    public sealed class Contact
    {
        /// <summary>The contact id.</summary>
        [JsonPropertyName("id")] public string Id { get; set; } = "";
        /// <summary>The contact's email address.</summary>
        [JsonPropertyName("email")] public string Email { get; set; } = "";
        /// <summary>The contact's name, if known.</summary>
        [JsonPropertyName("name")] public string? Name { get; set; }
        /// <summary>Free-form metadata for the contact.</summary>
        [JsonPropertyName("metadata")] public JsonElement? Metadata { get; set; }
        /// <summary>Creation timestamp (ISO 8601).</summary>
        [JsonPropertyName("created_at")] public string? CreatedAt { get; set; }
    }

    /// <summary>A contact list with a page of its contacts.</summary>
    public sealed class ContactListDetail
    {
        /// <summary>The list id.</summary>
        [JsonPropertyName("id")] public string Id { get; set; } = "";
        /// <summary>The list name.</summary>
        [JsonPropertyName("name")] public string Name { get; set; } = "";
        /// <summary>An optional description.</summary>
        [JsonPropertyName("description")] public string? Description { get; set; }
        /// <summary>A seed used to render the list's generated icon.</summary>
        [JsonPropertyName("icon_seed")] public string? IconSeed { get; set; }
        /// <summary>The number of contacts in the list.</summary>
        [JsonPropertyName("contact_count")] public int ContactCount { get; set; }
        /// <summary>Creation timestamp (ISO 8601).</summary>
        [JsonPropertyName("created_at")] public string? CreatedAt { get; set; }
        /// <summary>The page of contacts in this list.</summary>
        [JsonPropertyName("contacts")] public List<Contact> Contacts { get; set; } = new List<Contact>();
    }

    /// <summary>Result of a CSV contact import.</summary>
    public sealed class CsvImportResult
    {
        /// <summary>Number of contacts imported.</summary>
        [JsonPropertyName("imported")] public int Imported { get; set; }
        /// <summary>Number of rows skipped (duplicates or invalid).</summary>
        [JsonPropertyName("skipped")] public int Skipped { get; set; }
        /// <summary>Per-row error messages, if any.</summary>
        [JsonPropertyName("errors")] public List<string> Errors { get; set; } = new List<string>();
    }

    /// <summary>Result of a templated bulk send to a list.</summary>
    public sealed class BulkSendResult
    {
        /// <summary>Number of messages queued.</summary>
        [JsonPropertyName("queued")] public int Queued { get; set; }
        /// <summary>Number of contacts skipped (suppressed or invalid).</summary>
        [JsonPropertyName("skipped")] public int Skipped { get; set; }
        /// <summary>Per-contact error messages, if any.</summary>
        [JsonPropertyName("errors")] public List<string> Errors { get; set; } = new List<string>();
    }
}
