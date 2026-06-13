using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Axene.Mailer.Internal;

namespace Axene.Mailer
{
    /// <summary>
    /// Manage reusable email templates (Starter plan and up). Accessed as
    /// <c>client.Templates</c>.
    /// </summary>
    public sealed class TemplatesResource
    {
        private readonly ApiTransport _transport;

        internal TemplatesResource(ApiTransport transport) => _transport = transport;

        /// <summary>List all templates, most recently updated first.</summary>
        public Task<List<Template>> ListAsync(CancellationToken ct = default)
            => _transport.RequestAsync<List<Template>>(HttpMethod.Get, "v1/templates/", null, ct);

        /// <summary>
        /// Create a template. <c>variables</c> are derived server-side from
        /// <c>{{name}}</c> placeholders, so you do not pass them. The SDK's
        /// <paramref name="html"/> / <paramref name="text"/> map to the wire fields
        /// <c>html_body</c> / <c>text_body</c>.
        /// </summary>
        public Task<Template> CreateAsync(string name, string? subject = null, string? html = null,
            string? text = null, object? blocksJson = null, CancellationToken ct = default)
            => _transport.RequestAsync<Template>(HttpMethod.Post, "v1/templates/",
                Wire.Prune(new Dictionary<string, object?>
                {
                    ["name"] = name,
                    ["subject"] = subject,
                    ["html_body"] = html,
                    ["text_body"] = text,
                    ["blocks_json"] = blocksJson,
                }), ct);

        /// <summary>Fetch a single template.</summary>
        public Task<Template> GetAsync(string id, CancellationToken ct = default)
            => _transport.RequestAsync<Template>(HttpMethod.Get, $"v1/templates/{Uri.EscapeDataString(id)}", null, ct);

        /// <summary>Update a template (partial). <c>html</c> / <c>text</c> map to <c>html_body</c> / <c>text_body</c>.</summary>
        public Task<Template> UpdateAsync(string id, string? name = null, string? subject = null,
            string? html = null, string? text = null, object? blocksJson = null, CancellationToken ct = default)
            => _transport.RequestAsync<Template>(new HttpMethod("PATCH"), $"v1/templates/{Uri.EscapeDataString(id)}",
                Wire.Prune(new Dictionary<string, object?>
                {
                    ["name"] = name,
                    ["subject"] = subject,
                    ["html_body"] = html,
                    ["text_body"] = text,
                    ["blocks_json"] = blocksJson,
                }), ct);

        /// <summary>Delete a template.</summary>
        public Task DeleteAsync(string id, CancellationToken ct = default)
            => _transport.RequestNoContentAsync(HttpMethod.Delete, $"v1/templates/{Uri.EscapeDataString(id)}", null, ct);

        /// <summary>Duplicate a template (the copy's <c>blocks_json</c> is not carried over).</summary>
        public Task<Template> DuplicateAsync(string id, CancellationToken ct = default)
            => _transport.RequestAsync<Template>(HttpMethod.Post, $"v1/templates/{Uri.EscapeDataString(id)}/duplicate", null, ct);
    }
}
