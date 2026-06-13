//! Serde structs for every request and response shape on the Axene Mailer API.
//!
//! Wire quirks are honoured here via `#[serde(rename = ...)]`: the sender field
//! serializes as the literal key `from_`, template bodies map `html`/`text` to
//! `html_body`/`text_body`, suppression `email` maps to `email_address`, and so
//! on. Loose or provider-specific shapes are modelled as [`serde_json::Value`].

use serde::{Deserialize, Serialize};
use serde_json::Value;
use std::collections::HashMap;

// -- shared -----------------------------------------------------------------

/// A recipient or sender address. A bare string is sugar for `{ email }`.
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Address {
    /// The email address (required).
    pub email: String,
    /// Optional display name.
    #[serde(skip_serializing_if = "Option::is_none")]
    pub name: Option<String>,
}

impl Address {
    /// Build an address with just an email.
    pub fn new(email: impl Into<String>) -> Self {
        Self {
            email: email.into(),
            name: None,
        }
    }

    /// Build an address with an email and a display name.
    pub fn named(email: impl Into<String>, name: impl Into<String>) -> Self {
        Self {
            email: email.into(),
            name: Some(name.into()),
        }
    }
}

impl From<&str> for Address {
    fn from(s: &str) -> Self {
        Address::new(s)
    }
}

impl From<String> for Address {
    fn from(s: String) -> Self {
        Address::new(s)
    }
}

impl From<(&str, &str)> for Address {
    fn from((email, name): (&str, &str)) -> Self {
        Address::named(email, name)
    }
}

/// A file attachment. `content_base64` is raw base64 with no `data:` prefix.
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Attachment {
    /// File name (1-255 chars, no `/`, `\`, or NUL).
    pub filename: String,
    /// Raw base64-encoded file content.
    pub content_base64: String,
    /// MIME type. Defaults server-side to `application/octet-stream`.
    #[serde(skip_serializing_if = "Option::is_none")]
    pub content_type: Option<String>,
}

/// A paginated envelope `{ items, total, page, limit }`.
#[derive(Debug, Clone, Deserialize)]
pub struct Page<T> {
    /// The items on this page.
    pub items: Vec<T>,
    /// Total number of items across all pages.
    pub total: u64,
    /// Zero-based page index.
    pub page: u64,
    /// Page size.
    pub limit: u64,
}

// -- emails: request --------------------------------------------------------

/// Body of a send / validate request.
///
/// Build with [`SendEmail::builder`]. The sender field serializes as `from_`.
#[derive(Debug, Clone, Serialize)]
pub struct SendEmail {
    /// Sender address (serialized as `from_` on the wire).
    #[serde(rename = "from_")]
    pub from: Address,
    /// One or more recipients.
    pub to: Vec<Address>,
    /// Subject line.
    pub subject: String,
    /// HTML body.
    #[serde(skip_serializing_if = "Option::is_none")]
    pub html: Option<String>,
    /// Plain-text body.
    #[serde(skip_serializing_if = "Option::is_none")]
    pub text: Option<String>,
    /// CC recipients.
    #[serde(skip_serializing_if = "Option::is_none")]
    pub cc: Option<Vec<Address>>,
    /// BCC recipients.
    #[serde(skip_serializing_if = "Option::is_none")]
    pub bcc: Option<Vec<Address>>,
    /// Reply-To address.
    #[serde(skip_serializing_if = "Option::is_none")]
    pub reply_to: Option<Address>,
    /// Custom message headers.
    #[serde(skip_serializing_if = "Option::is_none")]
    pub headers: Option<HashMap<String, String>>,
    /// Tags for filtering and analytics.
    #[serde(skip_serializing_if = "Option::is_none")]
    pub tags: Option<Vec<String>>,
    /// Schedule delivery for later (ISO 8601). Starter plan and up.
    #[serde(skip_serializing_if = "Option::is_none")]
    pub send_at: Option<String>,
    /// File attachments.
    #[serde(skip_serializing_if = "Option::is_none")]
    pub attachments: Option<Vec<Attachment>>,
}

impl SendEmail {
    /// Start building a message with a sender, recipients, and subject.
    pub fn builder(
        from: impl Into<Address>,
        to: impl IntoAddressList,
        subject: impl Into<String>,
    ) -> SendEmailBuilder {
        SendEmailBuilder {
            inner: SendEmail {
                from: from.into(),
                to: to.into_address_list(),
                subject: subject.into(),
                html: None,
                text: None,
                cc: None,
                bcc: None,
                reply_to: None,
                headers: None,
                tags: None,
                send_at: None,
                attachments: None,
            },
        }
    }
}

