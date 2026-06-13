# Axene Mailer SDK - Design Contract

Every SDK (typescript, dotnet, java, python, rust, ruby, php, go, swift) exposes
the SAME resources and methods over the SURFACE.md contract. Names below are the
canonical (camelCase) form; each language uses its idiom (snake_case for
ruby/rust/python, PascalCase methods for dotnet/go-exported, etc.).

## Client
- Construct with an API key (required; starts with `axm_k_`). Optional: baseUrl
  (default `https://mail.axene.io`), maxRetries (default 3), timeout (default 30s).
- One transport layer owns: bearer auth, JSON encode/decode, the `from`->`from_`
  mapping, retries on 429/5xx with backoff (honour Retry-After), error mapping to
  AxeneError. Resources are thin and call the transport. Keep transport, models,
  and resources in separate files/modules (separation of concerns).
- Error type `AxeneError`: fields `status:int`, `code:string?`, `message:string`.

## Resources & methods

### emails
- `send(message) -> SendResult`
- `sendBatch(messages[]) -> BatchResult`            (POST bare array)
- `validate(message) -> ValidationResult`           (full send body)
- `list({status?, page=0, limit=20}) -> Email[]`
- `get(id) -> EmailDetail`
- `events(id) -> EmailEvent[]`
- `retry(id) -> SendResult`
- `search({q?, status?, tag?, page=0, limit=20}) -> EmailSearchHit[]`
- `listScheduled() -> ScheduledEmail[]`
- `cancelScheduled(id) -> {id,status}`
- `sendScheduledNow(id) -> {id,status}`
- `updates(since) -> Email[]`                        (since required)
- `getSavedSearches() -> object[]`
- `setSavedSearches(searches[]) -> object[]`

### domains
- `list() -> DomainListItem[]`
- `create(name) -> Domain`
- `get(id) -> Domain`
- `delete(id) -> void`
- `verify(id) -> Domain`
- `health(id) -> DomainHealth`
- `diagnose(id) -> DomainDiagnosis`
- `mxStatus(id) -> object`            (open map)
- `publishedRecords(id) -> object`    (open map)
- `rotateDkim(id) -> DkimRotation`
- `transfer(id, {targetEmail, note?}) -> DomainTransfer`
- `checkAvailability(name) -> DomainAvailability`
- `check(name) -> DomainCheck`
(NICHE deferred unless trivial: nsProvider, bimi*, domainConnect* - OK to omit in
v1; document as not-yet-covered. Do NOT fake them.)

### contacts
- `listLists() -> ContactList[]`
- `createList({name, description?, iconSeed?}) -> ContactList`
- `getList(id, {page=0, limit=50}) -> ContactListDetail`
- `updateList(id, {name?, description?, iconSeed?}) -> ContactList`
- `deleteList(id) -> void`
- `addContact(listId, {email, name?, metadata?}) -> Contact`
- `removeContact(listId, contactId) -> void`
- `uploadCsv(listId, fileBytes, filename) -> CsvImportResult`   (multipart field `file`)
- `bulkSend(listId, {senderAddressId, subject, html?, text?, tags?}) -> BulkSendResult`
   (sets contact_list_id = listId automatically)

### suppressions
- `list({page=0, limit=50, search?}) -> Page<Suppression>`   (envelope items/total/page/limit)
- `add({email, reason="manual"}) -> Suppression`             (wire field email_address)
- `bulkUpload(fileBytes, filename) -> BulkSuppressionResult`  (multipart field `file`)
- `remove(id) -> void`

### templates
- `list() -> Template[]`
- `create({name, subject?, html?, text?, blocksJson?}) -> Template`   (html->html_body, text->text_body)
- `get(id) -> Template`
- `update(id, {name?, subject?, html?, text?, blocksJson?}) -> Template`
- `delete(id) -> void`
- `duplicate(id) -> Template`

### webhooks
- `list() -> Webhook[]`
- `create({url, events[]}) -> Webhook`
- `update(id, {url?, events?, isActive?}) -> Webhook`
- `delete(id) -> void`
- `test(id) -> {queued, url}`
- `listDeliveries(id, {page=0, limit=20, status?}) -> Page<Delivery>`   (envelope)
- `getDelivery(id, deliveryId) -> DeliveryDetail`

## Conventions
- Recipient sugar: a bare string is accepted anywhere an Address is expected.
- `page` is zero-based everywhere.
- Bare-array endpoints return a plain list; envelope endpoints return a Page
  type `{ items, total, page, limit }`.
- Multipart uploads send exactly one field named `file`.
- Wire mappings to honour: `from`->`from_`, `html`->`html_body` and
  `text`->`text_body` for templates ONLY (emails keep `html`/`text`),
  `iconSeed`->`icon_seed`, `blocksJson`->`blocks_json`, `senderAddressId`->
  `sender_address_id`, suppression `email`->`email_address`, `isActive`->`is_active`.
- Tests: each SDK ships a mock-transport / local-HTTP-server test that asserts at
  least: bearer header, `from_` mapping, batch bare-array, validate full body,
  one envelope (suppressions or deliveries) parse, and 429-retry-then-success.

## CI / release
- CI job per package, gated by `[test]` in commit message or PR title.
- Release tag prefixes: `ts-v*`, `py-v*`, `dotnet-v*`, `java-v*`, `rust-v*`,
  `ruby-v*`, `php-v*`, `swift-v*`. Go lives in its own repo (git tag only).
- Tests always run before publish. No releases until explicitly requested.
