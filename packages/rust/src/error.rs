//! Error type raised by the SDK.

use std::fmt;

/// Raised for any non-2xx API response, or for a transport failure that
/// survives all retries.
///
/// Inspect [`AxeneError::status`] and [`AxeneError::code`] to branch on
/// specific failures (for example a `422` with code `"invalid"`). A `status`
/// of `0` indicates a transport/network failure with no HTTP response.
#[derive(Debug, Clone)]
pub struct AxeneError {
    /// HTTP status code. `0` indicates a transport/network failure (no response).
    pub status: u16,
    /// Machine-readable error code from the API body, when present.
    pub code: Option<String>,
    /// Human-readable error message.
    pub message: String,
}

impl AxeneError {
    /// Build an error with an explicit status, message, and optional code.
    pub fn new(status: u16, message: impl Into<String>, code: Option<String>) -> Self {
        Self {
            status,
            code,
            message: message.into(),
        }
    }

    /// Build a transport-level error (no HTTP response was received).
    pub fn transport(message: impl Into<String>) -> Self {
        Self {
            status: 0,
            code: None,
            message: message.into(),
        }
    }
}

impl fmt::Display for AxeneError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match &self.code {
            Some(code) => write!(f, "Axene error {} ({}): {}", self.status, code, self.message),
            None => write!(f, "Axene error {}: {}", self.status, self.message),
        }
    }
}

impl std::error::Error for AxeneError {}

/// Convenience alias for results returned by the SDK.
pub type Result<T> = std::result::Result<T, AxeneError>;
