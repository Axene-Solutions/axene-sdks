/**
 * The `emails` resource: send, look up, search, schedule, and inspect messages.
 * @module
 */
import type { HttpTransport } from '../http';
import { serializeSend } from '../internal/serialize';
import { query } from '../internal/query';
import type {
  BatchResponse,
  Email,
  EmailDetail,
  EmailEvent,
  EmailSearchHit,
  ListEmailsParams,
  ScheduledEmail,
  SearchEmailsParams,
  SendEmailParams,
  SendEmailResponse,
  ValidationResult,
} from '../types';

/** Accessed as `axene.emails`. */
export class Emails {
  /** @internal */
  constructor(private readonly http: HttpTransport) {}

  /** Send a single email. */
  send(params: SendEmailParams): Promise<SendEmailResponse> {
    return this.http.request<SendEmailResponse>('POST', '/v1/emails/', serializeSend(params));
  }

  /**
   * Send up to your plan's batch limit in one call. The API accepts a bare
   * array of messages and returns a per-message result set.
   */
  sendBatch(emails: SendEmailParams[]): Promise<BatchResponse> {
    return this.http.request<BatchResponse>('POST', '/v1/emails/batch', emails.map(serializeSend));
  }

  /**
   * Dry-run a send: check whether `message` would be accepted (sender
   * registered, domain verified, plan limits, account not restricted) without
   * actually sending it.
   */
  validate(message: SendEmailParams): Promise<ValidationResult> {
    return this.http.request<ValidationResult>('POST', '/v1/emails/validate', serializeSend(message));
  }

  /** List recent emails, newest first. */
  list(params: ListEmailsParams = {}): Promise<Email[]> {
    return this.http.request<Email[]>('GET', `/v1/emails/${query(params)}`);
  }

  /** Fetch a single email with its bodies and events. */
  get(id: string): Promise<EmailDetail> {
    return this.http.request<EmailDetail>('GET', `/v1/emails/${encodeURIComponent(id)}`);
  }

  /** List delivery / open / click / bounce events for an email. */
  events(id: string): Promise<EmailEvent[]> {
    return this.http.request<EmailEvent[]>('GET', `/v1/emails/${encodeURIComponent(id)}/events`);
  }

  /** Re-send a bounced, rejected, or failed email as a new message. */
  retry(id: string): Promise<SendEmailResponse> {
    return this.http.request<SendEmailResponse>('POST', `/v1/emails/${encodeURIComponent(id)}/retry`);
  }

  /**
   * Search emails. `q` supports inline tokens (`to:`, `from:`, `status:`,
   * `domain:`, `tag:`); leftover words are matched as free text.
   */
  search(params: SearchEmailsParams = {}): Promise<EmailSearchHit[]> {
    return this.http.request<EmailSearchHit[]>('GET', `/v1/emails/search${query(params)}`);
  }

  /** List emails scheduled for future delivery, soonest first. */
  listScheduled(): Promise<ScheduledEmail[]> {
    return this.http.request<ScheduledEmail[]>('GET', '/v1/emails/scheduled');
  }

  /** Cancel a scheduled email. */
  cancelScheduled(id: string): Promise<{ id: string; status: string }> {
    return this.http.request('DELETE', `/v1/emails/scheduled/${encodeURIComponent(id)}`);
  }

  /** Send a scheduled email immediately instead of waiting. */
  sendScheduledNow(id: string): Promise<{ id: string; status: string }> {
    return this.http.request('POST', `/v1/emails/scheduled/${encodeURIComponent(id)}/send-now`);
  }

  /**
   * Poll for emails whose status changed at or after `since` (ISO 8601 or a
   * `Date`). Capped at 50 rows; use for live status updates.
   */
  updates(since: string | Date): Promise<Email[]> {
    const iso = since instanceof Date ? since.toISOString() : since;
    return this.http.request<Email[]>('GET', `/v1/emails/updates${query({ since: iso })}`);
  }

  /** Get the caller's saved searches. */
  getSavedSearches(): Promise<unknown[]> {
    return this.http
      .request<{ searches: unknown[] }>('GET', '/v1/emails/saved-searches')
      .then((r) => r.searches);
  }

  /** Replace the caller's saved searches (max 50). */
  setSavedSearches(searches: unknown[]): Promise<unknown[]> {
    return this.http
      .request<{ searches: unknown[] }>('PUT', '/v1/emails/saved-searches', { searches })
      .then((r) => r.searches);
  }
}
