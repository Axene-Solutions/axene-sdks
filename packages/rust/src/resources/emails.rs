//! The `emails` resource: send, look up, search, schedule, and inspect messages.

use reqwest::Method;
use serde::Deserialize;
use serde_json::Value;

use super::{query, urlencode};
use crate::error::Result;
use crate::http::HttpTransport;
use crate::models::{
    BatchResponse, Email, EmailDetail, EmailEvent, EmailSearchHit, IdStatus, ScheduledEmail,
    SendEmail, SendEmailResponse, ValidationResult,
};

/// Accessed as `client.emails()`.
#[derive(Debug, Clone)]
pub struct Emails {
    http: HttpTransport,
}

#[derive(Deserialize)]
struct SavedSearches {
    searches: Vec<Value>,
}

impl Emails {
    pub(crate) fn new(http: HttpTransport) -> Self {
        Self { http }
    }

    /// Send a single email.
    pub async fn send(&self, message: &SendEmail) -> Result<SendEmailResponse> {
        self.http
            .request_json(Method::POST, "/v1/emails/", message)
            .await
    }

    /// Send up to your plan's batch limit in one call. The API accepts a bare
    /// array of messages and returns a per-message result set.
    pub async fn send_batch(&self, messages: &[SendEmail]) -> Result<BatchResponse> {
        self.http
            .request_json(Method::POST, "/v1/emails/batch", &messages)
            .await
    }

    /// Dry-run a send: check whether the message would be accepted without
    /// actually sending it.
    pub async fn validate(&self, message: &SendEmail) -> Result<ValidationResult> {
        self.http
            .request_json(Method::POST, "/v1/emails/validate", message)
            .await
    }

    /// List recent emails, newest first. `page` is zero-based.
    pub async fn list(
        &self,
        status: Option<&str>,
        page: Option<u64>,
        limit: Option<u64>,
    ) -> Result<Vec<Email>> {
        let q = query(&[
            ("status", status.map(String::from)),
            ("page", page.map(|p| p.to_string())),
            ("limit", limit.map(|l| l.to_string())),
        ]);
        self.http
            .request(Method::GET, &format!("/v1/emails/{q}"))
            .await
    }

    /// Fetch a single email with its bodies and events.
    pub async fn get(&self, id: &str) -> Result<EmailDetail> {
        self.http
            .request(Method::GET, &format!("/v1/emails/{}", urlencode(id)))
            .await
    }

    /// List delivery / open / click / bounce events for an email.
    pub async fn events(&self, id: &str) -> Result<Vec<EmailEvent>> {
        self.http
            .request(
                Method::GET,
                &format!("/v1/emails/{}/events", urlencode(id)),
            )
            .await
    }

    /// Re-send a bounced, rejected, or failed email as a new message.
    pub async fn retry(&self, id: &str) -> Result<SendEmailResponse> {
        self.http
            .request(Method::POST, &format!("/v1/emails/{}/retry", urlencode(id)))
            .await
    }

    /// Search emails. `q` supports inline tokens (`to:`, `from:`, `status:`,
    /// `domain:`, `tag:`); leftover words are matched as free text.
    pub async fn search(
        &self,
        q: Option<&str>,
        status: Option<&str>,
        tag: Option<&str>,
        page: Option<u64>,
        limit: Option<u64>,
    ) -> Result<Vec<EmailSearchHit>> {
        let qs = query(&[
            ("q", q.map(String::from)),
            ("status", status.map(String::from)),
            ("tag", tag.map(String::from)),
            ("page", page.map(|p| p.to_string())),
            ("limit", limit.map(|l| l.to_string())),
        ]);
        self.http
            .request(Method::GET, &format!("/v1/emails/search{qs}"))
            .await
    }

    /// List emails scheduled for future delivery, soonest first.
    pub async fn list_scheduled(&self) -> Result<Vec<ScheduledEmail>> {
        self.http
            .request(Method::GET, "/v1/emails/scheduled")
            .await
    }

    /// Cancel a scheduled email.
    pub async fn cancel_scheduled(&self, id: &str) -> Result<IdStatus> {
        self.http
            .request(
                Method::DELETE,
                &format!("/v1/emails/scheduled/{}", urlencode(id)),
            )
            .await
    }

    /// Send a scheduled email immediately instead of waiting.
    pub async fn send_scheduled_now(&self, id: &str) -> Result<IdStatus> {
        self.http
            .request(
                Method::POST,
                &format!("/v1/emails/scheduled/{}/send-now", urlencode(id)),
            )
            .await
    }

    /// Poll for emails whose status changed at or after `since` (ISO 8601).
    /// Capped at 50 rows.
    pub async fn updates(&self, since: &str) -> Result<Vec<Email>> {
        let q = query(&[("since", Some(since.to_string()))]);
        self.http
            .request(Method::GET, &format!("/v1/emails/updates{q}"))
            .await
    }

    /// Get the caller's saved searches.
    pub async fn get_saved_searches(&self) -> Result<Vec<Value>> {
        let r: SavedSearches = self
            .http
            .request(Method::GET, "/v1/emails/saved-searches")
            .await?;
        Ok(r.searches)
    }

    /// Replace the caller's saved searches (max 50).
    pub async fn set_saved_searches(&self, searches: Vec<Value>) -> Result<Vec<Value>> {
        let body = serde_json::json!({ "searches": searches });
        let r: SavedSearches = self
            .http
            .request_json(Method::PUT, "/v1/emails/saved-searches", &body)
            .await?;
        Ok(r.searches)
    }
}
