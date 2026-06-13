using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Axene.Mailer
{
    /// <summary>
    /// A paginated envelope <c>{ items, total, page, limit }</c> returned by the
    /// suppressions list and webhook deliveries endpoints. <c>Page</c> is
    /// zero-based.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    public sealed class Page<T>
    {
        /// <summary>The items on this page.</summary>
        [JsonPropertyName("items")] public List<T> Items { get; set; } = new List<T>();
        /// <summary>The total number of items across all pages.</summary>
        [JsonPropertyName("total")] public int Total { get; set; }
        /// <summary>The zero-based page index this envelope represents.</summary>
        [JsonPropertyName("page")] public int Page_ { get; set; }
        /// <summary>The page size.</summary>
        [JsonPropertyName("limit")] public int Limit { get; set; }
    }
}
