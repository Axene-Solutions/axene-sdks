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

/** Result of {@link Emails.validate}. */
export interface ValidationResult {
  email: string;
  valid: boolean;
  reason?: string;
}

/** A sending domain and its verification status. */
export interface Domain {
  id: string;
  name: string;
  status: string;
  created_at: string | null;
  platform_warning?: string | null;
}
