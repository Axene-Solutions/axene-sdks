//! The `domains` resource: register, verify, inspect, and transfer domains.

use reqwest::Method;
use serde_json::Value;

use super::{query, urlencode};
use crate::error::Result;
use crate::http::HttpTransport;
use crate::models::{
    DkimRotation, Domain, DomainAvailability, DomainCheck, DomainDiagnosis, DomainHealth,
    DomainListItem, DomainTransfer, TransferDomain,
};

/// Accessed as `client.domains()`.
#[derive(Debug, Clone)]
pub struct Domains {
    http: HttpTransport,
}

impl Domains {
    pub(crate) fn new(http: HttpTransport) -> Self {
        Self { http }
    }

    /// List your sending domains and their verification status.
    pub async fn list(&self) -> Result<Vec<DomainListItem>> {
        self.http.request(Method::GET, "/v1/domains/").await
    }

    /// Register a new sending domain. Returns the DNS records to publish.
    pub async fn create(&self, name: &str) -> Result<Domain> {
        let body = serde_json::json!({ "name": name });
        self.http
            .request_json(Method::POST, "/v1/domains/", &body)
            .await
    }

    /// Fetch a domain with its DKIM selector and DNS records.
    pub async fn get(&self, id: &str) -> Result<Domain> {
        self.http
            .request(Method::GET, &format!("/v1/domains/{}", urlencode(id)))
            .await
    }

    /// Delete a domain.
    pub async fn delete(&self, id: &str) -> Result<()> {
        self.http
            .request_empty(Method::DELETE, &format!("/v1/domains/{}", urlencode(id)))
            .await
    }

    /// Re-check DNS and verify the domain.
    pub async fn verify(&self, id: &str) -> Result<Domain> {
        self.http
            .request(
                Method::POST,
                &format!("/v1/domains/{}/verify", urlencode(id)),
            )
            .await
    }

    /// Run live DNS health checks (DKIM, SPF, DMARC, return-path, MX).
    pub async fn health(&self, id: &str) -> Result<DomainHealth> {
        self.http
            .request(
                Method::GET,
                &format!("/v1/domains/{}/health", urlencode(id)),
            )
            .await
    }

    /// Diagnose configuration issues and get a health score.
    pub async fn diagnose(&self, id: &str) -> Result<DomainDiagnosis> {
        self.http
            .request(
                Method::GET,
                &format!("/v1/domains/{}/diagnose", urlencode(id)),
            )
            .await
    }

    /// Current MX status for inbound/forwarding (shape varies by provider).
    pub async fn mx_status(&self, id: &str) -> Result<Value> {
        self.http
            .request(
                Method::GET,
                &format!("/v1/domains/{}/mx-status", urlencode(id)),
            )
            .await
    }

    /// The values currently published in DNS for each of the domain's records.
    pub async fn published_records(&self, id: &str) -> Result<Value> {
        self.http
            .request(
                Method::GET,
                &format!("/v1/domains/{}/published-records", urlencode(id)),
            )
            .await
    }

    /// Rotate the domain's DKIM key, returning the new record to publish.
    pub async fn rotate_dkim(&self, id: &str) -> Result<DkimRotation> {
        self.http
            .request(
                Method::POST,
                &format!("/v1/domains/{}/rotate-dkim", urlencode(id)),
            )
            .await
    }

    /// Initiate a transfer of this domain to another Axene account.
    pub async fn transfer(&self, id: &str, params: &TransferDomain) -> Result<DomainTransfer> {
        self.http
            .request_json(
                Method::POST,
                &format!("/v1/domains/{}/transfer", urlencode(id)),
                params,
            )
            .await
    }

    /// Check whether a domain name is available to add (checks public DNS).
    pub async fn check_availability(&self, name: &str) -> Result<DomainAvailability> {
        let q = query(&[("name", Some(name.to_string()))]);
        self.http
            .request(Method::GET, &format!("/v1/domains/check-availability{q}"))
            .await
    }

    /// Check whether a domain name already exists in your account.
    pub async fn check(&self, name: &str) -> Result<DomainCheck> {
        self.http
            .request(
                Method::GET,
                &format!("/v1/domains/check/{}", urlencode(name)),
            )
            .await
    }
}
