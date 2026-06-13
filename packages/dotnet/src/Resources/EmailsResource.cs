using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Axene.Mailer.Internal;

namespace Axene.Mailer
{
    /// <summary>
    /// Send, look up, search, schedule, and inspect emails. Accessed as
    /// <c>client.Emails</c>.
    /// </summary>
    public sealed class EmailsResource
    {
        private readonly ApiTransport _transport;

        internal EmailsResource(ApiTransport transport) => _transport = transport;

        /// <summary>Send a single email.</summary>
        public Task<SendEmailResult> SendAsync(SendEmail email, CancellationToken ct = default)
            => _transport.RequestAsync<SendEmailResult>(HttpMethod.Post, "v1/emails/", email.ToWire(), ct);

        /// <summary>
        /// Send up to your plan's batch limit in one call. The API accepts a bare
        /// array of messages and returns a per-message result set.
        /// </summary>
        public Task<BatchResult> SendBatchAsync(IEnumerable<SendEmail> emails, CancellationToken ct = default)
            => _transport.RequestAsync<BatchResult>(HttpMethod.Post, "v1/emails/batch",
                emails.Select(e => e.ToWire()).ToList(), ct);

        /// <summary>
        /// Dry-run a send: check whether <paramref name="message"/> would be
        /// accepted (sender registered, domain verified, plan limits, account not
        /// restricted) without actually sending it.
        /// </summary>
        public Task<ValidationResult> ValidateAsync(SendEmail message, CancellationToken ct = default)
            => _transport.RequestAsync<ValidationResult>(HttpMethod.Post, "v1/emails/validate", message.ToWire(), ct);

        /// <summary>List recent emails, newest first (zero-based <paramref name="page"/>).</summary>
        public Task<List<EmailRecord>> ListAsync(string? status = null, int page = 0, int limit = 20, CancellationToken ct = default)
            => _transport.RequestAsync<List<EmailRecord>>(HttpMethod.Get,
                "v1/emails/" + Wire.Query(("status", status), ("page", page), ("limit", limit)), null, ct);

        /// <summary>Fetch a single email with its bodies and events.</summary>
        public Task<EmailDetail> GetAsync(string id, CancellationToken ct = default)
            => _transport.RequestAsync<EmailDetail>(HttpMethod.Get, $"v1/emails/{Uri.EscapeDataString(id)}", null, ct);

        /// <summary>List delivery / open / click / bounce events for an email.</summary>
        public Task<List<EmailEvent>> EventsAsync(string id, CancellationToken ct = default)
            => _transport.RequestAsync<List<EmailEvent>>(HttpMethod.Get, $"v1/emails/{Uri.EscapeDataString(id)}/events", null, ct);

        /// <summary>Re-send a bounced, rejected, or failed email as a new message.</summary>
        public Task<SendEmailResult> RetryAsync(string id, CancellationToken ct = default)
            => _transport.RequestAsync<SendEmailResult>(HttpMethod.Post, $"v1/emails/{Uri.EscapeDataString(id)}/retry", null, ct);

        /// <summary>
        /// Search emails. <paramref name="q"/> supports inline tokens (<c>to:</c>,
        /// <c>from:</c>, <c>status:</c>, <c>domain:</c>, <c>tag:</c>); leftover words
        /// are matched as free text. Zero-based <paramref name="page"/>.
        /// </summary>
        public Task<List<EmailSearchHit>> SearchAsync(string? q = null, string? status = null, string? tag = null,
            int page = 0, int limit = 20, CancellationToken ct = default)
            => _transport.RequestAsync<List<EmailSearchHit>>(HttpMethod.Get,
                "v1/emails/search" + Wire.Query(("q", q), ("status", status), ("tag", tag), ("page", page), ("limit", limit)), null, ct);

        /// <summary>List emails scheduled for future delivery, soonest first.</summary>
        public Task<List<ScheduledEmail>> ListScheduledAsync(CancellationToken ct = default)
            => _transport.RequestAsync<List<ScheduledEmail>>(HttpMethod.Get, "v1/emails/scheduled", null, ct);

        /// <summary>Cancel a scheduled email.</summary>
        public Task<ScheduledActionResult> CancelScheduledAsync(string id, CancellationToken ct = default)
            => _transport.RequestAsync<ScheduledActionResult>(HttpMethod.Delete, $"v1/emails/scheduled/{Uri.EscapeDataString(id)}", null, ct);

        /// <summary>Send a scheduled email immediately instead of waiting.</summary>
        public Task<ScheduledActionResult> SendScheduledNowAsync(string id, CancellationToken ct = default)
            => _transport.RequestAsync<ScheduledActionResult>(HttpMethod.Post, $"v1/emails/scheduled/{Uri.EscapeDataString(id)}/send-now", null, ct);

        /// <summary>
        /// Poll for emails whose status changed at or after <paramref name="since"/>.
        /// Capped at 50 rows; use for live status updates.
        /// </summary>
        public Task<List<EmailRecord>> UpdatesAsync(string since, CancellationToken ct = default)
            => _transport.RequestAsync<List<EmailRecord>>(HttpMethod.Get, "v1/emails/updates" + Wire.Query(("since", since)), null, ct);

        /// <summary>
        /// Poll for emails whose status changed at or after <paramref name="since"/>.
        /// </summary>
        public Task<List<EmailRecord>> UpdatesAsync(DateTimeOffset since, CancellationToken ct = default)
            => UpdatesAsync(since.ToUniversalTime().ToString("o"), ct);

        /// <summary>Get the caller's saved searches.</summary>
        public async Task<List<JsonElement>> GetSavedSearchesAsync(CancellationToken ct = default)
        {
            var r = await _transport.RequestAsync<SavedSearchesEnvelope>(HttpMethod.Get, "v1/emails/saved-searches", null, ct).ConfigureAwait(false);
            return r.Searches;
        }

        /// <summary>Replace the caller's saved searches (max 50).</summary>
        public async Task<List<JsonElement>> SetSavedSearchesAsync(IEnumerable<object> searches, CancellationToken ct = default)
        {
            var body = new Dictionary<string, object?> { ["searches"] = searches.ToList() };
            var r = await _transport.RequestAsync<SavedSearchesEnvelope>(HttpMethod.Put, "v1/emails/saved-searches", body, ct).ConfigureAwait(false);
            return r.Searches;
        }

        private sealed class SavedSearchesEnvelope
        {
            [System.Text.Json.Serialization.JsonPropertyName("searches")]
            public List<JsonElement> Searches { get; set; } = new List<JsonElement>();
        }
    }
}
