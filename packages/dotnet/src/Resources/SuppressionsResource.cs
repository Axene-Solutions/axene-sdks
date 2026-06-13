using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Axene.Mailer.Internal;

namespace Axene.Mailer
{
    /// <summary>Manage the do-not-send suppression list. Accessed as <c>client.Suppressions</c>.</summary>
    public sealed class SuppressionsResource
    {
        private readonly ApiTransport _transport;

        internal SuppressionsResource(ApiTransport transport) => _transport = transport;

        /// <summary>
        /// List suppressed addresses as a paginated envelope (zero-based
        /// <paramref name="page"/>).
        /// </summary>
        public Task<Page<Suppression>> ListAsync(int page = 0, int limit = 50, string? search = null, CancellationToken ct = default)
            => _transport.RequestAsync<Page<Suppression>>(HttpMethod.Get,
                "v1/suppressions" + Wire.Query(("page", page), ("limit", limit), ("search", search)), null, ct);

        /// <summary>Suppress a single address. The wire field is <c>email_address</c>.</summary>
        public Task<Suppression> AddAsync(string email, string reason = "manual", CancellationToken ct = default)
            => _transport.RequestAsync<Suppression>(HttpMethod.Post, "v1/suppressions",
                new Dictionary<string, object?> { ["email_address"] = email, ["reason"] = reason }, ct);

        /// <summary>
        /// Bulk-import suppressions from a file (one email per line). Sent as
        /// <c>multipart/form-data</c> under the field <c>file</c>.
        /// </summary>
        public Task<BulkSuppressionResult> BulkUploadAsync(byte[] file, string filename = "suppressions.txt", CancellationToken ct = default)
            => _transport.UploadAsync<BulkSuppressionResult>("v1/suppressions/bulk", file, filename, ct);

        /// <summary>Remove an address from the suppression list.</summary>
        public Task RemoveAsync(string id, CancellationToken ct = default)
            => _transport.RequestNoContentAsync(HttpMethod.Delete, $"v1/suppressions/{Uri.EscapeDataString(id)}", null, ct);
    }
}
