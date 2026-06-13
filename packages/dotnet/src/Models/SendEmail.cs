using System;
using System.Collections.Generic;
using System.Linq;

namespace Axene.Mailer
{
    /// <summary>
    /// A message to send. <see cref="From"/>, <see cref="To"/>, and
    /// <see cref="Subject"/> are required; provide <see cref="Html"/>,
    /// <see cref="Text"/>, or both.
    /// </summary>
    public sealed class SendEmail
    {
        /// <summary>Sender address. Must be on a verified domain in your account.</summary>
        public Address From { get; set; } = new Address();

        /// <summary>One or more recipients.</summary>
        public List<Address> To { get; set; } = new List<Address>();

        /// <summary>Subject line.</summary>
        public string Subject { get; set; } = "";

        /// <summary>HTML body.</summary>
        public string? Html { get; set; }

        /// <summary>Plain-text body.</summary>
        public string? Text { get; set; }

        /// <summary>Carbon-copy recipients.</summary>
        public List<Address>? Cc { get; set; }

        /// <summary>Blind carbon-copy recipients.</summary>
        public List<Address>? Bcc { get; set; }

        /// <summary>Reply-to address.</summary>
        public Address? ReplyTo { get; set; }

        /// <summary>Custom headers to attach to the message.</summary>
        public Dictionary<string, string>? Headers { get; set; }

        /// <summary>Tags for filtering and analytics.</summary>
        public List<string>? Tags { get; set; }

        /// <summary>Schedule delivery for later (Starter plan and up).</summary>
        public DateTimeOffset? SendAt { get; set; }

        /// <summary>File attachments.</summary>
        public List<Attachment>? Attachments { get; set; }

        /// <summary>
        /// Build the JSON wire body. The API names the sender field <c>from_</c>,
        /// so the mapping from <see cref="From"/> happens here in one place, and
        /// null fields are omitted.
        /// </summary>
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
}
