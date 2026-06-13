/**
 * The `emails` resource: send, look up, and inspect messages.
 * @module
 */
import type { HttpTransport } from '../http';
import { serializeSend } from '../internal/serialize';
import type {
  BatchResponse,
  Email,
  EmailEvent,
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
   * array of messages.
   */
  sendBatch(emails: SendEmailParams[]): Promise<BatchResponse> {
    return this.http.request<BatchResponse>('POST', '/v1/emails/batch', emails.map(serializeSend));
  }

  /** Fetch a single email and its current status. */
  get(id: string): Promise<Email> {
    return this.http.request<Email>('GET', `/v1/emails/${encodeURIComponent(id)}`);
  }

  /** List delivery / open / click / bounce events for an email. */
  events(id: string): Promise<EmailEvent[]> {
    return this.http.request<EmailEvent[]>('GET', `/v1/emails/${encodeURIComponent(id)}/events`);
  }

  /**
   * Dry-run a send: check whether `message` would be accepted (sender
   * registered, domain verified, plan limits, account not restricted) without
   * actually sending it.
   */
  validate(message: SendEmailParams): Promise<ValidationResult> {
    return this.http.request<ValidationResult>('POST', '/v1/emails/validate', serializeSend(message));
  }
}
