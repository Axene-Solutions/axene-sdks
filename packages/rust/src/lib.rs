//! Async Rust SDK for the [Axene Mailer](https://mail.axene.io) API.
//!
//! Send transactional and bulk email, manage sending domains, subscriber
//! lists, suppressions, templates, and webhooks. Built on `reqwest` + `serde`,
//! with a single transport layer that owns bearer auth, JSON, retries on
//! `429`/`5xx` (honouring `Retry-After`), and error mapping to [`AxeneError`].
//!
//! # Quickstart
//!
//! ```no_run
//! use axene_mailer::{Axene, SendEmail};
//!
//! #[tokio::main]
//! async fn main() -> Result<(), axene_mailer::AxeneError> {
//!     let client = Axene::new(std::env::var("AXENE_API_KEY").unwrap())?;
//!
//!     let message = SendEmail::builder(
//!         ("hello@yourdomain.com", "Your Company"),
//!         "customer@example.com",
//!         "Your receipt",
//!     )
//!     .html("<p>Thanks for your order.</p>")
//!     .build();
//!
//!     let res = client.emails().send(&message).await?;
//!     println!("queued message {}", res.id);
//!     Ok(())
//! }
//! ```
//!
//! Resources are reached through accessor methods: [`Axene::emails`],
//! [`Axene::domains`], [`Axene::contacts`], [`Axene::suppressions`],
//! [`Axene::templates`], and [`Axene::webhooks`].

#![forbid(unsafe_code)]
#![warn(missing_docs)]

mod client;
mod error;
mod http;
mod models;
mod resources;

pub use client::{Axene, AxeneBuilder};
pub use error::{AxeneError, Result};
pub use models::*;
pub use resources::{Contacts, Domains, Emails, Suppressions, Templates, Webhooks};