/// Conversion trait so `to`/`cc`/`bcc` accept a single address or a list.
pub trait IntoAddressList {
    /// Normalize one-or-many addresses into a vector.
    fn into_address_list(self) -> Vec<Address>;
}

impl IntoAddressList for Address {
    fn into_address_list(self) -> Vec<Address> {
        vec![self]
    }
}

impl IntoAddressList for &str {
    fn into_address_list(self) -> Vec<Address> {
        vec![Address::new(self)]
    }
}

impl IntoAddressList for String {
    fn into_address_list(self) -> Vec<Address> {
        vec![Address::new(self)]
    }
}

impl<T: Into<Address>> IntoAddressList for Vec<T> {
    fn into_address_list(self) -> Vec<Address> {
        self.into_iter().map(Into::into).collect()
    }
}

/// Ergonomic builder for [`SendEmail`].
#[derive(Debug, Clone)]
pub struct SendEmailBuilder {
    inner: SendEmail,
}

impl SendEmailBuilder {
    /// Set the HTML body.
    pub fn html(mut self, html: impl Into<String>) -> Self {
        self.inner.html = Some(html.into());
        self
    }

    /// Set the plain-text body.
    pub fn text(mut self, text: impl Into<String>) -> Self {
        self.inner.text = Some(text.into());
        self
    }

    /// Set CC recipients.
    pub fn cc(mut self, cc: impl IntoAddressList) -> Self {
        self.inner.cc = Some(cc.into_address_list());
        self
    }

    /// Set BCC recipients.
    pub fn bcc(mut self, bcc: impl IntoAddressList) -> Self {
        self.inner.bcc = Some(bcc.into_address_list());
        self
    }

    /// Set the Reply-To address.
    pub fn reply_to(mut self, reply_to: impl Into<Address>) -> Self {
        self.inner.reply_to = Some(reply_to.into());
        self
    }

    /// Set custom headers.
    pub fn headers(mut self, headers: HashMap<String, String>) -> Self {
        self.inner.headers = Some(headers);
        self
    }

    /// Set tags.
    pub fn tags(mut self, tags: Vec<String>) -> Self {
        self.inner.tags = Some(tags);
        self
    }

    /// Schedule delivery for later (ISO 8601 string).
    pub fn send_at(mut self, send_at: impl Into<String>) -> Self {
        self.inner.send_at = Some(send_at.into());
        self
    }

    /// Set attachments.
    pub fn attachments(mut self, attachments: Vec<Attachment>) -> Self {
        self.inner.attachments = Some(attachments);
        self
    }

    /// Finish building.
    pub fn build(self) -> SendEmail {
        self.inner
    }
}

// -- emails: response -------------------------------------------------------

/// Result of a send: the queued message id and its initial status.
#[derive(Debug, Clone, Deserialize)]
pub struct SendEmailResponse {
    /// The new message id.
    pub id: String,
    /// Initial status, e.g. `queued`.
    pub status: String,
    /// Provider message id, if assigned.
    pub message_id: Option<String>,
    /// Reason the message was rejected, if any.
    pub rejection_reason: Option<String>,
}

/// One per-message result inside a batch response.
#[derive(Debug, Clone, Deserialize)]
pub struct BatchItemResult {
    /// The new message id, or `None` if the item errored.
    pub id: Option<String>,
    /// Per-item status, e.g. `queued` or `error`.
    pub status: String,
    /// Reason the item was rejected, if any.
    pub rejection_reason: Option<String>,
}

/// Result of a batch send.
#[derive(Debug, Clone, Deserialize)]
pub struct BatchResponse {
    /// Number of messages submitted.
    pub total: u64,
    /// Number accepted for delivery.
    pub sent: u64,
    /// Number rejected.
    pub failed: u64,
    /// One result per submitted message, in order.
    pub results: Vec<BatchItemResult>,
}

/// A single reason a message would not send.
#[derive(Debug, Clone, Deserialize)]
pub struct ValidationIssue {
    /// The offending field.
    pub field: String,
    /// The error description.
    pub error: String,
}

