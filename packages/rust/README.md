# axene-mailer (Rust)

Async Rust SDK for the [Axene Mailer](https://mail.axene.io) API. Send
transactional and bulk email, manage sending domains, subscriber lists,
suppressions, templates, and webhooks.

Built on `reqwest` and `serde`, with a single transport layer that handles
bearer auth, JSON encoding/decoding, retries on `429`/`5xx` with backoff
(honouring `Retry-After`), multipart uploads, and error mapping.

## Install

```toml
[dependencies]
axene-mailer = "0.1"
tokio = { version = "1", features = ["full"] }
```

## Quickstart

```rust
use axene_mailer::{Axene, SendEmail};

#[tokio::main]
async fn main() -> Result<(), axene_mailer::AxeneError> {
    let client = Axene::new(std::env::var("AXENE_API_KEY").unwrap())?;

    let message = SendEmail::builder(
        ("hello@yourdomain.com", "Your Company"),
        "customer@example.com",
        "Your receipt",
    )
    .html("<p>Thanks for your order.</p>")
    .text("Thanks for your order.")
    .tags(vec!["receipt".into()])
    .build();

    let res = client.emails().send(&message).await?;
    println!("queued message {}", res.id);
    Ok(())
}
```

The API key starts with `axm_k_`. The default base URL is
`https://mail.axene.io`; override it (and retries/timeout) via the builder:

```rust
use std::time::Duration;
use axene_mailer::Axene;

let client = Axene::builder("axm_k_...")
    .base_url("https://staging.mail.axene.io")
    .max_retries(5)
    .timeout(Duration::from_secs(60))
    .build()?;
# Ok::<(), axene_mailer::AxeneError>(())
```

## Resources

Reach each resource through an accessor on the client.

### emails

```rust
# async fn demo(client: &axene_mailer::Axene) -> Result<(), axene_mailer::AxeneError> {
use axene_mailer::SendEmail;

let a = SendEmail::builder("from@ex.com", "a@ex.com", "Hi A").text("hi").build();
let b = SendEmail::builder("from@ex.com", "b@ex.com", "Hi B").text("hi").build();

client.emails().send(&a).await?;                       // single send
client.emails().send_batch(&[a, b]).await?;            // bare-array batch
client.emails().validate(&SendEmail::builder("from@ex.com", "c@ex.com", "Check").build()).await?;
client.emails().list(None, Some(0), Some(20)).await?;  // page is zero-based
client.emails().get("em_123").await?;
client.emails().events("em_123").await?;
client.emails().retry("em_123").await?;
client.emails().search(Some("status:bounced"), None, None, Some(0), Some(20)).await?;
client.emails().list_scheduled().await?;
client.emails().cancel_scheduled("em_123").await?;
client.emails().send_scheduled_now("em_123").await?;
client.emails().updates("2026-06-13T00:00:00Z").await?;
client.emails().get_saved_searches().await?;
# Ok(())
# }
```

### domains

```rust
# async fn demo(client: &axene_mailer::Axene) -> Result<(), axene_mailer::AxeneError> {
use axene_mailer::TransferDomain;

client.domains().list().await?;
let domain = client.domains().create("send.yourdomain.com").await?;
client.domains().verify(&domain.id).await?;
client.domains().health(&domain.id).await?;
client.domains().rotate_dkim(&domain.id).await?;
client.domains().transfer(&domain.id, &TransferDomain::new("new-owner@ex.com").note("handover")).await?;
client.domains().check_availability("send.yourdomain.com").await?;
# Ok(())
# }
```

`mx_status` and `published_records` return `serde_json::Value` because their
shapes vary by provider.

### contacts

```rust
# async fn demo(client: &axene_mailer::Axene) -> Result<(), axene_mailer::AxeneError> {
use axene_mailer::{AddContact, BulkSend, CreateList};

let list = client.contacts().create_list(&CreateList::new("Newsletter")).await?;
client.contacts().add_contact(&list.id, &AddContact::new("reader@ex.com").name("Reader")).await?;
client.contacts().upload_csv(&list.id, std::fs::read("contacts.csv").unwrap(), "contacts.csv").await?;
client.contacts().bulk_send(&list.id, &BulkSend::new("snd_1", "Hello {{name}}").html("<p>hi {{name}}</p>")).await?;
# Ok(())
# }
```

`bulk_send` sets `contact_list_id` to the given list id automatically.

### suppressions

```rust
# async fn demo(client: &axene_mailer::Axene) -> Result<(), axene_mailer::AxeneError> {
use axene_mailer::AddSuppression;

let page = client.suppressions().list(Some(0), Some(50), None).await?;  // envelope
println!("{} of {}", page.items.len(), page.total);
client.suppressions().add(&AddSuppression::new("blocked@ex.com").reason("bounce")).await?;
client.suppressions().bulk_upload(b"a@ex.com\nb@ex.com\n".to_vec(), "list.txt").await?;
# Ok(())
# }
```

### templates

```rust
# async fn demo(client: &axene_mailer::Axene) -> Result<(), axene_mailer::AxeneError> {
use axene_mailer::CreateTemplate;

client.templates().create(&CreateTemplate::new("Welcome").html("<p>Hi {{name}}</p>")).await?;
client.templates().list().await?;
client.templates().duplicate("tpl_1").await?;
# Ok(())
# }
```

### webhooks

```rust
# async fn demo(client: &axene_mailer::Axene) -> Result<(), axene_mailer::AxeneError> {
use axene_mailer::{CreateWebhook, UpdateWebhook};

let wh = client.webhooks().create(&CreateWebhook::new("https://ex.com/hook", vec!["email.delivered".into()])).await?;
client.webhooks().update(&wh.id, &UpdateWebhook::default().is_active(false)).await?;
client.webhooks().test(&wh.id).await?;
let deliveries = client.webhooks().list_deliveries(&wh.id, Some(0), Some(20), None).await?;  // envelope
println!("{} deliveries", deliveries.total);
# Ok(())
# }
```

## Error handling

Every method returns `Result<T, AxeneError>`. `AxeneError` carries `status: u16`,
`code: Option<String>`, and `message: String`, and implements `std::error::Error`.
A `status` of `0` indicates a transport failure with no HTTP response.

```rust
# async fn demo(client: &axene_mailer::Axene) {
match client.emails().get("missing").await {
    Ok(email) => println!("{}", email.email.id),
    Err(e) if e.status == 404 => println!("not found"),
    Err(e) => eprintln!("error {}: {}", e.status, e.message),
}
# }
```

## Pagination

`page` is zero-based everywhere (`page = 0` is the first page). Bare-array
endpoints return a plain `Vec<T>`; envelope endpoints (suppressions list and
webhook deliveries) return `Page<T>` with `items`, `total`, `page`, and `limit`.

## License

MIT
