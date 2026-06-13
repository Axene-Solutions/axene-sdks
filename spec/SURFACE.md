# Axene Mailer SDK — Core Surface Contract

Single source of truth for the Core SDK surface, extracted directly from the
backend routers + Pydantic schemas (not the OpenAPI guess). Base URL
`https://mail.axene.io`. Auth: `Authorization: Bearer axm_k_...`. All datetimes
ISO 8601; all UUIDs are strings on the wire. Pagination is **zero-based**
(`page=0` is the first page).

Two list conventions coexist, faithfully preserved:
- **bare array**: emails (list/search/scheduled/updates/events), domains list,
  contact lists, templates, webhooks list.
- **envelope** `{items,total,page,limit}`: suppressions list, webhook deliveries.

Shared objects:
- **Address**: `{ email: string (required), name?: string|null }`. A bare string
  is sugar for `{email}`.
- **Attachment**: `{ filename: string (1-255, no / \ NUL), content_base64: string
  (raw base64, no data: prefix), content_type?: string (default
  "application/octet-stream") }`.

WIRE QUIRKS (must be honoured exactly):
- The sender field serializes as the literal key **`from_`** (trailing
  underscore), NOT `from`. SDKs expose a clean `from` and map it to `from_`.
- Contact + email-event `metadata` is the wire name (DB column is `metadata_`).
- Webhook deliveries filter param is `status` (aliased).

---

## emails  (prefix /v1/emails)

### POST /v1/emails/  -> 202 SendEmailResponse
Body SendEmailRequest: `from_` Address (req), `to` Address[] (req), `cc`
Address[]|null, `bcc` Address[]|null, `subject` string (req), `text` string|null,
`html` string|null, `headers` map<string,string>|null, `tags` string[]|null,
`reply_to` Address|null, `send_at` datetime|null, `attachments` Attachment[]|null.
Response: `{ id: string, status: string, message_id: string|null, rejection_reason: string|null }`.
Quirks: `send_at` must be >now, >=now+1min, <=now+30d (Starter+). Per-file and
combined attachment cap 24 MiB -> 413. Bad base64 -> 400. Unverified sender -> 422.

### POST /v1/emails/batch  -> 202 BatchResponse
Body: **bare array** of SendEmailRequest. Response: `{ total, sent, failed,
results: [{ id: string|null, status: string, rejection_reason: string|null }] }`.
Empty array -> 400. Over plan max_batch_size (25/50/100) -> 400. Per-item errors
captured in results with status "error".

### POST /v1/emails/validate  -> 200 ValidationResult
Body: SendEmailRequest (full). Response: `{ valid: bool, can_send: bool, issues:
[{ field: string, error: string }], plan: string, usage: { daily, daily_limit,
monthly, monthly_limit } }`. Dry-run, never sends.

### GET /v1/emails/  -> 200 EmailResponse[] (bare array)
Query: `status` string?, `page` int=0, `limit` int=20 (1-100).
EmailResponse: `{ id, from_address, to_addresses: string[], subject: string|null,
status, source, opened_count: int, clicked_count: int, tags: string[]|null,
scheduled_at: datetime|null, created_at, sent_at: datetime|null, delivered_at:
datetime|null, retry_of_id: string|null }`.

### GET /v1/emails/{id}  -> 200 EmailDetailResponse
EmailResponse + `{ cc_addresses: string[]|null, bcc_addresses: string[]|null,
text_body: string|null, html_body: string|null, headers: object|null, message_id:
string|null, events: EmailEvent[] }`. 404 if not in active org.
EmailEvent: `{ id, event_type: string, metadata: object|null, created_at }`.

### GET /v1/emails/{id}/events  -> 200 EmailEvent[] (bare array)

### POST /v1/emails/{id}/retry  -> 202 SendEmailResponse
No body. Original status must be bounced/rejected/failed else 409. New row links
via retry_of_id.

### GET /v1/emails/search  -> 200 array (bare)
Query: `q` string="", `status` string?, `tag` string?, `page` int=0, `limit`
int=20. Items: `{ id, from_address, to_addresses, subject, status, tags, source,
created_at, delivered_at }`. `q` supports tokens to:/from:/status:/domain:/tag:.

### GET /v1/emails/scheduled  -> 200 array (bare)
Items: EmailResponse-ish + `seconds_until_send: int`. status==scheduled, asc.