/// Sending-quota usage returned alongside a validation.
#[derive(Debug, Clone, Deserialize)]
pub struct ValidationUsage {
    /// Emails sent today.
    pub daily: u64,
    /// Daily send limit.
    pub daily_limit: u64,
    /// Emails sent this month.
    pub monthly: u64,
    /// Monthly send limit.
    pub monthly_limit: u64,
}

/// Result of a dry-run validation.
#[derive(Debug, Clone, Deserialize)]
pub struct ValidationResult {
    /// Whether the request body is well-formed.
    pub valid: bool,
    /// Whether the message could actually be sent right now.
    pub can_send: bool,
    /// Any issues that would block the send.
    pub issues: Vec<ValidationIssue>,
    /// The caller's plan.
    pub plan: String,
    /// Current quota usage.
    pub usage: ValidationUsage,
}

/// A stored email and its current status.
#[derive(Debug, Clone, Deserialize)]
pub struct Email {
    /// Message id.
    pub id: String,
    /// Sender address.
    pub from_address: String,
    /// Recipient addresses.
    pub to_addresses: Vec<String>,
    /// Subject line.
    pub subject: Option<String>,
    /// Current status.
    pub status: String,
    /// Where the message originated (e.g. `api`).
    #[serde(default)]
    pub source: Option<String>,
    /// Number of opens.
    #[serde(default)]
    pub opened_count: Option<u64>,
    /// Number of clicks.
    #[serde(default)]
    pub clicked_count: Option<u64>,
    /// Tags.
    #[serde(default)]
    pub tags: Option<Vec<String>>,
    /// Scheduled send time, if any.
    #[serde(default)]
    pub scheduled_at: Option<String>,
    /// Creation time.
    pub created_at: Option<String>,
    /// Send time, if sent.
    #[serde(default)]
    pub sent_at: Option<String>,
    /// Delivery time, if delivered.
    #[serde(default)]
    pub delivered_at: Option<String>,
    /// Id of the original message this is a retry of, if any.
    #[serde(default)]
    pub retry_of_id: Option<String>,
}

/// A delivery / open / click / bounce event for a message.
#[derive(Debug, Clone, Deserialize)]
pub struct EmailEvent {
    /// Event id.
    pub id: String,
    /// Event type, e.g. `delivered`.
    pub event_type: String,
    /// Arbitrary event metadata.
    #[serde(default)]
    pub metadata: Option<Value>,
    /// Creation time.
    pub created_at: String,
}

/// A stored email with its bodies and events.
#[derive(Debug, Clone, Deserialize)]
pub struct EmailDetail {
    /// The base email row.
    #[serde(flatten)]
    pub email: Email,
    /// CC addresses.
    #[serde(default)]
    pub cc_addresses: Option<Vec<String>>,
    /// BCC addresses.
    #[serde(default)]
    pub bcc_addresses: Option<Vec<String>>,
    /// Plain-text body.
    #[serde(default)]
    pub text_body: Option<String>,
    /// HTML body.
    #[serde(default)]
    pub html_body: Option<String>,
    /// Custom headers.
    #[serde(default)]
    pub headers: Option<Value>,
    /// Provider message id.
    #[serde(default)]
    pub message_id: Option<String>,
    /// Associated events.
    #[serde(default)]
    pub events: Vec<EmailEvent>,
}

/// A scheduled email awaiting send.
#[derive(Debug, Clone, Deserialize)]
pub struct ScheduledEmail {
    /// Message id.
    pub id: String,
    /// Sender address.
    pub from_address: String,
    /// Recipient addresses.
    pub to_addresses: Vec<String>,
    /// Subject line.
    pub subject: Option<String>,
    /// Current status (always `scheduled`).
    pub status: String,
    /// Tags.
    #[serde(default)]
    pub tags: Option<Vec<String>>,
    /// Scheduled send time.
    #[serde(default)]
    pub scheduled_at: Option<String>,
    /// Seconds remaining until send.
    pub seconds_until_send: i64,
    /// Creation time.
    pub created_at: Option<String>,
}

/// A search hit from the email search endpoint.
#[derive(Debug, Clone, Deserialize)]
pub struct EmailSearchHit {
    /// Message id.
    pub id: String,
    /// Sender address.
    pub from_address: String,
    /// Recipient addresses.
    pub to_addresses: Vec<String>,
    /// Subject line.
    pub subject: Option<String>,
    /// Current status.
    pub status: String,
    /// Tags.
    #[serde(default)]
    pub tags: Option<Vec<String>>,
    /// Where the message originated.
    #[serde(default)]
    pub source: Option<String>,
    /// Creation time.
    pub created_at: Option<String>,
    /// Delivery time.
    #[serde(default)]
    pub delivered_at: Option<String>,
}

