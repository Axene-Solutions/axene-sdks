/**
 * Axene Mailer SDK for TypeScript / JavaScript.
 *
 * Professional email for Africa: send receipts, confirmations, and campaigns
 * from your own domain. Priced in KES, billed via M-Pesa.
 *
 *   import { Axene } from '@axene/mailer';
 *   const axene = new Axene({ apiKey: process.env.AXENE_API_KEY! });
 *   await axene.emails.send({
 *     from: 'hello@yourdomain.com',
 *     to: 'customer@example.com',
 *     subject: 'Your receipt',
 *     html: '<p>Thanks for your order.</p>',
 *   });
 */

export interface AxeneOptions {
  /** API key from your Axene Mailer dashboard (starts with `axm_k_`). */
  apiKey: string;
  /** Override the API base URL. Defaults to https://mail.axene.io */
  baseUrl?: string;
  /** Total attempts on 429 / 5xx (including the first). Defaults to 3. */
  maxRetries?: number;
  /** Per-request timeout in ms. Defaults to 30000. */
  timeoutMs?: number;
  /** Inject a custom fetch (e.g. for Node < 18 or testing). */
  fetch?: typeof fetch;
}

/** A recipient or sender. A bare string is treated as `{ email }`. */
export type Address = string | { email: string; name?: string };

export interface Attachment {
  filename: string;
  /** Base64-encoded file content. */
  content_base64: string;
  content_type?: string;
}

export interface SendEmailParams {
  from: Address;
  to: Address | Address[];
  subject: string;
  html?: string;
  text?: string;
  cc?: Address | Address[];
  bcc?: Address | Address[];
  replyTo?: Address;
  headers?: Record<string, string>;
  tags?: string[];
  /** Schedule for later (ISO 8601 string or Date). Starter plan and up. */
  sendAt?: string | Date;
  attachments?: Attachment[];
}

export interface SendEmailResponse {
  id: string;
  status: string;
  message_id?: string | null;
  rejection_reason?: string | null;
}

export interface EmailEvent {
  id: string;
  type: string;
  created_at: string;
  [k: string]: unknown;
}

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

/** Thrown for any non-2xx API response. */
export class AxeneError extends Error {
  readonly status: number;
  readonly code?: string;
  readonly detail?: unknown;
  constructor(status: number, message: string, code?: string, detail?: unknown) {
    super(message);
    this.name = 'AxeneError';
    this.status = status;
    this.code = code;
    this.detail = detail;
  }
}

const DEFAULT_BASE = 'https://mail.axene.io';
const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));

function toAddress(a: Address): { email: string; name?: string } {
  return typeof a === 'string' ? { email: a } : a;
}
function toAddressList(a: Address | Address[] | undefined): { email: string; name?: string }[] | undefined {
  if (a === undefined) return undefined;
  return (Array.isArray(a) ? a : [a]).map(toAddress);
}

export class Axene {
  readonly emails: Emails;
  readonly domains: Domains;
  private readonly opts: Required<Omit<AxeneOptions, 'fetch'>> & { fetch: typeof fetch };

  constructor(options: AxeneOptions) {
    if (!options?.apiKey) throw new Error('Axene: `apiKey` is required.');
    const f = options.fetch ?? globalThis.fetch;
    if (!f) throw new Error('Axene: no global fetch found. Pass `fetch` in options (Node < 18).');
    this.opts = {
      apiKey: options.apiKey,
      baseUrl: (options.baseUrl ?? DEFAULT_BASE).replace(/\/+$/, ''),
      maxRetries: options.maxRetries ?? 3,
      timeoutMs: options.timeoutMs ?? 30000,
      fetch: f,
    };
    this.emails = new Emails(this);
    this.domains = new Domains(this);
  }

