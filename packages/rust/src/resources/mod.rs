//! Resource handles. Each is a thin wrapper over the shared transport, grouped
//! by API area: [`Emails`], [`Domains`], [`Contacts`], [`Suppressions`],
//! [`Templates`], and [`Webhooks`].

mod contacts;
mod domains;
mod emails;
mod suppressions;
mod templates;
mod webhooks;

pub use contacts::Contacts;
pub use domains::Domains;
pub use emails::Emails;
pub use suppressions::Suppressions;
pub use templates::Templates;
pub use webhooks::Webhooks;

/// Build a `?a=1&b=2` query string from `(key, value)` pairs, skipping `None`.
/// Returns an empty string when nothing is set.
pub(crate) fn query(pairs: &[(&str, Option<String>)]) -> String {
    let parts: Vec<String> = pairs
        .iter()
        .filter_map(|(k, v)| v.as_ref().map(|v| format!("{}={}", k, urlencode(v))))
        .collect();
    if parts.is_empty() {
        String::new()
    } else {
        format!("?{}", parts.join("&"))
    }
}

/// Percent-encode a path segment or query value (RFC 3986 unreserved kept).
pub(crate) fn urlencode(s: &str) -> String {
    let mut out = String::with_capacity(s.len());
    for b in s.bytes() {
        match b {
            b'A'..=b'Z' | b'a'..=b'z' | b'0'..=b'9' | b'-' | b'_' | b'.' | b'~' => {
                out.push(b as char)
            }
            _ => out.push_str(&format!("%{:02X}", b)),
        }
    }
    out
}
