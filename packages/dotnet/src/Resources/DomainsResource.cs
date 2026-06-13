using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Axene.Mailer.Internal;

namespace Axene.Mailer
{
    /// <summary>
    /// Register, verify, inspect, and transfer sending domains. Accessed as
    /// <c>client.Domains</c>.
    /// </summary>
    public sealed class DomainsResource
    {
        private readonly ApiTransport _transport;

        internal DomainsResource(ApiTransport transport) => _transport = transport;

        /// <summary>List your sending domains and their verification status.</summary>
        public Task<List<DomainRecord>> ListAsync(CancellationToken ct = default)
            => _transport.RequestAsync<List<DomainRecord>>(HttpMethod.Get, "v1/domains/", null, ct);

        /// <summary>Register a new sending domain. Returns the DNS records to publish.</summary>
        public Task<DomainDetail> CreateAsync(string name, CancellationToken ct = default)
            => _transport.RequestAsync<DomainDetail>(HttpMethod.Post, "v1/domains/", new Dictionary<string, object?> { ["name"] = name }, ct);

        /// <summary>Fetch a domain with its DKIM selector and DNS records.</summary>
        public Task<DomainDetail> GetAsync(string id, CancellationToken ct = default)
            => _transport.RequestAsync<DomainDetail>(HttpMethod.Get, $"v1/domains/{Uri.EscapeDataString(id)}", null, ct);

        /// <summary>Delete a domain.</summary>
        public Task DeleteAsync(string id, CancellationToken ct = default)
            => _transport.RequestNoContentAsync(HttpMethod.Delete, $"v1/domains/{Uri.EscapeDataString(id)}", null, ct);

        /// <summary>Re-check DNS and verify the domain.</summary>
        public Task<DomainDetail> VerifyAsync(string id, CancellationToken ct = default)
            => _transport.RequestAsync<DomainDetail>(HttpMethod.Post, $"v1/domains/{Uri.EscapeDataString(id)}/verify", null, ct);

        /// <summary>Run live DNS health checks (DKIM, SPF, DMARC, return-path, MX).</summary>
        public Task<DomainHealth> HealthAsync(string id, CancellationToken ct = default)
            => _transport.RequestAsync<DomainHealth>(HttpMethod.Get, $"v1/domains/{Uri.EscapeDataString(id)}/health", null, ct);

        /// <summary>Diagnose configuration issues and get a health score.</summary>
        public Task<DomainDiagnosis> DiagnoseAsync(string id, CancellationToken ct = default)
            => _transport.RequestAsync<DomainDiagnosis>(HttpMethod.Get, $"v1/domains/{Uri.EscapeDataString(id)}/diagnose", null, ct);

        /// <summary>Current MX status for inbound / forwarding (shape varies by provider).</summary>
        public Task<JsonElement> MxStatusAsync(string id, CancellationToken ct = default)
            => _transport.RequestAsync<JsonElement>(HttpMethod.Get, $"v1/domains/{Uri.EscapeDataString(id)}/mx-status", null, ct);

        /// <summary>The values currently published in DNS for each of the domain's records.</summary>
        public Task<JsonElement> PublishedRecordsAsync(string id, CancellationToken ct = default)
            => _transport.RequestAsync<JsonElement>(HttpMethod.Get, $"v1/domains/{Uri.EscapeDataString(id)}/published-records", null, ct);

        /// <summary>Rotate the domain's DKIM key, returning the new record to publish.</summary>
        public Task<DkimRotation> RotateDkimAsync(string id, CancellationToken ct = default)
            => _transport.RequestAsync<DkimRotation>(HttpMethod.Post, $"v1/domains/{Uri.EscapeDataString(id)}/rotate-dkim", null, ct);

        /// <summary>Initiate a transfer of this domain to another Axene account.</summary>
        public Task<DomainTransfer> TransferAsync(string id, string targetEmail, string? note = null, CancellationToken ct = default)
            => _transport.RequestAsync<DomainTransfer>(HttpMethod.Post, $"v1/domains/{Uri.EscapeDataString(id)}/transfer",
                new Dictionary<string, object?> { ["target_email"] = targetEmail, ["note"] = note }, ct);

        /// <summary>Check whether a domain name is available to add (checks public DNS).</summary>
        public Task<DomainAvailability> CheckAvailabilityAsync(string name, CancellationToken ct = default)
            => _transport.RequestAsync<DomainAvailability>(HttpMethod.Get, "v1/domains/check-availability" + Wire.Query(("name", name)), null, ct);

        /// <summary>Check whether a domain name already exists in your account.</summary>
        public Task<DomainCheck> CheckAsync(string name, CancellationToken ct = default)
            => _transport.RequestAsync<DomainCheck>(HttpMethod.Get, $"v1/domains/check/{Uri.EscapeDataString(name)}", null, ct);
    }
}