/// A simple `{ id, status }` response (cancel / send-now).
#[derive(Debug, Clone, Deserialize)]
pub struct IdStatus {
    /// The affected id.
    pub id: String,
    /// The resulting status.
    pub status: String,
}

// -- domains ----------------------------------------------------------------

/// A row from the domain list: a sending domain and its status.
#[derive(Debug, Clone, Deserialize)]
pub struct DomainListItem {
    /// Domain id.
    pub id: String,
    /// Domain name.
    pub name: String,
    /// Verification status.
    pub status: String,
    /// Creation time.
    pub created_at: Option<String>,
    /// Warning if this is a platform/shared domain.
    #[serde(default)]
    pub platform_warning: Option<String>,
}

/// A DNS record the API expects you to publish for a domain.
#[derive(Debug, Clone, Deserialize)]
pub struct DnsRecord {
    /// Record id.
    pub id: String,
    /// Record type, e.g. `TXT`.
    pub record_type: String,
    /// What the record is for, e.g. `dkim`.
    pub purpose: String,
    /// The host/name.
    pub host: String,
    /// The expected value.
    pub value: String,
    /// Whether the record has been verified.
    pub is_verified: bool,
    /// When the record was last checked.
    #[serde(default)]
    pub last_checked_at: Option<String>,
}

/// A sending domain with its DKIM selector and DNS records.
#[derive(Debug, Clone, Deserialize)]
pub struct Domain {
    /// Domain id.
    pub id: String,
    /// Domain name.
    pub name: String,
    /// Verification status.
    pub status: String,
    /// The DKIM selector.
    pub dkim_selector: String,
    /// When the domain was verified.
    #[serde(default)]
    pub verified_at: Option<String>,
    /// Creation time.
    pub created_at: Option<String>,
    /// DNS records to publish.
    pub dns_records: Vec<DnsRecord>,
    /// Warning if this is a platform/shared domain.
    #[serde(default)]
    pub platform_warning: Option<String>,
}

/// One row of a domain health report.
#[derive(Debug, Clone, Deserialize)]
pub struct DomainHealthCheck {
    /// Check key.
    pub key: String,
    /// Human-readable label.
    pub label: String,
    /// Outcome: `ok`, `warn`, `error`, or `info`.
    pub status: String,
    /// Detail message.
    pub detail: String,
    /// Recommended fix, if any.
    #[serde(default)]
    pub recommendation: Option<String>,
    /// The associated DNS record, if any.
    #[serde(default)]
    pub record: Option<Value>,
}

/// Tally of health-check outcomes.
#[derive(Debug, Clone, Deserialize)]
pub struct DomainHealthSummary {
    /// Number of `ok` checks.
    pub ok: u64,
    /// Number of `warn` checks.
    pub warn: u64,
    /// Number of `error` checks.
    pub error: u64,
    /// Number of `info` checks.
    pub info: u64,
}

/// Result of a domain health report.
#[derive(Debug, Clone, Deserialize)]
pub struct DomainHealth {
    /// Domain name.
    pub domain: String,
    /// Per-record checks.
    pub checks: Vec<DomainHealthCheck>,
    /// Summary tally.
    pub summary: DomainHealthSummary,
}

/// Result of a domain diagnosis. `issues` shapes vary; treated as opaque.
#[derive(Debug, Clone, Deserialize)]
pub struct DomainDiagnosis {
    /// Domain name.
    pub domain: String,
    /// The detected issues (loose shape).
    pub issues: Vec<Value>,
    /// A 0-100 health score.
    pub health_score: i64,
}

/// Result of a DKIM rotation: the new record plus the updated domain.
#[derive(Debug, Clone, Deserialize)]
pub struct DkimRotation {
    /// New DKIM record host.
    pub dkim_record_host: String,
    /// New DKIM record value.
    pub dkim_record_value: String,
    /// The updated domain.
    pub domain: Domain,
}

