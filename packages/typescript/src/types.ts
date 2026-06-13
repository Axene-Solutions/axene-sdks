/**
 * Public request/response types for the Axene Mailer API.
 * @module
 */

/** Options for constructing an {@link Axene} client. */
export interface AxeneOptions {
  /** API key from your Axene Mailer dashboard (starts with `axm_k_`). */
  apiKey: string;
  /** Override the API base URL. Defaults to `https://mail.axene.io`. */
  baseUrl?: string;
  /** Total attempts on `429` / `5xx`, including the first. Defaults to `3`. */
  maxRetries?: number;
  /** Per-request timeout in milliseconds. Defaults to `30000`. */
  timeoutMs?: number;
  /** Inject a custom `fetch` implementation (for Node &lt; 18 or testing). */
  fetch?: typeof fetch;
}

/** A recipient or sender. A bare string is treated as `{ email }`. */
export type Address = string | { email: string; name?: string };

/** A file attachment. `content_base64` is the base64-encoded file content. */
export interface Attachment {
  filename: string;
  content_base64: string;
  content_type?: string;
}

/** Parameters for {@link Emails.send} and {@link Emails.sendBatch}. */
export interface SendEmailParams {
  /** Sender address. Must be on a verified domain in your account. */
  from: Address;
  /** One or more recipients. */
  to: Address | Address[];
  subject: string;
  /** HTML body. Provide `html`, `text`, or both. */
  html?: string;
  /** Plain-text body. Provide `html`, `text`, or both. */
  text?: string;
  cc?: Address | Address[];
  bcc?: Address | Address[];
  replyTo?: Address;
  /** Custom headers to attach to the message. */
  headers?: Record<string, string>;
  /** Tags for filtering and analytics. */
  tags?: string[];
  /** Schedule delivery for later (ISO 8601 string or `Date`). Starter plan and up. */
  sendAt?: string | Date;
  attachments?: Attachment[];
}

/** Result of a send: the queued message id and its initial status. */
export interface SendEmailResponse {
  id: string;
  status: string;
  message_id?: string | null;
  rejection_reason?: string | null;
}

/** Result of {@link Emails.sendBatch}. */
export interface BatchResponse {
  /** Number of messages submitted. */
  total: number;
  /** Number accepted for delivery. */
  sent: number;
  /** Number rejected. */
  failed: number;
  /** One result per submitted message, in order. */
  results: SendEmailResponse[];
}

/** A delivery / open / click / bounce event for a message. */
export interface EmailEvent {
  id: string;
  type: string;
  created_at: string;
  [k: string]: unknown;
}

/** A stored email and its current status. */
export interface Email {
  id: string;
  from_address: string;
  to_addresses: string[];
  subject: string | null;
  status: string;
  created_at: string | null;
  delivered_at?: string | null;
  [k: string]: unknown;
}

/** A stored email with its bodies and events, from {@link Emails.get}. */
export interface EmailDetail extends Email {
  cc_addresses?: string[] | null;
  bcc_addresses?: string[] | null;
  text_body?: string | null;
  html_body?: string | null;
  headers?: Record<string, unknown> | null;
  message_id?: string | null;
  events: EmailEvent[];
}

/** A single reason a message would not send. */
export interface ValidationIssue {
  field: string;
  error: string;
}

/** Sending-quota usage returned alongside a validation. */
export interface ValidationUsage {
  daily: number;
  daily_limit: number;
  monthly: number;
  monthly_limit: number;
}

/**
 * Result of {@link Emails.validate}: a dry-run that checks whether a message
 * would send (sender registered, domain verified, plan limits, restrictions)
 * without actually sending it.
 */
export interface ValidationResult {
  valid: boolean;
  can_send: boolean;
  issues: ValidationIssue[];
  plan: string;
  usage: ValidationUsage;
}

/** A row from {@link Domains.list}: a sending domain and its status. */
export interface DomainListItem {
  id: string;
  name: string;
  status: string;
  created_at: string | null;
  platform_warning?: string | null;
}

/** A DNS record the API expects you to publish for a domain. */
export interface DnsRecord {
  id: string;
  record_type: string;
  purpose: string;
  host: string;
  value: string;
  is_verified: boolean;
  last_checked_at?: string | null;
}

/** A sending domain with its DKIM selector and DNS records. */
export interface Domain {
  id: string;
  name: string;
  status: string;
  dkim_selector: string;
  verified_at?: string | null;
  created_at: string | null;
  dns_records: DnsRecord[];
  platform_warning?: string | null;
}

/** One row of a domain health report. */
export interface DomainHealthCheck {
  key: string;
  label: string;
  status: 'ok' | 'warn' | 'error' | 'info';
  detail: string;
  recommendation?: string | null;
  record?: { type: string; host: string; value: string } | null;
}

/** Result of {@link Domains.health}: per-record checks plus a summary tally. */
export interface DomainHealth {
  domain: string;
  checks: DomainHealthCheck[];
  summary: { ok: number; warn: number; error: number; info: number };
}

/** Result of {@link Domains.diagnose}. `issues` shapes vary; treated as opaque. */
export interface DomainDiagnosis {
  domain: string;
  issues: unknown[];
  health_score: number;
}

/** Result of {@link Domains.rotateDkim}: the new DKIM record plus the domain. */
export interface DkimRotation {
  dkim_record_host: string;
  dkim_record_value: string;
  domain: Domain;
}

