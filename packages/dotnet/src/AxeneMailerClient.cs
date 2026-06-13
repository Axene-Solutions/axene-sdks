using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Axene.Mailer
{
    /// <summary>
    /// Official .NET client for Axene Mailer.
    ///
    /// <code>
    /// var axene = new AxeneMailerClient("axm_k_your_api_key");
    /// var res = await axene.SendAsync(new SendEmail {
    ///     From = "hello@yourdomain.com",
    ///     To = { "customer@example.com" },
    ///     Subject = "Your receipt",
    ///     Html = "&lt;p&gt;Thanks for your order.&lt;/p&gt;"
    /// });
    /// </code>
    /// </summary>
    public sealed class AxeneMailerClient : IDisposable
    {
        private const string DefaultBaseUrl = "https://mail.axene.io";
        private readonly HttpClient _http;
        private readonly bool _ownsHttp;
        private readonly int _maxRetries;

        private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = null
        };

        /// <param name="apiKey">API key from your dashboard (starts with <c>axm_k_</c>).</param>
        /// <param name="baseUrl">Override the API base URL. Defaults to https://mail.axene.io</param>
        /// <param name="httpClient">Supply your own HttpClient (recommended in DI). One is created otherwise.</param>
        /// <param name="maxRetries">Total attempts on 429 / 5xx. Defaults to 3.</param>
        public AxeneMailerClient(string apiKey, string? baseUrl = null, HttpClient? httpClient = null, int maxRetries = 3)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("apiKey is required", nameof(apiKey));

            _ownsHttp = httpClient == null;
            _http = httpClient ?? new HttpClient();
            _http.BaseAddress ??= new Uri((baseUrl ?? DefaultBaseUrl).TrimEnd('/') + "/");
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            if (!_http.DefaultRequestHeaders.UserAgent.Any())
                _http.DefaultRequestHeaders.UserAgent.ParseAdd("Axene.Mailer/0.1.0");
            _maxRetries = Math.Max(1, maxRetries);
        }

        /// <summary>Send a single email.</summary>
        public Task<SendEmailResult> SendAsync(SendEmail email, CancellationToken ct = default)
            => RequestAsync<SendEmailResult>(HttpMethod.Post, "v1/emails/", email.ToWire(), ct);

        /// <summary>Send up to your plan's batch limit in one call.</summary>
        public Task<BatchResult> SendBatchAsync(IEnumerable<SendEmail> emails, CancellationToken ct = default)
            => RequestAsync<BatchResult>(HttpMethod.Post, "v1/emails/batch",
                new Dictionary<string, object> { ["emails"] = emails.Select(e => e.ToWire()).ToList() }, ct);

        /// <summary>Fetch a single email and its current status.</summary>
        public Task<EmailRecord> GetAsync(string id, CancellationToken ct = default)
            => RequestAsync<EmailRecord>(HttpMethod.Get, $"v1/emails/{Uri.EscapeDataString(id)}", null, ct);

        /// <summary>Validate an address is well-formed and its domain can receive mail.</summary>
        public Task<ValidationResult> ValidateAsync(string email, CancellationToken ct = default)
            => RequestAsync<ValidationResult>(HttpMethod.Post, "v1/emails/validate",
                new Dictionary<string, object> { ["email"] = email }, ct);

        private async Task<T> RequestAsync<T>(HttpMethod method, string path, object? body, CancellationToken ct)
        {
            Exception? last = null;
            for (var attempt = 1; attempt <= _maxRetries; attempt++)
            {
                try
                {
                    using var req = new HttpRequestMessage(method, path);
                    if (body != null)
                        req.Content = new StringContent(JsonSerializer.Serialize(body, JsonOpts), Encoding.UTF8, "application/json");

                    using var res = await _http.SendAsync(req, ct).ConfigureAwait(false);

                    if (((int)res.StatusCode == 429 || (int)res.StatusCode >= 500) && attempt < _maxRetries)
                    {
                        var wait = res.Headers.RetryAfter?.Delta ?? TimeSpan.FromMilliseconds(250 * Math.Pow(2, attempt - 1));
                        await Task.Delay(wait, ct).ConfigureAwait(false);
                        continue;
                    }

                    var text = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (!res.IsSuccessStatusCode)
                        throw AxeneException.FromResponse((int)res.StatusCode, text);

                    return JsonSerializer.Deserialize<T>(text, JsonOpts)!;
                }
                catch (AxeneException) { throw; }
                catch (Exception ex) when (attempt < _maxRetries)
                {
                    last = ex;
                    await Task.Delay(TimeSpan.FromMilliseconds(250 * Math.Pow(2, attempt - 1)), ct).ConfigureAwait(false);
                }
            }
            throw new AxeneException(0, $"Axene request failed: {last?.Message}", null);
        }

        public void Dispose()
        {
            if (_ownsHttp) _http.Dispose();
        }
    }

    public sealed class Address
    {
        [JsonPropertyName("email")] public string Email { get; set; } = "";
        [JsonPropertyName("name")] public string? Name { get; set; }
        public Address() { }
        public Address(string email, string? name = null) { Email = email; Name = name; }
        public static implicit operator Address(string email) => new Address(email);
    }

    public sealed class Attachment
    {
        [JsonPropertyName("filename")] public string Filename { get; set; } = "";
        [JsonPropertyName("content_base64")] public string ContentBase64 { get; set; } = "";
        [JsonPropertyName("content_type")] public string? ContentType { get; set; }
    }

    /// <summary>A message to send. <see cref="From"/> is required, as are <see cref="To"/> and <see cref="Subject"/>.</summary>
    public sealed class SendEmail
    {
        public Address From { get; set; } = new Address();
        public List<Address> To { get; set; } = new List<Address>();
        public string Subject { get; set; } = "";
        public string? Html { get; set; }
        public string? Text { get; set; }
        public List<Address>? Cc { get; set; }
        public List<Address>? Bcc { get; set; }
        public Address? ReplyTo { get; set; }
        public Dictionary<string, string>? Headers { get; set; }
        public List<string>? Tags { get; set; }
        public DateTimeOffset? SendAt { get; set; }
        public List<Attachment>? Attachments { get; set; }

        // The API field for the sender is wire-named `from_`; we expose a clean `From`.
        internal Dictionary<string, object?> ToWire()
        {
            var d = new Dictionary<string, object?>
            {
                ["from_"] = From,
                ["to"] = To,
                ["subject"] = Subject,
                ["html"] = Html,
                ["text"] = Text,
                ["cc"] = Cc,
                ["bcc"] = Bcc,
                ["reply_to"] = ReplyTo,
                ["headers"] = Headers,
                ["tags"] = Tags,
                ["send_at"] = SendAt?.ToUniversalTime().ToString("o"),
                ["attachments"] = Attachments,
            };
            return d.Where(kv => kv.Value != null).ToDictionary(kv => kv.Key, kv => kv.Value);
        }
    }

    public sealed class SendEmailResult
    {
        [JsonPropertyName("id")] public string Id { get; set; } = "";
        [JsonPropertyName("status")] public string Status { get; set; } = "";
        [JsonPropertyName("message_id")] public string? MessageId { get; set; }
        [JsonPropertyName("rejection_reason")] public string? RejectionReason { get; set; }
    }

    public sealed class BatchResult
    {
        [JsonPropertyName("results")] public List<SendEmailResult> Results { get; set; } = new List<SendEmailResult>();
    }

    public sealed class EmailRecord
    {
        [JsonPropertyName("id")] public string Id { get; set; } = "";
        [JsonPropertyName("status")] public string Status { get; set; } = "";
        [JsonPropertyName("subject")] public string? Subject { get; set; }
        [JsonPropertyName("from_address")] public string? FromAddress { get; set; }
        [JsonPropertyName("to_addresses")] public List<string>? ToAddresses { get; set; }
        [JsonPropertyName("created_at")] public string? CreatedAt { get; set; }
        [JsonPropertyName("delivered_at")] public string? DeliveredAt { get; set; }
    }

    public sealed class ValidationResult
    {
        [JsonPropertyName("email")] public string Email { get; set; } = "";
        [JsonPropertyName("valid")] public bool Valid { get; set; }
        [JsonPropertyName("reason")] public string? Reason { get; set; }
    }

    /// <summary>Thrown for any non-2xx API response.</summary>
    public sealed class AxeneException : Exception
    {
        public int Status { get; }
        public string? Code { get; }
        public AxeneException(int status, string message, string? code) : base(message)
        {
            Status = status;
            Code = code;
        }

        internal static AxeneException FromResponse(int status, string body)
        {
            string message = $"Axene request failed ({status})";
            string? code = null;
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("detail", out var detail))
                {
                    if (detail.ValueKind == JsonValueKind.String) message = detail.GetString() ?? message;
                    else if (detail.ValueKind == JsonValueKind.Object)
                    {
                        if (detail.TryGetProperty("message", out var m)) message = m.GetString() ?? message;
                        if (detail.TryGetProperty("code", out var c)) code = c.GetString();
                    }
                }
            }
            catch { /* non-JSON body: keep the generic message */ }
            return new AxeneException(status, message, code);
        }
    }
}