/// A domain transfer record.
#[derive(Debug, Clone, Deserialize)]
pub struct DomainTransfer {
    /// Transfer id.
    pub id: String,
    /// The domain being transferred.
    pub domain_id: String,
    /// The domain name.
    #[serde(default)]
    pub domain_name: Option<String>,
    /// A label for the source account.
    #[serde(default)]
    pub source_label: Option<String>,
    /// The recipient email.
    pub target_email: String,
    /// Transfer status.
    pub status: String,
    /// An optional note.
    #[serde(default)]
    pub note: Option<String>,
    /// Cool-off deadline, if any.
    #[serde(default)]
    pub cooloff_until: Option<String>,
    /// When the transfer was initiated.
    pub initiated_at: String,
    /// When the transfer was accepted, if at all.
    #[serde(default)]
    pub accepted_at: Option<String>,
    /// When the transfer completed, if at all.
    #[serde(default)]
    pub completed_at: Option<String>,
    /// When the transfer offer expires.
    pub expires_at: String,
}

/// Result of a domain availability check.
#[derive(Debug, Clone, Deserialize)]
pub struct DomainAvailability {
    /// Whether the domain can be added.
    pub available: bool,
    /// Why not, if unavailable.
    pub reason: Option<String>,
    /// Additional detail.
    pub detail: Option<String>,
    /// Count of stale verification tokens.
    pub stale_tokens: Option<i64>,
}

/// Result of a domain existence check.
#[derive(Debug, Clone, Deserialize)]
pub struct DomainCheck {
    /// Whether the domain exists in your account.
    pub exists: bool,
    /// Whether it is verified.
    pub verified: bool,
    /// Status, if present.
    #[serde(default)]
    pub status: Option<String>,
    /// The queried domain name.
    pub domain: String,
    /// The domain id, if present.
    #[serde(default)]
    pub id: Option<String>,
}

// -- contacts ---------------------------------------------------------------

/// A subscriber list.
#[derive(Debug, Clone, Deserialize)]
pub struct ContactList {
    /// List id.
    pub id: String,
    /// List name.
    pub name: String,
    /// Optional description.
    pub description: Option<String>,
    /// Avatar seed.
    pub icon_seed: Option<String>,
    /// Number of contacts on the list.
    pub contact_count: u64,
    /// Creation time.
    pub created_at: String,
}

/// A single contact in a list.
#[derive(Debug, Clone, Deserialize)]
pub struct Contact {
    /// Contact id.
    pub id: String,
    /// Email address.
    pub email: String,
    /// Display name.
    pub name: Option<String>,
    /// Arbitrary custom fields.
    #[serde(default)]
    pub metadata: Option<Value>,
    /// Creation time.
    pub created_at: String,
}

/// A contact list with a page of its contacts.
#[derive(Debug, Clone, Deserialize)]
pub struct ContactListDetail {
    /// The base list.
    #[serde(flatten)]
    pub list: ContactList,
    /// A page of contacts.
    pub contacts: Vec<Contact>,
}

/// Body for creating a contact list. Build with [`CreateList::new`].
#[derive(Debug, Clone, Serialize, Default)]
pub struct CreateList {
    /// List name (required).
    pub name: String,
    /// Optional description.
    #[serde(skip_serializing_if = "Option::is_none")]
    pub description: Option<String>,
    /// Avatar seed (serialized as `icon_seed`).
    #[serde(rename = "icon_seed", skip_serializing_if = "Option::is_none")]
    pub icon_seed: Option<String>,
}

impl CreateList {
    /// Start a create-list body with a name.
    pub fn new(name: impl Into<String>) -> Self {
        Self {
            name: name.into(),
            description: None,
            icon_seed: None,
        }
    }

    /// Set the description.
    pub fn description(mut self, description: impl Into<String>) -> Self {
        self.description = Some(description.into());
        self
    }

    /// Set the avatar seed.
    pub fn icon_seed(mut self, icon_seed: impl Into<String>) -> Self {
        self.icon_seed = Some(icon_seed.into());
        self
    }
}

/// Partial body for updating a contact list. Defaults to all-unset.
#[derive(Debug, Clone, Serialize, Default)]
pub struct UpdateList {
    /// New name.
    #[serde(skip_serializing_if = "Option::is_none")]
    pub name: Option<String>,
    /// New description.
    #[serde(skip_serializing_if = "Option::is_none")]
    pub description: Option<String>,
    /// New avatar seed (serialized as `icon_seed`).
    #[serde(rename = "icon_seed", skip_serializing_if = "Option::is_none")]
    pub icon_seed: Option<String>,
}

impl UpdateList {
    /// Set the name.
    pub fn name(mut self, name: impl Into<String>) -> Self {
        self.name = Some(name.into());
        self
    }

