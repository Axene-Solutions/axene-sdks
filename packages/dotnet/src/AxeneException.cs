using System;
using System.Text.Json;

namespace Axene.Mailer
{
    /// <summary>Thrown for any non-2xx API response, or a transport failure that survives all retries.</summary>
    public sealed class AxeneException : Exception
    {
        /// <summary>HTTP status code. <c>0</c> indicates a transport/network failure (no response).</summary>
        public int Status { get; }

        /// <summary>Machine-readable error code from the API body, when present.</summary>
        public string? Code { get; }

        /// <summary>Create an exception.</summary>
        public AxeneException(int status, string message, string? code) : base(message)
        {
            Status = status;
            Code = code;
        }

        /// <summary>Parse the API's <c>{ "detail": { "code", "message" } }</c> (or string) body into an exception.</summary>
        internal static AxeneException FromResponse(int status, string body)
        {
            string message = $"Axene request failed ({status})";
            string? code = null;
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("detail", out var detail))
                {
                    if (detail.ValueKind == JsonValueKind.String)
                    {
                        message = detail.GetString() ?? message;
                    }
                    else if (detail.ValueKind == JsonValueKind.Object)
                    {
                        if (detail.TryGetProperty("message", out var m)) message = m.GetString() ?? message;
                        if (detail.TryGetProperty("code", out var c)) code = c.GetString();
                    }
                }
            }
            catch
            {
                // non-JSON body: keep the generic message
            }
            return new AxeneException(status, message, code);
        }
    }
}
