using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Axene.Mailer.Internal;

namespace Axene.Mailer
{
    /// <summary>
    /// Official .NET client for Axene Mailer: send receipts, confirmations, and
    /// campaigns from your own domain. Priced in KES, billed via M-Pesa.
    /// </summary>
    /// <example>
    /// <code>
    /// var axene = new AxeneMailerClient("axm_k_your_api_key");
    /// var res = await axene.SendAsync(new SendEmail {
    ///     From = "hello@yourdomain.com",
    ///     To = { "customer@example.com" },
    ///     Subject = "Your receipt",
    ///     Html = "&lt;p&gt;Thanks for your order.&lt;/p&gt;"
    /// });
    /// </code>
    /// </example>
    public sealed class AxeneMailerClient : System.IDisposable
    {
        private readonly ApiTransport _transport;

        /// <summary>Create a client.</summary>
        /// <param name="apiKey">API key from your dashboard (starts with <c>axm_k_</c>).</param>
        /// <param name="baseUrl">Override the API base URL. Defaults to https://mail.axene.io.</param>
        /// <param name="httpClient">Supply your own <see cref="HttpClient"/> (recommended under DI). One is created and owned otherwise.</param>
        /// <param name="maxRetries">Total attempts on 429 / 5xx. Defaults to 3.</param>
        public AxeneMailerClient(string apiKey, string? baseUrl = null, HttpClient? httpClient = null, int maxRetries = 3)
        {
            _transport = new ApiTransport(apiKey, baseUrl, httpClient, maxRetries);
        }

        /// <summary>Send a single email.</summary>
        public Task<SendEmailResult> SendAsync(SendEmail email, CancellationToken ct = default)
            => _transport.RequestAsync<SendEmailResult>(HttpMethod.Post, "v1/emails/", email.ToWire(), ct);

        /// <summary>Send up to your plan's batch limit in one call. The API accepts a bare array of messages.</summary>
        public Task<BatchResult> SendBatchAsync(IEnumerable<SendEmail> emails, CancellationToken ct = default)
            => _transport.RequestAsync<BatchResult>(HttpMethod.Post, "v1/emails/batch",
                emails.Select(e => e.ToWire()).ToList(), ct);

        /// <summary>Fetch a single email and its current status.</summary>
        public Task<EmailRecord> GetAsync(string id, CancellationToken ct = default)
            => _transport.RequestAsync<EmailRecord>(HttpMethod.Get, $"v1/emails/{System.Uri.EscapeDataString(id)}", null, ct);

        /// <summary>
        /// Dry-run a send: check whether <paramref name="message"/> would be accepted
        /// (sender registered, domain verified, plan limits, account not restricted)
        /// without actually sending it.
        /// </summary>
        public Task<ValidationResult> ValidateAsync(SendEmail message, CancellationToken ct = default)
            => _transport.RequestAsync<ValidationResult>(HttpMethod.Post, "v1/emails/validate", message.ToWire(), ct);

        /// <summary>List your sending domains and their verification status.</summary>
        public Task<List<DomainRecord>> ListDomainsAsync(CancellationToken ct = default)
            => _transport.RequestAsync<List<DomainRecord>>(HttpMethod.Get, "v1/domains/", null, ct);

        /// <summary>Dispose the underlying <see cref="HttpClient"/> if this client created it.</summary>
        public void Dispose() => _transport.Dispose();
    }
}
