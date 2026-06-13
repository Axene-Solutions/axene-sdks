//! The `templates` resource: reusable email templates (Starter plan and up).

use reqwest::Method;

use super::urlencode;
use crate::error::Result;
use crate::http::HttpTransport;
use crate::models::{CreateTemplate, Template, UpdateTemplate};

/// Accessed as `client.templates()`.
#[derive(Debug, Clone)]
pub struct Templates {
    http: HttpTransport,
}

impl Templates {
    pub(crate) fn new(http: HttpTransport) -> Self {
        Self { http }
    }

    /// List all templates, most recently updated first.
    pub async fn list(&self) -> Result<Vec<Template>> {
        self.http.request(Method::GET, "/v1/templates/").await
    }

    /// Create a template. `variables` are derived server-side from `{{name}}`
    /// placeholders, so they are not passed.
    pub async fn create(&self, params: &CreateTemplate) -> Result<Template> {
        self.http
            .request_json(Method::POST, "/v1/templates/", params)
            .await
    }

    /// Fetch a single template.
    pub async fn get(&self, id: &str) -> Result<Template> {
        self.http
            .request(Method::GET, &format!("/v1/templates/{}", urlencode(id)))
            .await
    }

    /// Update a template (partial).
    pub async fn update(&self, id: &str, params: &UpdateTemplate) -> Result<Template> {
        self.http
            .request_json(
                Method::PATCH,
                &format!("/v1/templates/{}", urlencode(id)),
                params,
            )
            .await
    }

    /// Delete a template.
    pub async fn delete(&self, id: &str) -> Result<()> {
        self.http
            .request_empty(Method::DELETE, &format!("/v1/templates/{}", urlencode(id)))
            .await
    }

    /// Duplicate a template (the copy's `blocks_json` is not carried over).
    pub async fn duplicate(&self, id: &str) -> Result<Template> {
        self.http
            .request(
                Method::POST,
                &format!("/v1/templates/{}/duplicate", urlencode(id)),
            )
            .await
    }
}
