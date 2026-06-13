using System.Text.Json.Serialization;

namespace Axene.Mailer
{
    /// <summary>A sender or recipient. Converts implicitly from a bare email string.</summary>
    public sealed class Address
    {
        /// <summary>The email address.</summary>
        [JsonPropertyName("email")] public string Email { get; set; } = "";

        /// <summary>Optional display name.</summary>
        [JsonPropertyName("name")] public string? Name { get; set; }

        /// <summary>Create an empty address (for object initializers).</summary>
        public Address() { }

        /// <summary>Create an address with an optional display name.</summary>
        public Address(string email, string? name = null) { Email = email; Name = name; }

        /// <summary>Allow <c>Address a = "x@y.co";</c>.</summary>
        public static implicit operator Address(string email) => new Address(email);
    }
}
