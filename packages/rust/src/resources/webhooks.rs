//! The `webhooks` resource: manage event subscriptions and inspect deliveries.

use reqwest::Method;

use super::{query, urlencode};
use crate::error::Result;
use crate::http::HttpTransport;
use crate::models::{
    CreateWebhook, Page, UpdateWebhook, Webhook, WebhookDelivery, WebhookDeliveryDetail,
    WebhookTestResult,
};

/// Accessed as `client.webhooks()`.
#[derive(Debug, Clone)]
pub struct Webhooks {
    http: HttpTransport,
}

impl Webhooks {
    pub(crate) fn new(http: HttpTransport) -> Self {
        Self { http }
    }

    /// List your active webhooks.
    pub async fn list(&self) -> Result<Vec<Webhook>> {
        self.http.request(Method::GET, "/v1/webhooks/").await
    }

    /// Create a webhook. The signing `secret` is generated and returned.
    pub async fn create(&self, params: &CreateWebhook) -> Result<Webhook> {
        self.http
            .request_json(Method::POST, "/v1/webhooks/", params)
            .await
    }

    /// Update a webhook's url, events, or active state (partial).
    pub async fn update(&self, id: &str, params: &UpdateWebhook) -> Result<Webhook> {
        self.http
            .request_json(
                Method::PATCH,
                &format!("/v1/webhooks/{}", urlencode(id)),
                params,
            )
            .await
    }

    /// Delete a webhook.
    pub async fn delete(&self, id: &str) -> Result<()> {
        self.http
            .request_empty(Method::DELETE, &format!("/v1/webhooks/{}", urlencode(id)))
            .await
    }

    /// Queue a sample `email.delivered` delivery to test the endpoint.
    pub async fn test(&self, id: &str) -> Result<WebhookTestResult> {
        self.http
            .request(
                Method::POST,
                &format!("/v1/webhooks/{}/test", urlencode(id)),
            )
            .await
    }

    /// List delivery attempts for a webhook (paginated envelope).
    pub async fn list_deliveries(
        &self,
        id: &str,
        page: Option<u64>,
        limit: Option<u64>,
        status: Option<&str>,
    ) -> Result<Page<WebhookDelivery>> {
        let q = query(&[
            ("page", page.map(|p| p.to_string())),
            ("limit", limit.map(|l| l.to_string())),
            ("status", status.map(String::from)),
        ]);
        self.http
            .request(
                Method::GET,
                &format!("/v1/webhooks/{}/deliveries{q}", urlencode(id)),
            )
            .await
    }

    /// Fetch one delivery with its full payload and the endpoint's response.
    pub async fn get_delivery(
        &self,
        id: &str,
        delivery_id: &str,
    ) -> Result<WebhookDeliveryDetail> {
        self.http
            .request(
                Method::GET,
                &format!(
                    "/v1/webhooks/{}/deliveries/{}",
                    urlencode(id),
                    urlencode(delivery_id)
                ),
            )
            .await
    }
}