    /// Set the description.
    pub fn description(mut self, description: impl Into<String>) -> Self {
        self.description = Some(description.into());
        self
    }

    /// Set the avatar seed.
    pub fn icon_seed(mut self, icon_seed: impl Into<String>) -> Self {
        self.icon_seed = Some(icon_seed.into());
        self
    }
}

/// Body for adding a contact to a list.
#[derive(Debug, Clone, Serialize)]
pub struct AddContact {
    /// Email address (required).
    pub email: String,
    /// Display name.
    #[serde(skip_serializing_if = "Option::is_none")]
    pub name: Option<String>,
    /// Arbitrary custom fields.
    #[serde(skip_serializing_if = "Option::is_none")]
    pub metadata: Option<Value>,
}

impl AddContact {
    /// Start an add-contact body with an email.
    pub fn new(email: impl Into<String>) -> Self {
        Self {
            email: email.into(),
            name: None,
            metadata: None,
        }
    }

    /// Set the display name.
    pub fn name(mut self, name: impl Into<String>) -> Self {
        self.name = Some(name.into());
        self
    }

    /// Set custom fields.
    pub fn metadata(mut self, metadata: Value) -> Self {
        self.metadata = Some(metadata);
        self
    }
}

/// Result of a CSV contact import.
#[derive(Debug, Clone, Deserialize)]
pub struct CsvImportResult {
    /// Number of contacts imported.
    pub imported: u64,
    /// Number of rows skipped.
    pub skipped: u64,
    /// Per-row error messages.
    pub errors: Vec<String>,
}

/// Body for a templated bulk send. `contact_list_id` is injected by the SDK.
#[derive(Debug, Clone, Serialize)]
pub struct BulkSend {
    /// The list id (set automatically by the SDK).
    pub contact_list_id: String,
    /// The verified sender address id.
    pub sender_address_id: String,
    /// Subject line (may use `{{placeholders}}`).
    pub subject: String,
    /// HTML body.
    #[serde(skip_serializing_if = "Option::is_none")]
    pub html: Option<String>,
    /// Plain-text body.
    #[serde(skip_serializing_if = "Option::is_none")]
    pub text: Option<String>,
    /// Tags.
    #[serde(skip_serializing_if = "Option::is_none")]
    pub tags: Option<Vec<String>>,
}

impl BulkSend {
    /// Start a bulk-send body with a sender address id and subject. The list id
    /// is filled in by [`crate::resources::Contacts::bulk_send`].
    pub fn new(sender_address_id: impl Into<String>, subject: impl Into<String>) -> Self {
        Self {
            contact_list_id: String::new(),
            sender_address_id: sender_address_id.into(),
            subject: subject.into(),
            html: None,
            text: None,
            tags: None,
        }
    }

    /// Set the HTML body.
    pub fn html(mut self, html: impl Into<String>) -> Self {
        self.html = Some(html.into());
        self
    }

    /// Set the plain-text body.
    pub fn text(mut self, text: impl Into<String>) -> Self {
        self.text = Some(text.into());
        self
    }

    /// Set tags.
    pub fn tags(mut self, tags: Vec<String>) -> Self {
        self.tags = Some(tags);
        self
    }
}

/// Result of a bulk send.
#[derive(Debug, Clone, Deserialize)]
pub struct BulkSendResult {
    /// Number of messages queued.
    pub queued: u64,
    /// Number of contacts skipped.
    pub skipped: u64,
    /// Per-contact error messages.
    pub errors: Vec<String>,
}

// -- suppressions -----------------------------------------------------------

/// A suppressed recipient address.
#[derive(Debug, Clone, Deserialize)]
pub struct Suppression {
    /// Suppression id.
    pub id: String,
    /// The suppressed email (wire field `email_address`).
    pub email_address: String,
    /// Why the address was suppressed.
    pub reason: String,
    /// Creation time.
    #[serde(default)]
    pub created_at: Option<String>,
}

/// Body for adding a suppression. `email` maps to `email_address` on the wire.
#[derive(Debug, Clone, Serialize)]
pub struct AddSuppression {
    /// The email to suppress (serialized as `email_address`).
    #[serde(rename = "email_address")]
    pub email: String,
    /// The reason (defaults to `manual`).
    pub reason: String,
}

impl AddSuppression {
    /// Build a suppression with the default reason `manual`.
    pub fn new(email: impl Into<String>) -> Self {
        Self {
            email: email.into(),
            reason: "manual".to_string(),
        }
    }