/** A domain transfer record returned by {@link Domains.transfer}. */
export interface DomainTransfer {
  id: string;
  domain_id: string;
  domain_name?: string | null;
  source_label?: string | null;
  target_email: string;
  status: string;
  note?: string | null;
  cooloff_until?: string | null;
  initiated_at: string;
  accepted_at?: string | null;
  completed_at?: string | null;
  expires_at: string;
  [k: string]: unknown;
}

/** Result of {@link Domains.checkAvailability}. */
export interface DomainAvailability {
  available: boolean;
  reason: string | null;
  detail: string | null;
  stale_tokens: number | null;
}

/** Result of {@link Domains.check}: whether a domain name exists in your account. */
export interface DomainCheck {
  exists: boolean;
  verified: boolean;
  status?: string;
  domain: string;
  id?: string;
}

/** A paginated envelope `{ items, total, page, limit }`. */
export interface Page<T> {
  items: T[];
  total: number;
  page: number;
  limit: number;
}

// -- contacts ---------------------------------------------------------------

/** A subscriber list. */
export interface ContactList {
  id: string;
  name: string;
  description: string | null;
  icon_seed: string | null;
  contact_count: number;
  created_at: string;
}

/** A single contact in a list. */
export interface Contact {
  id: string;
  email: string;
  name: string | null;
  metadata: Record<string, unknown> | null;
  created_at: string;
}

/** A contact list with a page of its contacts. */
export interface ContactListDetail extends ContactList {
  contacts: Contact[];
}

/** Parameters for {@link Contacts.createList}. */
export interface CreateListParams {
  name: string;
  description?: string | null;
  iconSeed?: string | null;
}

/** Parameters for {@link Contacts.updateList} (partial). */
export interface UpdateListParams {
  name?: string | null;
  description?: string | null;
  iconSeed?: string | null;
}

/** Parameters for {@link Contacts.addContact}. */
export interface AddContactParams {
  email: string;
  name?: string | null;
  metadata?: Record<string, unknown> | null;
}

/** Result of {@link Contacts.uploadCsv}. */
export interface CsvImportResult {
  imported: number;
  skipped: number;
  errors: string[];
}

/** Parameters for {@link Contacts.bulkSend}. */
export interface BulkSendParams {
  senderAddressId: string;
  subject: string;
  html?: string | null;
  text?: string | null;
  tags?: string[] | null;
}

/** Result of {@link Contacts.bulkSend}. */
export interface BulkSendResult {
  queued: number;
  skipped: number;
  errors: string[];
}

// -- suppressions -----------------------------------------------------------

/** A suppressed recipient address. */
export interface Suppression {
  id: string;
  email_address: string;
  reason: string;
  created_at?: string | null;
}

/** Parameters for {@link Suppressions.add}. */
export interface AddSuppressionParams {
  email: string;
  reason?: string;
}

/** Result of {@link Suppressions.bulkUpload}. */
export interface BulkSuppressionResult {
  added: number;
  skipped: number;
  total_processed: number;
}

// -- templates --------------------------------------------------------------

/** A reusable email template. `variables` is derived server-side and read-only. */
export interface Template {
  id: string;
  name: string;
  subject: string | null;
  html_body: string | null;
  text_body: string | null;
  variables: string[] | null;
  blocks_json: Record<string, unknown> | null;
  created_at: string;
  updated_at: string;
}

/** Parameters for {@link Templates.create}. */
export interface CreateTemplateParams {
  name: string;
  subject?: string | null;
  html?: string | null;
  text?: string | null;
  blocksJson?: Record<string, unknown> | null;
}

/** Parameters for {@link Templates.update} (partial). */
export interface UpdateTemplateParams {
  name?: string | null;
  subject?: string | null;
  html?: string | null;
  text?: string | null;
  blocksJson?: Record<string, unknown> | null;
}

// -- webhooks ---------------------------------------------------------------

/** A configured webhook endpoint. `secret` is returned in plaintext. */
export interface Webhook {
  id: string;
  url: string;
  events: string[];
  secret: string;
  is_active: boolean;
  created_at: string;
}

/** Parameters for {@link Webhooks.create}. */
export interface CreateWebhookParams {
  url: string;
  events: string[];
}

/** Parameters for {@link Webhooks.update} (partial). */
export interface UpdateWebhookParams {
  url?: string;
  events?: string[];
  isActive?: boolean;
}

/** A summary of one webhook delivery attempt. */
export interface WebhookDelivery {
  id: string;
  webhook_id: string;
  event_type: string | null;
  status: string;
  response_status: number | null;
  attempt: number;
  next_retry_at: string | null;
  created_at: string | null;
}

/** A webhook delivery with the full payload and endpoint response. */
export interface WebhookDeliveryDetail extends WebhookDelivery {
  payload: Record<string, unknown>;
  response_body: string | null;
  endpoint_url: string;
}

// -- emails (extended) ------------------------------------------------------

/** A scheduled email awaiting send. */
export interface ScheduledEmail {
  id: string;
  from_address: string;
  to_addresses: string[];
  subject: string | null;
  status: string;
  tags: string[] | null;
  scheduled_at: string | null;
  seconds_until_send: number;
  created_at: string | null;
}

/** A search hit from {@link Emails.search}. */
export interface EmailSearchHit {
  id: string;
  from_address: string;
  to_addresses: string[];
  subject: string | null;
  status: string;
  tags: string[] | null;
  source: string;
  created_at: string | null;
  delivered_at: string | null;
}

/** Options for {@link Emails.list}. */
export interface ListEmailsParams {
  status?: string;
  page?: number;
  limit?: number;
}

/** Options for {@link Emails.search}. */
export interface SearchEmailsParams {
  q?: string;
  status?: string;
  tag?: string;
  page?: number;
  limit?: number;
}