  /** @internal */
  async request<T>(method: string, path: string, body?: unknown): Promise<T> {
    const url = `${this.opts.baseUrl}${path}`;
    let lastErr: unknown;
    for (let attempt = 1; attempt <= this.opts.maxRetries; attempt++) {
      const controller = new AbortController();
      const timer = setTimeout(() => controller.abort(), this.opts.timeoutMs);
      try {
        const res = await this.opts.fetch(url, {
          method,
          headers: {
            Authorization: `Bearer ${this.opts.apiKey}`,
            'Content-Type': 'application/json',
            'User-Agent': '@axene/mailer',
          },
          body: body === undefined ? undefined : JSON.stringify(body),
          signal: controller.signal,
        });
        clearTimeout(timer);

        if (res.status === 429 || res.status >= 500) {
          if (attempt < this.opts.maxRetries) {
            const retryAfter = Number(res.headers.get('retry-after'));
            await sleep(Number.isFinite(retryAfter) && retryAfter > 0 ? retryAfter * 1000 : 250 * 2 ** (attempt - 1));
            continue;
          }
        }

        const isJson = (res.headers.get('content-type') ?? '').includes('application/json');
        const payload = isJson ? await res.json().catch(() => undefined) : undefined;
        if (!res.ok) {
          const d = (payload as { detail?: unknown } | undefined)?.detail;
          const code = typeof d === 'object' && d ? (d as { code?: string }).code : undefined;
          const message =
            (typeof d === 'object' && d ? (d as { message?: string }).message : undefined) ??
            (typeof d === 'string' ? d : undefined) ??
            `Axene request failed (${res.status})`;
          throw new AxeneError(res.status, message, code, payload);
        }
        return payload as T;
      } catch (err) {
        clearTimeout(timer);
        if (err instanceof AxeneError) throw err;
        lastErr = err;
        if (attempt < this.opts.maxRetries) {
          await sleep(250 * 2 ** (attempt - 1));
          continue;
        }
      }
    }
    throw new AxeneError(0, `Axene request failed: ${String(lastErr)}`);
  }
}

class Emails {
  constructor(private readonly client: Axene) {}

  /** Send a single email. */
  send(params: SendEmailParams): Promise<SendEmailResponse> {
    return this.client.request<SendEmailResponse>('POST', '/v1/emails/', serializeSend(params));
  }

  /** Send up to your plan's batch limit in one call. */
  sendBatch(emails: SendEmailParams[]): Promise<{ results: SendEmailResponse[] }> {
    return this.client.request('POST', '/v1/emails/batch', { emails: emails.map(serializeSend) });
  }

  /** Fetch a single email and its current status. */
  get(id: string): Promise<Email> {
    return this.client.request<Email>('GET', `/v1/emails/${encodeURIComponent(id)}`);
  }

  /** List delivery / open / click / bounce events for an email. */
  events(id: string): Promise<EmailEvent[]> {
    return this.client.request<EmailEvent[]>('GET', `/v1/emails/${encodeURIComponent(id)}/events`);
  }

  /** Validate that an address is well-formed and its domain can receive mail. */
  validate(email: string): Promise<{ email: string; valid: boolean; reason?: string }> {
    return this.client.request('POST', '/v1/emails/validate', { email });
  }
}

class Domains {
  constructor(private readonly client: Axene) {}
  /** List your sending domains and their verification status. */
  list(): Promise<unknown[]> {
    return this.client.request<unknown[]>('GET', '/v1/domains/');
  }
}

function serializeSend(p: SendEmailParams): Record<string, unknown> {
  // The API field for the sender is wire-named `from_`; we expose a clean `from`.
  const sendAt = p.sendAt instanceof Date ? p.sendAt.toISOString() : p.sendAt;
  return prune({
    from_: toAddress(p.from),
    to: toAddressList(p.to),
    subject: p.subject,
    html: p.html,
    text: p.text,
    cc: toAddressList(p.cc),
    bcc: toAddressList(p.bcc),
    reply_to: p.replyTo ? toAddress(p.replyTo) : undefined,
    headers: p.headers,
    tags: p.tags,
    send_at: sendAt,
    attachments: p.attachments,
  });
}

function prune(o: Record<string, unknown>): Record<string, unknown> {
  for (const k of Object.keys(o)) if (o[k] === undefined) delete o[k];
  return o;
}