### DELETE /v1/emails/scheduled/{id}  -> 200 `{ id, status: "cancelled" }`
Only status scheduled, else 404.

### POST /v1/emails/scheduled/{id}/send-now  -> 200 `{ id, status: "queued" }`
No body. Only status scheduled, else 404.

### GET /v1/emails/updates  -> 200 EmailResponse[] (bare, max 50)
Query: `since` string **required** (ISO 8601). Rows changed since `since`.

### GET /v1/emails/saved-searches  -> 200 `{ searches: object[] }`
### PUT /v1/emails/saved-searches  -> 200 `{ searches: object[] }`
Body `{ searches: object[] }`. Must be a list else 400, cap 50. Each item
normalized to `{ id, name(<=60), query(<=200), range(<=10,="24h"), status(<=24,
="all"), domain(<=120,="all"), source(<=24,="all") }`.

---

## domains  (prefix /v1/domains)

### GET /v1/domains/  -> 200 DomainListItem[] (bare array)
DomainListItem: `{ id, name, status, created_at, platform_warning: string|null }`.

### POST /v1/domains/  -> 201 Domain  [scope domains:write]
Body: `{ name: string (req) }`. Name normalized: trimmed/lowercased/scheme+path
stripped; hostname regex. 409 if owned by another account; 422 platform subdomain.
Domain: `{ id, name, status, dkim_selector, verified_at: datetime|null,
created_at, dns_records: DnsRecord[], platform_warning: string|null }`.
DnsRecord: `{ id, record_type, purpose, host, value, is_verified: bool,
last_checked_at: datetime|null }`.

