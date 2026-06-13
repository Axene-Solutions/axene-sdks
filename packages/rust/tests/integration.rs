//! Integration tests against a local mock HTTP server (`wiremock`).
//!
//! Covers: bearer header, `from_` mapping, batch bare-array, validate full
//! body, multipart upload field name, suppressions envelope parse, webhooks
//! `is_active` mapping, and 429-retry-then-success.

use axene_mailer::{
    AddSuppression, Axene, BulkSend, CreateTemplate, SendEmail, UpdateWebhook,
};
use serde_json::{json, Value};
use wiremock::matchers::{body_json, header, method, path, query_param};
use wiremock::{Mock, MockServer, Request, ResponseTemplate};

async fn client(server: &MockServer) -> Axene {
    Axene::builder("axm_k_test123")
        .base_url(server.uri())
        .max_retries(3)
        .build()
        .expect("client builds")
}

#[tokio::test]
async fn send_sets_bearer_and_maps_from() {
    let server = MockServer::start().await;

    Mock::given(method("POST"))
        .and(path("/v1/emails/"))
        .and(header("authorization", "Bearer axm_k_test123"))
        // The sender field must serialize as `from_`, not `from`.
        .and(body_json(json!({
            "from_": { "email": "hello@ex.com", "name": "Hi" },
            "to": [{ "email": "to@ex.com" }],
            "subject": "Hey",
            "html": "<p>hi</p>"
        })))
        .respond_with(ResponseTemplate::new(202).set_body_json(json!({
            "id": "em_1", "status": "queued", "message_id": null, "rejection_reason": null
        })))
        .mount(&server)
        .await;

    let c = client(&server).await;
    let msg = SendEmail::builder(("hello@ex.com", "Hi"), "to@ex.com", "Hey")
        .html("<p>hi</p>")
        .build();
    let res = c.emails().send(&msg).await.expect("send ok");
    assert_eq!(res.id, "em_1");
    assert_eq!(res.status, "queued");
}

#[tokio::test]
async fn send_batch_posts_bare_array() {
    let server = MockServer::start().await;

    Mock::given(method("POST"))
        .and(path("/v1/emails/batch"))
        .respond_with(|req: &Request| {
            let body: Value = req.body_json().unwrap();
            // The batch body must be a bare JSON array.
            assert!(body.is_array(), "batch body should be a bare array");
            assert_eq!(body.as_array().unwrap().len(), 2);
            assert_eq!(body[0]["from_"]["email"], "a@ex.com");
            ResponseTemplate::new(202).set_body_json(json!({
                "total": 2, "sent": 2, "failed": 0,
                "results": [
                    { "id": "1", "status": "queued", "rejection_reason": null },
                    { "id": "2", "status": "queued", "rejection_reason": null }
                ]
            }))
        })
        .mount(&server)
        .await;

    let c = client(&server).await;
    let msgs = vec![
        SendEmail::builder("a@ex.com", "x@ex.com", "One").text("1").build(),
        SendEmail::builder("b@ex.com", "y@ex.com", "Two").text("2").build(),
    ];
    let res = c.emails().send_batch(&msgs).await.expect("batch ok");
    assert_eq!(res.total, 2);
    assert_eq!(res.sent, 2);
    assert_eq!(res.results.len(), 2);
}

#[tokio::test]
async fn validate_sends_full_body() {
    let server = MockServer::start().await;

    Mock::given(method("POST"))
        .and(path("/v1/emails/validate"))
        .and(body_json(json!({
            "from_": { "email": "f@ex.com" },
            "to": [{ "email": "t@ex.com" }],
            "subject": "Subj",
            "html": "<p>h</p>",
            "text": "h",
            "tags": ["a", "b"]
        })))
        .respond_with(ResponseTemplate::new(200).set_body_json(json!({
            "valid": true, "can_send": true, "issues": [], "plan": "starter",
            "usage": { "daily": 1, "daily_limit": 100, "monthly": 10, "monthly_limit": 1000 }
        })))
        .mount(&server)
        .await;

    let c = client(&server).await;
    let msg = SendEmail::builder("f@ex.com", "t@ex.com", "Subj")
        .html("<p>h</p>")
        .text("h")
        .tags(vec!["a".into(), "b".into()])
        .build();
    let res = c.emails().validate(&msg).await.expect("validate ok");
    assert!(res.valid);
    assert!(res.can_send);
    assert_eq!(res.usage.daily_limit, 100);
}

#[tokio::test]
async fn upload_csv_uses_multipart_file_field() {
    let server = MockServer::start().await;

    Mock::given(method("POST"))
        .and(path("/v1/contacts/list_1/upload"))
        .respond_with(|req: &Request| {
            let ct = req
                .headers
                .get("content-type")
                .map(|v| v.to_str().unwrap().to_string())
                .unwrap_or_default();
            assert!(ct.starts_with("multipart/form-data"), "should be multipart");
            let body = String::from_utf8_lossy(&req.body);
            // The single multipart field must be named `file`.
            assert!(body.contains("name=\"file\""), "field must be named file");
            assert!(body.contains("email,name"), "csv content present");
            ResponseTemplate::new(200).set_body_json(json!({
                "imported": 3, "skipped": 1, "errors": []
            }))
        })
        .mount(&server)
        .await;

    let c = client(&server).await;
    let csv = b"email,name\na@ex.com,A\n".to_vec();
    let res = c
        .contacts()
        .upload_csv("list_1", csv, "contacts.csv")
        .await
        .expect("upload ok");
    assert_eq!(res.imported, 3);
    assert_eq!(res.skipped, 1);
}

