using System.Collections.Generic;
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
    /// <remarks>
    /// The full Core API surface is exposed through resource groups:
    /// <see cref="Emails"/>, <see cref="Domains"/>, <see cref="Contacts"/>,
    /// <see cref="Suppressions"/>, <see cref="Templates"/>, and
    /// <see cref="Webhooks"/>. A handful of shortcut methods on the client itself
    /// cover the most common email operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// var axene = new AxeneMailerClient("axm_k_your_api_key");
    /// await axene.Emails.SendAsync(new SendEmail {
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

        /// <summary>Send, search, schedule, and inspect emails.</summary>
        public EmailsResource Emails { get; }

        /// <summary>Register, verify, and transfer sending domains.</summary>
        public DomainsResource Domains { get; }

        /// <summary>Manage subscriber lists and bulk sends.</summary>
        public ContactsResource Contacts { get; }

        /// <summary>Manage the do-not-send suppression list.</summary>
        public SuppressionsResource Suppressions { get; }

        /// <summary>Manage reusable email templates.</summary>
        public TemplatesResource Templates { get; }

        /// <summary>Manage event webhooks and inspect deliveries.</summary>
        public WebhooksResource Webhooks { get; }

        /// <summary>Create a client.</summary>
        /// <param name="apiKey">API key from your dashboard (starts with <c>axm_k_</c>).</param>
        /// <param name="baseUrl">Override the API base URL. Defaults to https://mail.axene.io.</param>
        /// <param name="httpClient">Supply your own <see cref="HttpClient"/> (recommended under DI). One is created and owned otherwise.</param>
        /// <param name="maxRetries">Total attempts on 429 / 5xx. Defaults to 3.</param>
        public AxeneMailerClient(string apiKey, string? baseUrl = null, HttpClient? httpClient = null, int maxRetries = 3)
        {
            _transport = new ApiTransport(apiKey, baseUrl, httpClient, maxRetries);
            Emails = new EmailsResource(_transport);
            Domains = new DomainsResource(_transport);
            Contacts = new ContactsResource(_transport);
            Suppressions = new SuppressionsResource(_transport);
            Templates = new TemplatesResource(_transport);
            Webhooks = new WebhooksResource(_transport);
        }

        /// <summary>Send a single email. Shortcut for <c>Emails.SendAsync</c>.</summary>
        public Task<SendEmailResult> SendAsync(SendEmail email, CancellationToken ct = default)
            => Emails.SendAsync(email, ct);

        /// <summary>
        /// Send up to your plan's batch limit in one call. Shortcut for
        /// <c>Emails.SendBatchAsync</c>.
        /// </summary>
        public Task<BatchResult> SendBatchAsync(IEnumerable<SendEmail> emails, CancellationToken ct = default)
            => Emails.SendBatchAsync(emails, ct);

        /// <summary>
        /// Dry-run a send without sending it. Shortcut for <c>Emails.ValidateAsync</c>.
        /// </summary>
        public Task<ValidationResult> ValidateAsync(SendEmail message, CancellationToken ct = default)
            => Emails.ValidateAsync(message, ct);

        /// <summary>Fetch a single email with its bodies and events. Shortcut for <c>Emails.GetAsync</c>.</summary>
        public Task<EmailDetail> GetAsync(string id, CancellationToken ct = default)
            => Emails.GetAsync(id, ct);

        /// <summary>List your sending domains. Shortcut for <c>Domains.ListAsync</c>.</summary>
        public Task<List<DomainRecord>> ListDomainsAsync(CancellationToken ct = default)
            => Domains.ListAsync(ct);

        /// <summary>Dispose the underlying <see cref="HttpClient"/> if this client created it.</summary>
        public void Dispose() => _transport.Dispose();
    }
}
