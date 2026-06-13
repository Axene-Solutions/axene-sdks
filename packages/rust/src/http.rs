//! HTTP transport: the single place that talks to the network.
//!
//! Owns authentication (bearer), JSON encode/decode, retries with backoff on
//! `429`/`5xx` (honouring `Retry-After`), multipart upload, and turning non-2xx
//! responses into [`AxeneError`]. Resources depend on this, not on `reqwest`
//! directly.

use std::time::Duration;

use reqwest::{multipart, Client, Method, Response, StatusCode};
use serde::de::DeserializeOwned;
use serde::Serialize;
use serde_json::Value;

use crate::error::{AxeneError, Result};

const DEFAULT_BASE: &str = "https://mail.axene.io";
const USER_AGENT: &str = concat!("axene-mailer-rust/", env!("CARGO_PKG_VERSION"));

/// The network transport shared by every resource.
#[derive(Debug, Clone)]
pub struct HttpTransport {
    client: Client,
    api_key: String,
    base_url: String,
    max_retries: u32,
}

impl HttpTransport {
    /// Build a transport. `base_url` has any trailing slashes trimmed.
    pub(crate) fn new(
        api_key: String,
        base_url: Option<String>,
        max_retries: u32,
        timeout: Duration,
    ) -> Result<Self> {
        if api_key.is_empty() {
            return Err(AxeneError::transport("Axene: `api_key` is required."));
        }
        let base = base_url.unwrap_or_else(|| DEFAULT_BASE.to_string());
        let base = base.trim_end_matches('/').to_string();
        let client = Client::builder()
            .timeout(timeout)
            .build()
            .map_err(|e| AxeneError::transport(format!("failed to build HTTP client: {e}")))?;
        Ok(Self {
            client,
            api_key,
            base_url: base,
            // At least one attempt.
            max_retries: max_retries.max(1),
        })
    }

    /// Send a request with no body and decode the JSON response.
    pub(crate) async fn request<T: DeserializeOwned>(
        &self,
        method: Method,
        path: &str,
    ) -> Result<T> {
        self.send::<(), T>(method, path, None).await
    }

    /// Send a request with a JSON body and decode the JSON response.
    pub(crate) async fn request_json<B: Serialize, T: DeserializeOwned>(
        &self,
        method: Method,
        path: &str,
        body: &B,
    ) -> Result<T> {
        self.send(method, path, Some(body)).await
    }

    /// Send a request that returns no content (204 / empty body).
    pub(crate) async fn request_empty(&self, method: Method, path: &str) -> Result<()> {
        let _: IgnoredBody = self.send::<(), IgnoredBody>(method, path, None).await?;
        Ok(())
    }

    async fn send<B: Serialize, T: DeserializeOwned>(
        &self,
        method: Method,
        path: &str,
        body: Option<&B>,
    ) -> Result<T> {
        let url = format!("{}{}", self.base_url, path);
        let mut last_error: Option<AxeneError> = None;

        for attempt in 1..=self.max_retries {
            let mut builder = self
                .client
                .request(method.clone(), &url)
                .bearer_auth(&self.api_key)
                .header(reqwest::header::USER_AGENT, USER_AGENT);
            if let Some(b) = body {
                builder = builder.json(b);
            }

            match builder.send().await {
                Ok(res) => {
                    let status = res.status();
                    if is_retryable(status) && attempt < self.max_retries {
                        let wait = backoff(Some(&res), attempt);
                        last_error = Some(error_placeholder(status));
                        tokio::time::sleep(wait).await;
                        continue;
                    }
                    return decode(res).await;
                }
                Err(err) => {
                    last_error = Some(AxeneError::transport(format!(
                        "Axene request failed: {err}"
                    )));
                    if attempt < self.max_retries {
                        tokio::time::sleep(backoff(None, attempt)).await;
                        continue;
                    }
                }
            }
        }

        Err(last_error
            .unwrap_or_else(|| AxeneError::transport("Axene request failed: no attempts made")))
    }

    /// Upload a single file as `multipart/form-data` under the field `file`.
    ///
    /// Not retried (uploads are not idempotent). The runtime sets the multipart
    /// boundary; we do not set `Content-Type` manually.
    pub(crate) async fn upload<T: DeserializeOwned>(
        &self,
        path: &str,
        file: Vec<u8>,
        filename: &str,
    ) -> Result<T> {
        let url = format!("{}{}", self.base_url, path);
        let part = multipart::Part::bytes(file).file_name(filename.to_string());
        let form = multipart::Form::new().part("file", part);

        let res = self
            .client
            .post(&url)
            .bearer_auth(&self.api_key)
            .header(reqwest::header::USER_AGENT, USER_AGENT)
            .multipart(form)
            .send()
            .await
            .map_err(|e| AxeneError::transport(format!("Axene upload failed: {e}")))?;
        decode(res).await
    }
}

/// A unit-like type that deserializes from any (or empty) body.
#[derive(serde::Deserialize)]
struct IgnoredBody {
    #[serde(flatten)]
    #[allow(dead_code)]
    rest: std::collections::HashMap<String, Value>,
}

fn is_retryable(status: StatusCode) -> bool {
    status == StatusCode::TOO_MANY_REQUESTS || status.is_server_error()
}

fn backoff(res: Option<&Response>, attempt: u32) -> Duration {
    if let Some(res) = res {
        if let Some(secs) = res
            .headers()
            .get(reqwest::header::RETRY_AFTER)
            .and_then(|v| v.to_str().ok())
            .and_then(|v| v.trim().parse::<f64>().ok())
        {
            if secs > 0.0 {
                return Duration::from_secs_f64(secs);
            }
        }
    }
    // 250ms, 500ms, 1s, ...
    Duration::from_millis(250 * 2u64.pow(attempt.saturating_sub(1)))
}

/// Used only to record the last status when a retry loop exhausts.
fn error_placeholder(status: StatusCode) -> AxeneError {
    AxeneError::new(
        status.as_u16(),
        format!("Axene request failed ({})", status.as_u16()),
        None,
    )
}

/// Decode a response: success bodies parse into `T`, errors map to [`AxeneError`].
async fn decode<T: DeserializeOwned>(res: Response) -> Result<T> {
    let status = res.status();
    let bytes = res
        .bytes()
        .await
        .map_err(|e| AxeneError::transport(format!("failed to read response body: {e}")))?;

    if status.is_success() {
        if bytes.is_empty() {
            // 204 No Content, or an empty body for a `()`/unit decode.
            return serde_json::from_slice(b"{}").map_err(|e| {
                AxeneError::transport(format!("failed to decode empty body: {e}"))
            });
        }
        return serde_json::from_slice(&bytes).map_err(|e| {
            AxeneError::transport(format!("failed to decode response: {e}"))
        });
    }

    Err(to_error(status.as_u16(), &bytes))
}

/// Map the API's `{ detail: { code, message } }` (or string) into an error.
fn to_error(status: u16, bytes: &[u8]) -> AxeneError {
    let payload: Option<Value> = serde_json::from_slice(bytes).ok();
    let detail = payload.as_ref().and_then(|p| p.get("detail"));

    let mut code = None;
    let mut message = None;

    if let Some(detail) = detail {
        if let Some(obj) = detail.as_object() {
            code = obj.get("code").and_then(|c| c.as_str()).map(String::from);
            message = obj.get("message").and_then(|m| m.as_str()).map(String::from);
        } else if let Some(s) = detail.as_str() {
            message = Some(s.to_string());
        }
    }

    AxeneError::new(
        status,
        message.unwrap_or_else(|| format!("Axene request failed ({status})")),
        code,
    )
}
