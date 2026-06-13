//! The Axene client. Composes the HTTP transport with the resource handles.

use std::time::Duration;

use crate::error::Result;
use crate::http::HttpTransport;
use crate::resources::{Contacts, Domains, Emails, Suppressions, Templates, Webhooks};

const DEFAULT_TIMEOUT_SECS: u64 = 30;
const DEFAULT_MAX_RETRIES: u32 = 3;

/// Builder for [`Axene`]. Construct with [`Axene::builder`].
#[derive(Debug, Clone)]
pub struct AxeneBuilder {
    api_key: String,
    base_url: Option<String>,
    max_retries: u32,
    timeout: Duration,
}

impl AxeneBuilder {
    /// Override the API base URL. Defaults to `https://mail.axene.io`.
    pub fn base_url(mut self, base_url: impl Into<String>) -> Self {
        self.base_url = Some(base_url.into());
        self
    }

    /// Total attempts on `429` / `5xx`, including the first. Defaults to `3`.
    pub fn max_retries(mut self, max_retries: u32) -> Self {
        self.max_retries = max_retries;
        self
    }

    /// Per-request timeout. Defaults to 30 seconds.
    pub fn timeout(mut self, timeout: Duration) -> Self {
        self.timeout = timeout;
        self
    }

    /// Build the client.
    pub fn build(self) -> Result<Axene> {
        let http = HttpTransport::new(self.api_key, self.base_url, self.max_retries, self.timeout)?;
        Ok(Axene {
            emails: Emails::new(http.clone()),
            domains: Domains::new(http.clone()),
            contacts: Contacts::new(http.clone()),
            suppressions: Suppressions::new(http.clone()),
            templates: Templates::new(http.clone()),
            webhooks: Webhooks::new(http),
        })
    }
}

/// Axene Mailer API client.
///
/// # Example
///
/// ```no_run
/// use axene_mailer::{Axene, SendEmail};
///
/// # async fn run() -> Result<(), axene_mailer::AxeneError> {
/// let client = Axene::new("axm_k_...")?;
/// let message = SendEmail::builder("hello@yourdomain.com", "customer@example.com", "Your receipt")
///     .html("<p>Thanks for your order.</p>")
///     .build();
/// let res = client.emails().send(&message).await?;
/// println!("queued {}", res.id);
/// # Ok(())
/// # }
/// ```
#[derive(Debug, Clone)]
pub struct Axene {
    emails: Emails,
    domains: Domains,
    contacts: Contacts,
    suppressions: Suppressions,
    templates: Templates,
    webhooks: Webhooks,
}

impl Axene {
    /// Build a client with the default base URL, retries, and timeout.
    ///
    /// The API key must start with `axm_k_`.
    pub fn new(api_key: impl Into<String>) -> Result<Self> {
        Self::builder(api_key).build()
    }

    /// Start configuring a client.
    pub fn builder(api_key: impl Into<String>) -> AxeneBuilder {
        AxeneBuilder {
            api_key: api_key.into(),
            base_url: None,
            max_retries: DEFAULT_MAX_RETRIES,
            timeout: Duration::from_secs(DEFAULT_TIMEOUT_SECS),
        }
    }

    /// Send, search, schedule, and inspect emails.
    pub fn emails(&self) -> &Emails {
        &self.emails
    }

    /// Register, verify, and transfer sending domains.
    pub fn domains(&self) -> &Domains {
        &self.domains
    }

    /// Manage subscriber lists and bulk sends.
    pub fn contacts(&self) -> &Contacts {
        &self.contacts
    }

    /// Manage the do-not-send suppression list.
    pub fn suppressions(&self) -> &Suppressions {
        &self.suppressions
    }

    /// Manage reusable email templates.
    pub fn templates(&self) -> &Templates {
        &self.templates
    }

    /// Manage event webhooks and inspect deliveries.
    pub fn webhooks(&self) -> &Webhooks {
        &self.webhooks
    }
}
