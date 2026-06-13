//! The `suppressions` resource: manage the do-not-send list.

use reqwest::Method;

use super::{query, urlencode};
use crate::error::Result;
use crate::http::HttpTransport;
use crate::models::{AddSuppression, BulkSuppressionResult, Page, Suppression};

/// Accessed as `client.suppressions()`.
#[derive(Debug, Clone)]
pub struct Suppressions {
    http: HttpTransport,
}

impl Suppressions {
    pub(crate) fn new(http: HttpTransport) -> Self {
        Self { http }
    }

    /// List suppressed addresses (paginated envelope; zero-based `page`).
    pub async fn list(
        &self,
        page: Option<u64>,
        limit: Option<u64>,
        search: Option<&str>,
    ) -> Result<Page<Suppression>> {
        let q = query(&[
            ("page", page.map(|p| p.to_string())),
            ("limit", limit.map(|l| l.to_string())),
            ("search", search.map(String::from)),
        ]);
        self.http
            .request(Method::GET, &format!("/v1/suppressions{q}"))
            .await
    }

    /// Suppress a single address.
    pub async fn add(&self, params: &AddSuppression) -> Result<Suppression> {
        self.http
            .request_json(Method::POST, "/v1/suppressions", params)
            .await
    }

    /// Bulk-import suppressions from a file (one email per line). Sent as
    /// `multipart/form-data` under the field `file`.
    pub async fn bulk_upload(
        &self,
        file: Vec<u8>,
        filename: &str,
    ) -> Result<BulkSuppressionResult> {
        self.http
            .upload("/v1/suppressions/bulk", file, filename)
            .await
    }

    /// Remove an address from the suppression list.
    pub async fn remove(&self, id: &str) -> Result<()> {
        self.http
            .request_empty(
                Method::DELETE,
                &format!("/v1/suppressions/{}", urlencode(id)),
            )
            .await
    }
}
