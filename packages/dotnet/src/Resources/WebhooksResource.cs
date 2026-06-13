using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Axene.Mailer.Internal;

namespace Axene.Mailer
{
    /// <summary>
    /// Manage event webhooks and inspect deliveries. Accessed as
    /// <c>client.Webhooks</c>.
    /// </summary>
    public sealed class WebhooksResource
    {
        private readonly ApiTransport _transport;

        internal WebhooksResource(ApiTransport transport) => _transport = transport;

        /// <summary>List your active webhooks.</summary>
        public Task<List<Webhook>> ListAsync(CancellationToken ct = default)
            => _transport.RequestAsync<List<Webhook>>(HttpMethod.Get, "v1/webhooks/", null, ct);

        /// <summary>Create a webhook. The signing <c>secret</c> is generated and returned.</summary>
        public Task<Webhook> CreateAsync(string url, IEnumerable<string> events, CancellationToken ct = default)
            => _transport.RequestAsync<Webhook>(HttpMethod.Post, "v1/webhooks/",
                new Dictionary<string, object?> { ["url"] = url, ["events"] = new List<string>(events) }, ct);

        /// <summary>Update a webhook's url, events, or active state (partial).</summary>
        public Task<Webhook> UpdateAsync(string id, string? url = null, IEnumerable<string>? events = null,
            bool? isActive = null, CancellationToken ct = default)
            => _transport.RequestAsync<Webhook>(new HttpMethod("PATCH"), $"v1/webhooks/{Uri.EscapeDataString(id)}",
                Wire.Prune(new Dictionary<string, object?>
                {
                    ["url"] = url,
                    ["events"] = events == null ? null : new List<string>(events),
                    ["is_active"] = isActive,
                }), ct);

        /// <summary>Delete a webhook.</summary>
        public Task DeleteAsync(string id, CancellationToken ct = default)
            => _transport.RequestNoContentAsync(HttpMethod.Delete, $"v1/webhooks/{Uri.EscapeDataString(id)}", null, ct);

        /// <summary>Queue a sample <c>email.delivered</c> delivery to test the endpoint.</summary>
        public Task<WebhookTestResult> TestAsync(string id, CancellationToken ct = default)
            => _transport.RequestAsync<WebhookTestResult>(HttpMethod.Post, $"v1/webhooks/{Uri.EscapeDataString(id)}/test", null, ct);

        /// <summary>List delivery attempts for a webhook as a paginated envelope (zero-based <paramref name="page"/>).</summary>
        public Task<Page<WebhookDelivery>> ListDeliveriesAsync(string id, int page = 0, int limit = 20, string? status = null, CancellationToken ct = default)
            => _transport.RequestAsync<Page<WebhookDelivery>>(HttpMethod.Get,
                $"v1/webhooks/{Uri.EscapeDataString(id)}/deliveries" + Wire.Query(("page", page), ("limit", limit), ("status", status)), null, ct);

        /// <summary>Fetch one delivery with its full payload and the endpoint's response.</summary>
        public Task<WebhookDeliveryDetail> GetDeliveryAsync(string id, string deliveryId, CancellationToken ct = default)
            => _transport.RequestAsync<WebhookDeliveryDetail>(HttpMethod.Get,
                $"v1/webhooks/{Uri.EscapeDataString(id)}/deliveries/{Uri.EscapeDataString(deliveryId)}", null, ct);
    }
}
