using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Axene.Mailer.Internal;

namespace Axene.Mailer
{
    /// <summary>
    /// Manage subscriber lists, their contacts, CSV imports, and templated bulk
    /// sends. Accessed as <c>client.Contacts</c>.
    /// </summary>
    public sealed class ContactsResource
    {
        private readonly ApiTransport _transport;

        internal ContactsResource(ApiTransport transport) => _transport = transport;

        /// <summary>List all subscriber lists in the active workspace.</summary>
        public Task<List<ContactList>> ListListsAsync(CancellationToken ct = default)
            => _transport.RequestAsync<List<ContactList>>(HttpMethod.Get, "v1/contacts/", null, ct);

        /// <summary>Create a subscriber list.</summary>
        public Task<ContactList> CreateListAsync(string name, string? description = null, string? iconSeed = null, CancellationToken ct = default)
            => _transport.RequestAsync<ContactList>(HttpMethod.Post, "v1/contacts/",
                Wire.Prune(new Dictionary<string, object?> { ["name"] = name, ["description"] = description, ["icon_seed"] = iconSeed }), ct);

        /// <summary>Get a list with a page of its contacts (zero-based <paramref name="page"/>).</summary>
        public Task<ContactListDetail> GetListAsync(string id, int page = 0, int limit = 50, CancellationToken ct = default)
            => _transport.RequestAsync<ContactListDetail>(HttpMethod.Get,
                $"v1/contacts/{Uri.EscapeDataString(id)}" + Wire.Query(("page", page), ("limit", limit)), null, ct);

        /// <summary>Update a list's name, description, or icon (partial).</summary>
        public Task<ContactList> UpdateListAsync(string id, string? name = null, string? description = null, string? iconSeed = null, CancellationToken ct = default)
            => _transport.RequestAsync<ContactList>(new HttpMethod("PATCH"), $"v1/contacts/{Uri.EscapeDataString(id)}",
                Wire.Prune(new Dictionary<string, object?> { ["name"] = name, ["description"] = description, ["icon_seed"] = iconSeed }), ct);

        /// <summary>Delete a list and all of its contacts.</summary>
        public Task DeleteListAsync(string id, CancellationToken ct = default)
            => _transport.RequestNoContentAsync(HttpMethod.Delete, $"v1/contacts/{Uri.EscapeDataString(id)}", null, ct);

        /// <summary>Add a single contact to a list.</summary>
        public Task<Contact> AddContactAsync(string listId, string email, string? name = null, object? metadata = null, CancellationToken ct = default)
            => _transport.RequestAsync<Contact>(HttpMethod.Post, $"v1/contacts/{Uri.EscapeDataString(listId)}/contacts",
                Wire.Prune(new Dictionary<string, object?> { ["email"] = email, ["name"] = name, ["metadata"] = metadata }), ct);

        /// <summary>Remove a contact from a list.</summary>
        public Task RemoveContactAsync(string listId, string contactId, CancellationToken ct = default)
            => _transport.RequestNoContentAsync(HttpMethod.Delete,
                $"v1/contacts/{Uri.EscapeDataString(listId)}/contacts/{Uri.EscapeDataString(contactId)}", null, ct);

        /// <summary>
        /// Import contacts from a CSV file (header row required). The email column
        /// is auto-detected; other columns become contact metadata. Sent as
        /// <c>multipart/form-data</c> under the field <c>file</c>.
        /// </summary>
        public Task<CsvImportResult> UploadCsvAsync(string listId, byte[] file, string filename = "contacts.csv", CancellationToken ct = default)
            => _transport.UploadAsync<CsvImportResult>($"v1/contacts/{Uri.EscapeDataString(listId)}/upload", file, filename, ct);

        /// <summary>
        /// Send a templated email to every contact in a list. Subject/html/text may
        /// use <c>{{email}}</c>, <c>{{name}}</c>, and <c>{{metadata_key}}</c>
        /// placeholders. The list id is injected as <c>contact_list_id</c>.
        /// </summary>
        public Task<BulkSendResult> BulkSendAsync(string listId, string senderAddressId, string subject,
            string? html = null, string? text = null, IEnumerable<string>? tags = null, CancellationToken ct = default)
            => _transport.RequestAsync<BulkSendResult>(HttpMethod.Post, $"v1/contacts/{Uri.EscapeDataString(listId)}/send",
                Wire.Prune(new Dictionary<string, object?>
                {
                    ["contact_list_id"] = listId,
                    ["sender_address_id"] = senderAddressId,
                    ["subject"] = subject,
                    ["html"] = html,
                    ["text"] = text,
                    ["tags"] = tags == null ? null : new List<string>(tags),
                }), ct);
    }
}