### GET /v1/domains/{id}  -> 200 Domain
### DELETE /v1/domains/{id}  -> 204  [scope domains:write]
### POST /v1/domains/{id}/verify  -> 200 Domain   (rate-limited 20/60s)
### GET /v1/domains/{id}/health  -> 200 `{ domain, checks: [{ key, label, status:
"ok"|"warn"|"error"|"info", detail, recommendation: string|null, record:
{type,host,value}|null }], summary: { ok, warn, error, info } }`
### GET /v1/domains/{id}/diagnose  -> 200 `{ domain, issues: object[], health_score: int }`
### GET /v1/domains/{id}/mx-status  -> 200 `{ domain, ...mx_status (open map) }`
### GET /v1/domains/{id}/published-records  -> 200 `{ records: map<recordId, string[]> }`
### POST /v1/domains/{id}/rotate-dkim  -> 200 `{ dkim_record_host, dkim_record_value, domain: Domain }`
### POST /v1/domains/{id}/transfer  -> 200 DomainTransfer  [scope domains:write, rate 5/60s]
Body: `{ target_email: string (req), note: string|null (<=1000) }`.
DomainTransfer: `{ id, domain_id, domain_name, source_user_id, source_org_id,
source_label, target_email, target_user_id, target_org_id, status, note,
cooloff_until: datetime|null, initiated_at, accepted_at: datetime|null,
completed_at: datetime|null, expires_at }`.

### GET /v1/domains/check-availability  -> 200 `{ available: bool, reason: string|null,
detail: string|null, stale_tokens: int|null }`  Query: `name` string (req). 400 if
empty/no-dot/>253.
### GET /v1/domains/check/{domain_name}  -> 200 `{ exists: bool, verified: bool,
status?: string, domain: string, id?: string }`  (never 404).

NICHE (model loosely, open maps; implement but document as advanced):
ns-provider, bimi/enable, bimi/logo (multipart), bimi/status, domain-connect,
domain-connect/support.

---

## contacts  (prefix /v1/contacts)

### GET /v1/contacts/  -> 200 ContactList[] (bare array)
ContactList: `{ id, name, description: string|null, icon_seed: string|null,
contact_count: int, created_at }`.
### POST /v1/contacts/  -> 201 ContactList
Body: `{ name: string (req), description: string|null, icon_seed: string|null }`.
### GET /v1/contacts/{list_id}  -> 200 ContactListDetail
Query: `page` int=0, `limit` int=50 (1-200). = ContactList + `{ contacts:
Contact[] }`. Contact: `{ id, email, name: string|null, metadata: object|null,
created_at }`.
### PATCH /v1/contacts/{list_id}  -> 200 ContactList
Body (partial, exclude_unset): `{ name?, description?, icon_seed? }`.
### DELETE /v1/contacts/{list_id}  -> 204
### POST /v1/contacts/{list_id}/contacts  -> 201 Contact
Body: `{ email: string (req), name: string|null, metadata: object|null }`. 409 dup.
### DELETE /v1/contacts/{list_id}/contacts/{contact_id}  -> 204
### POST /v1/contacts/{list_id}/upload  -> 200 `{ imported: int, skipped: int,
errors: string[] }`  **multipart/form-data**, field `file`. Header row required.
### POST /v1/contacts/{list_id}/send  -> 200 `{ queued: int, skipped: int, errors: string[] }`
Body BulkSend: `{ contact_list_id: string (req, ==list_id), sender_address_id:
string (req), subject: string (req), html: string|null, text: string|null, tags:
string[]|null }`. Templating {{email}}/{{name}}/{{metadata_key}}.

---

## suppressions  (prefix /v1/suppressions)

### GET /v1/suppressions  -> 200 envelope `{ items: [{ id, email_address, reason,
created_at: string|null }], total, page, limit }`  Query: `page` int=0, `limit`
int=50 (1-200), `search` string?.
### POST /v1/suppressions  -> 201 `{ id, email_address, reason }`
Body: `{ email_address: string (req), reason: string="manual" }`. 409 if dup.
### POST /v1/suppressions/bulk  -> 201 `{ added, skipped, total_processed }`
**multipart/form-data**, field `file`, one email per line. Feature-gated
has_csv_import (Starter+).
### DELETE /v1/suppressions/{id}  -> 204

---

## templates  (prefix /v1/templates)  — feature-gated has_templates (Starter+)

Template: `{ id, name, subject: string|null, html_body: string|null, text_body:
string|null, variables: string[]|null (server-derived, read-only), blocks_json:
object|null, created_at, updated_at }`.

### GET /v1/templates/  -> 200 Template[] (bare array)
### POST /v1/templates/  -> 201 Template
Body: `{ name: string (req), subject?, html_body?, text_body?, blocks_json? }`.
`variables` NOT accepted (derived from {{word}}). 403 over max_templates.
### GET /v1/templates/{id}  -> 200 Template
### PATCH /v1/templates/{id}  -> 200 Template  (partial, exclude_unset)
### DELETE /v1/templates/{id}  -> 204
### POST /v1/templates/{id}/duplicate  -> 200 Template  (no body; blocks_json NOT copied)

---

## webhooks  (prefix /v1/webhooks)

Webhook: `{ id, url, events: string[], secret (plaintext on every read),
is_active: bool, created_at }`. No updated_at. `events` is free-form string[]
(no server enum); canonical example emitted: `email.delivered`.

### GET /v1/webhooks/  -> 200 Webhook[] (bare array, active only)
### POST /v1/webhooks/  -> 201 Webhook  [scope webhooks:write]
Body: `{ url: string (req), events: string[] (req) }`. SSRF guard -> 422 on
private/unsafe url. secret server-generated.
### PATCH /v1/webhooks/{id}  -> 200 Webhook  [scope webhooks:write]
Body (partial, applied when !=null): `{ url?, events?, is_active? }`. Inactive ->404.
### DELETE /v1/webhooks/{id}  -> 204  [scope webhooks:write]
### POST /v1/webhooks/{id}/test  -> 200 `{ queued: true, url }`  [scope webhooks:write, rate 10/60s]
### GET /v1/webhooks/{id}/deliveries  -> 200 envelope `{ items: Delivery[], total,
page, limit }`  Query: `page` int=0, `limit` int=20 (1-100), `status` string?.
Delivery: `{ id, webhook_id, event_type: string|null, status, response_status:
int|null, attempt: int, next_retry_at: string|null, created_at: string|null }`.
NOTE: total ignores the status filter.
### GET /v1/webhooks/{id}/deliveries/{delivery_id}  -> 200 Delivery + `{ payload:
object, response_body: string|null, endpoint_url: string }`.

---

## Error envelope
Non-2xx: `{ "detail": { "code": string, "message": string } }` or
`{ "detail": string }`. SDKs map to AxeneError{ status, code?, message }.
Retry on 429 and 5xx with backoff (honour Retry-After). Never retry 4xx.