#[tokio::test]
async fn suppressions_list_parses_envelope() {
    let server = MockServer::start().await;

    Mock::given(method("GET"))
        .and(path("/v1/suppressions"))
        .and(query_param("page", "0"))
        .and(query_param("limit", "50"))
        .respond_with(ResponseTemplate::new(200).set_body_json(json!({
            "items": [
                { "id": "s1", "email_address": "x@ex.com", "reason": "bounce", "created_at": null }
            ],
            "total": 1, "page": 0, "limit": 50
        })))
        .mount(&server)
        .await;

    let c = client(&server).await;
    let page = c
        .suppressions()
        .list(Some(0), Some(50), None)
        .await
        .expect("list ok");
    assert_eq!(page.total, 1);
    assert_eq!(page.items.len(), 1);
    assert_eq!(page.items[0].email_address, "x@ex.com");
}

#[tokio::test]
async fn suppression_add_maps_email_address() {
    let server = MockServer::start().await;

    Mock::given(method("POST"))
        .and(path("/v1/suppressions"))
        .and(body_json(json!({ "email_address": "z@ex.com", "reason": "manual" })))
        .respond_with(ResponseTemplate::new(201).set_body_json(json!({
            "id": "s2", "email_address": "z@ex.com", "reason": "manual"
        })))
        .mount(&server)
        .await;

    let c = client(&server).await;
    let res = c
        .suppressions()
        .add(&AddSuppression::new("z@ex.com"))
        .await
        .expect("add ok");
    assert_eq!(res.email_address, "z@ex.com");
}

#[tokio::test]
async fn webhook_update_maps_is_active() {
    let server = MockServer::start().await;

    Mock::given(method("PATCH"))
        .and(path("/v1/webhooks/wh_1"))
        .and(body_json(json!({ "is_active": false })))
        .respond_with(ResponseTemplate::new(200).set_body_json(json!({
            "id": "wh_1", "url": "https://ex.com/hook", "events": ["email.delivered"],
            "secret": "whsec_x", "is_active": false, "created_at": "2026-06-13T00:00:00Z"
        })))
        .mount(&server)
        .await;

    let c = client(&server).await;
    let res = c
        .webhooks()
        .update("wh_1", &UpdateWebhook::default().is_active(false))
        .await
        .expect("update ok");
    assert!(!res.is_active);
}

#[tokio::test]
async fn template_create_maps_html_and_text_bodies() {
    let server = MockServer::start().await;

    Mock::given(method("POST"))
        .and(path("/v1/templates/"))
        .and(body_json(json!({
            "name": "Welcome",
            "html_body": "<p>hi</p>",
            "text_body": "hi"
        })))
        .respond_with(ResponseTemplate::new(201).set_body_json(json!({
            "id": "tpl_1", "name": "Welcome", "subject": null,
            "html_body": "<p>hi</p>", "text_body": "hi", "variables": [],
            "blocks_json": null, "created_at": "2026-06-13T00:00:00Z",
            "updated_at": "2026-06-13T00:00:00Z"
        })))
        .mount(&server)
        .await;

    let c = client(&server).await;
    let res = c
        .templates()
        .create(&CreateTemplate::new("Welcome").html("<p>hi</p>").text("hi"))
        .await
        .expect("create ok");
    assert_eq!(res.id, "tpl_1");
}

#[tokio::test]
async fn bulk_send_injects_contact_list_id() {
    let server = MockServer::start().await;

    Mock::given(method("POST"))
        .and(path("/v1/contacts/list_9/send"))
        .and(body_json(json!({
            "contact_list_id": "list_9",
            "sender_address_id": "snd_1",
            "subject": "Hello {{name}}"
        })))
        .respond_with(ResponseTemplate::new(200).set_body_json(json!({
            "queued": 5, "skipped": 0, "errors": []
        })))
        .mount(&server)
        .await;

    let c = client(&server).await;
    let res = c
        .contacts()
        .bulk_send("list_9", &BulkSend::new("snd_1", "Hello {{name}}"))
        .await
        .expect("bulk send ok");
    assert_eq!(res.queued, 5);
}

#[tokio::test]
async fn retries_on_429_then_succeeds() {
    let server = MockServer::start().await;

    // First attempt: 429 with a tiny Retry-After. Then success.
    Mock::given(method("GET"))
        .and(path("/v1/domains/"))
        .respond_with(ResponseTemplate::new(429).insert_header("retry-after", "0"))
        .up_to_n_times(1)
        .mount(&server)
        .await;

    Mock::given(method("GET"))
        .and(path("/v1/domains/"))
        .respond_with(ResponseTemplate::new(200).set_body_json(json!([
            { "id": "d1", "name": "ex.com", "status": "verified",
              "created_at": "2026-06-13T00:00:00Z", "platform_warning": null }
        ])))
        .mount(&server)
        .await;

    let c = client(&server).await;
    let domains = c.domains().list().await.expect("list ok after retry");
    assert_eq!(domains.len(), 1);
    assert_eq!(domains[0].name, "ex.com");
}

#[tokio::test]
async fn maps_error_envelope() {
    let server = MockServer::start().await;

    Mock::given(method("GET"))
        .and(path("/v1/emails/missing"))
        .respond_with(ResponseTemplate::new(404).set_body_json(json!({
            "detail": { "code": "not_found", "message": "Email not found" }
        })))
        .mount(&server)
        .await;

    let c = client(&server).await;
    let err = c.emails().get("missing").await.expect_err("should error");
    assert_eq!(err.status, 404);
    assert_eq!(err.code.as_deref(), Some("not_found"));
    assert_eq!(err.message, "Email not found");
}