    /// Override the reason.
    pub fn reason(mut self, reason: impl Into<String>) -> Self {
        self.reason = reason.into();
        self
    }
}

/// Result of a bulk suppression upload.
#[derive(Debug, Clone, Deserialize)]
pub struct BulkSuppressionResult {
    /// Number of addresses added.
    pub added: u64,
    /// Number skipped (already suppressed).
    pub skipped: u64,
    /// Total lines processed.
    pub total_processed: u64,
}

// -- templates --------------------------------------------------------------

/// A reusable email template. `variables` is server-derived and read-only.
#[derive(Debug, Clone, Deserialize)]
pub struct Template {
    /// Template id.
    pub id: String,
    /// Template name.
    pub name: String,
    /// Subject line.
    pub subject: Option<String>,
    /// HTML body.
    pub html_body: Option<String>,
    /// Plain-text body.
    pub text_body: Option<String>,
    /// Variables derived from `{{placeholders}}` (read-only).
    #[serde(default)]
    pub variables: Option<Vec<String>>,
    /// Editor block structure, if any.
    #[serde(default)]
    pub blocks_json: Option<Value>,
    /// Creation time.
    pub created_at: String,
    /// Last update time.
    pub updated_at: String,
}

/// Body for creating a template. `html`/`text` map to `html_body`/`text_body`.
#[derive(Debug, Clone, Serialize)]
pub struct CreateTemplate {
    /// Template name (required).
    pub name: String,
    /// Subject line.
    #[serde(skip_serializing_if = "Option::is_none")]
    pub subject: Option<String>,
    /// HTML body (serialized as `html_body`).
    #[serde(rename = "html_body", skip_serializing_if = "Option::is_none")]
    pub html: Option<String>,
    /// Plain-text body (serialized as `text_body`).
    #[serde(rename = "text_body", skip_serializing_if = "Option::is_none")]
    pub text: Option<String>,
    /// Editor block structure (serialized as `blocks_json`).
    #[serde(rename = "blocks_json", skip_serializing_if = "Option::is_none")]
    pub blocks_json: Option<Value>,
}

impl CreateTemplate {
    /// Start a create-template body with a name.
    pub fn new(name: impl Into<String>) -> Self {
        Self {
            name: name.into(),
            subject: None,
            html: None,
            text: None,
            blocks_json: None,
        }
    }

    /// Set the subject.
    pub fn subject(mut self, subject: impl Into<String>) -> Self {
        self.subject = Some(subject.into());
        self
    }

    /// Set the HTML body.
    pub fn html(mut self, html: impl Into<String>) -> Self {
        self.html = Some(html.into());
        self
    }

    /// Set the plain-text body.
    pub fn text(mut self, text: impl Into<String>) -> Self {
        self.text = Some(text.into());
        self
    }

    /// Set the editor block structure.
    pub fn blocks_json(mut self, blocks_json: Value) -> Self {
        self.blocks_json = Some(blocks_json);
        self
    }
}

/// Partial body for updating a template. Defaults to all-unset.
#[derive(Debug, Clone, Serialize, Default)]
pub struct UpdateTemplate {
    /// New name.
    #[serde(skip_serializing_if = "Option::is_none")]
    pub name: Option<String>,
    /// New subject.
    #[serde(skip_serializing_if = "Option::is_none")]
    pub subject: Option<String>,
    /// New HTML body (serialized as `html_body`).
    #[serde(rename = "html_body", skip_serializing_if = "Option::is_none")]
    pub html: Option<String>,
    /// New plain-text body (serialized as `text_body`).
    #[serde(rename = "text_body", skip_serializing_if = "Option::is_none")]
    pub text: Option<String>,
    /// New editor block structure (serialized as `blocks_json`).
    #[serde(rename = "blocks_json", skip_serializing_if = "Option::is_none")]
    pub blocks_json: Option<Value>,
}

impl UpdateTemplate {
    /// Set the name.
    pub fn name(mut self, name: impl Into<String>) -> Self {
        self.name = Some(name.into());
        self
    }

    /// Set the subject.
    pub fn subject(mut self, subject: impl Into<String>) -> Self {
        self.subject = Some(subject.into());
        self
    }

    /// Set the HTML body.
    pub fn html(mut self, html: impl Into<String>) -> Self {
        self.html = Some(html.into());
        self
    }

