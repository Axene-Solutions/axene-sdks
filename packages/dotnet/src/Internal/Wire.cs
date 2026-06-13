using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Axene.Mailer.Internal
{
    /// <summary>
    /// Internal helpers that translate the SDK's ergonomic shapes into the exact
    /// JSON body and query strings the API expects. Not part of the public API.
    /// </summary>
    internal static class Wire
    {
        /// <summary>
        /// Drop keys whose value is <c>null</c> so they are omitted from the JSON
        /// body. Mirrors the TypeScript SDK's <c>prune</c> helper. This is used for
        /// PATCH/partial bodies where an absent key means "leave unchanged".
        /// </summary>
        public static Dictionary<string, object?> Prune(Dictionary<string, object?> o)
            => o.Where(kv => kv.Value != null).ToDictionary(kv => kv.Key, kv => kv.Value);

        /// <summary>
        /// Build a URL query string, skipping <c>null</c> values. Returns <c>""</c>
        /// when nothing is set, or <c>"?a=1&amp;b=2"</c> otherwise. Booleans are
        /// lowercased to match the JSON wire form.
        /// </summary>
        public static string Query(params (string Key, object? Value)[] parameters)
        {
            var parts = new List<string>();
            foreach (var (key, value) in parameters)
            {
                if (value == null) continue;
                var s = value switch
                {
                    bool b => b ? "true" : "false",
                    System.IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
                    _ => value.ToString() ?? "",
                };
                parts.Add(System.Uri.EscapeDataString(key) + "=" + System.Uri.EscapeDataString(s));
            }
            return parts.Count == 0 ? "" : "?" + string.Join("&", parts);
        }
    }
}
