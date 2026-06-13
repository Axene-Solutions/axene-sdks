using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Axene.Mailer.Internal
{
    /// <summary>
    /// The single place that talks to the network. Owns authentication, JSON
    /// encoding, retries with backoff, and turning non-2xx responses into
    /// <see cref="AxeneException"/>. Internal: the public surface is
    /// <see cref="AxeneMailerClient"/>.
    /// </summary>
    internal sealed class ApiTransport : IDisposable
    {
        private const string DefaultBaseUrl = "https://mail.axene.io";

        private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = null,
        };

        private readonly HttpClient _http;
        private readonly bool _ownsHttp;
        private readonly int _maxRetries;

        public ApiTransport(string apiKey, string? baseUrl, HttpClient? httpClient, int maxRetries)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("apiKey is required", nameof(apiKey));

            _ownsHttp = httpClient == null;
            _http = httpClient ?? new HttpClient();
            _http.BaseAddress ??= new Uri((baseUrl ?? DefaultBaseUrl).TrimEnd('/') + "/");
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            if (!_http.DefaultRequestHeaders.UserAgent.Any())
                _http.DefaultRequestHeaders.UserAgent.ParseAdd("Axene.Mailer/0.1.1");
            _maxRetries = Math.Max(1, maxRetries);
        }

        /// <summary>Send a request and deserialize the JSON response, retrying 429/5xx.</summary>
        public async Task<T> RequestAsync<T>(HttpMethod method, string path, object? body, CancellationToken ct)
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

                    if (IsRetryable((int)res.StatusCode) && attempt < _maxRetries)
                    {
                        await Task.Delay(Backoff(res, attempt), ct).ConfigureAwait(false);
                        continue;
                    }

                    var text = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (!res.IsSuccessStatusCode)
                        throw AxeneException.FromResponse((int)res.StatusCode, text);

                    return Deserialize<T>(text);
                }
                catch (AxeneException) { throw; } // a real API error: do not retry
                catch (Exception ex) when (attempt < _maxRetries)
                {
                    last = ex; // transport error: retry if attempts remain
                    await Task.Delay(Backoff(null, attempt), ct).ConfigureAwait(false);
                }
            }
            throw new AxeneException(0, $"Axene request failed: {last?.Message}", null);
        }

        /// <summary>
        /// Send a request that returns no useful body (for example a <c>204</c>
        /// from a DELETE). The response body, if any, is discarded.
        /// </summary>
        public Task RequestNoContentAsync(HttpMethod method, string path, object? body, CancellationToken ct)
            => RequestAsync<Unit>(method, path, body, ct);

        /// <summary>
        /// Upload a single file as <c>multipart/form-data</c> under the field name
        /// <c>file</c>. Used by the CSV / suppression import endpoints. Not retried
        /// (uploads are not idempotent).
        /// </summary>
        public async Task<T> UploadAsync<T>(string path, byte[] file, string filename, CancellationToken ct)
        {
            using var form = new MultipartFormDataContent();
            var part = new ByteArrayContent(file);
            part.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            form.Add(part, "file", filename);

            using var req = new HttpRequestMessage(HttpMethod.Post, path) { Content = form };
            using var res = await _http.SendAsync(req, ct).ConfigureAwait(false);

            var text = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!res.IsSuccessStatusCode)
                throw AxeneException.FromResponse((int)res.StatusCode, text);

            return Deserialize<T>(text);
        }

        /// <summary>
        /// Deserialize a JSON body, tolerating an empty body (204 No Content) by
        /// returning <c>default</c>. The non-null suppression is safe: callers of
        /// the empty-body path use the throwaway <see cref="Unit"/> type.
        /// </summary>
        private static T Deserialize<T>(string text)
            => string.IsNullOrWhiteSpace(text)
                ? default!
                : JsonSerializer.Deserialize<T>(text, JsonOpts)!;

        private static bool IsRetryable(int status) => status == 429 || status >= 500;

        private static TimeSpan Backoff(HttpResponseMessage? res, int attempt)
            => res?.Headers.RetryAfter?.Delta ?? TimeSpan.FromMilliseconds(250 * Math.Pow(2, attempt - 1));

        public void Dispose()
        {
            if (_ownsHttp) _http.Dispose();
        }
    }

    /// <summary>A throwaway result type for requests that return no useful body.</summary>
    internal sealed class Unit { }
}