    /// Set the plain-text body.
    pub fn text(mut self, text: impl Into<String>) -> Self {
        self.text = Some(text.into());
        self
    }

    /// Set the editor block structure.
    pub fn blocks_json(mut self, blocks_json: Value) -> Self {
        self.blocks_json = Some(blocks_json);
        self
    }
}

// -- webhooks ---------------------------------------------------------------

/// A configured webhook endpoint. `secret` is returned in plaintext.
#[derive(Debug, Clone, Deserialize)]
pub struct Webhook {
    /// Webhook id.
    pub id: String,
    /// Destination URL.
    pub url: String,
    /// Subscribed event names.
    pub events: Vec<String>,
    /// HMAC signing secret (plaintext on every read).
    pub secret: String,
    /// Whether the webhook is active.
    pub is_active: bool,
    /// Creation time.
    pub created_at: String,
}

/// Body for creating a webhook.
#[derive(Debug, Clone, Serialize)]
pub struct CreateWebhook {
    /// Destination URL (required).
    pub url: String,
    /// Event names to subscribe to (required).
    pub events: Vec<String>,
}

impl CreateWebhook {
    /// Build a create-webhook body.
    pub fn new(url: impl Into<String>, events: Vec<String>) -> Self {
        Self {
            url: url.into(),
            events,
        }
    }
}

/// Partial body for updating a webhook. `is_active` honours the wire name.
#[derive(Debug, Clone, Serialize, Default)]
pub struct UpdateWebhook {
    /// New URL.
    #[serde(skip_serializing_if = "Option::is_none")]
    pub url: Option<String>,
    /// New event list.
    #[serde(skip_serializing_if = "Option::is_none")]
    pub events: Option<Vec<String>>,
    /// New active state (serialized as `is_active`).
    #[serde(rename = "is_active", skip_serializing_if = "Option::is_none")]
    pub is_active: Option<bool>,
}

impl UpdateWebhook {
    /// Set the URL.
    pub fn url(mut self, url: impl Into<String>) -> Self {
        self.url = Some(url.into());
        self
    }

    /// Set the event list.
    pub fn events(mut self, events: Vec<String>) -> Self {
        self.events = Some(events);
        self
    }

    /// Set the active state.
    pub fn is_active(mut self, is_active: bool) -> Self {
        self.is_active = Some(is_active);
        self
    }
}

/// Result of testing a webhook endpoint.
#[derive(Debug, Clone, Deserialize)]
pub struct WebhookTestResult {
    /// Whether a test delivery was queued.
    pub queued: bool,
    /// The destination URL.
    pub url: String,
}

/// A summary of one webhook delivery attempt.
#[derive(Debug, Clone, Deserialize)]
pub struct WebhookDelivery {
    /// Delivery id.
    pub id: String,
    /// The webhook it belongs to.
    pub webhook_id: String,
    /// The event type, if known.
    pub event_type: Option<String>,
    /// Delivery status.
    pub status: String,
    /// HTTP status the endpoint returned, if any.
    pub response_status: Option<i64>,
    /// Attempt number.
    pub attempt: i64,
    /// When the next retry is scheduled, if any.
    pub next_retry_at: Option<String>,
    /// Creation time.
    pub created_at: Option<String>,
}

/// A webhook delivery with the full payload and endpoint response.
#[derive(Debug, Clone, Deserialize)]
pub struct WebhookDeliveryDetail {
    /// The base delivery.
    #[serde(flatten)]
    pub delivery: WebhookDelivery,
    /// The delivered payload.
    pub payload: Value,
    /// The endpoint's response body, if any.
    pub response_body: Option<String>,
    /// The endpoint URL.
    pub endpoint_url: String,
}

// -- domain transfer params -------------------------------------------------

/// Body for initiating a domain transfer.
#[derive(Debug, Clone, Serialize)]
pub struct TransferDomain {
    /// The recipient email (required).
    pub target_email: String,
    /// An optional note (max 1000 chars).
    #[serde(skip_serializing_if = "Option::is_none")]
    pub note: Option<String>,
}

impl TransferDomain {
    /// Build a transfer body with a target email.
    pub fn new(target_email: impl Into<String>) -> Self {
        Self {
            target_email: target_email.into(),
            note: None,
        }
    }

    /// Attach a note.
    pub fn note(mut self, note: impl Into<String>) -> Self {
        self.note = Some(note.into());
        self
    }
}
