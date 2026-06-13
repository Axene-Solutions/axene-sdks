/**
 * The Axene client. Composes the HTTP transport with the resource groups
 * (`emails`, `domains`). This is the entry point most code touches.
 * @module
 */
import { HttpTransport } from './http';
import { Emails } from './resources/emails';
import { Domains } from './resources/domains';
import type { AxeneOptions } from './types';

/**
 * Axene Mailer API client.
 *
 * @example
 * ```ts
 * import { Axene } from '@axene/mailer';
 *
 * const axene = new Axene({ apiKey: process.env.AXENE_API_KEY! });
 * await axene.emails.send({
 *   from: 'hello@yourdomain.com',
 *   to: 'customer@example.com',
 *   subject: 'Your receipt',
 *   html: '<p>Thanks for your order.</p>',
 * });
 * ```
 */
export class Axene {
  /** Send and inspect emails. */
  readonly emails: Emails;
  /** Inspect your sending domains. */
  readonly domains: Domains;

  constructor(options: AxeneOptions) {
    const http = new HttpTransport(options);
    this.emails = new Emails(http);
    this.domains = new Domains(http);
  }
}
