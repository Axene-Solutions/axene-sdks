/**
 * The Axene client. Composes the HTTP transport with the resource groups. This
 * is the entry point most code touches.
 * @module
 */
import { HttpTransport } from './http';
import { Emails } from './resources/emails';
import { Domains } from './resources/domains';
import { Contacts } from './resources/contacts';
import { Suppressions } from './resources/suppressions';
import { Templates } from './resources/templates';
import { Webhooks } from './resources/webhooks';
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
  /** Send, search, schedule, and inspect emails. */
  readonly emails: Emails;
  /** Register, verify, and transfer sending domains. */
  readonly domains: Domains;
  /** Manage subscriber lists and bulk sends. */
  readonly contacts: Contacts;
  /** Manage the do-not-send suppression list. */
  readonly suppressions: Suppressions;
  /** Manage reusable email templates. */
  readonly templates: Templates;
  /** Manage event webhooks and inspect deliveries. */
  readonly webhooks: Webhooks;

  constructor(options: AxeneOptions) {
    const http = new HttpTransport(options);
    this.emails = new Emails(http);
    this.domains = new Domains(http);
    this.contacts = new Contacts(http);
    this.suppressions = new Suppressions(http);
    this.templates = new Templates(http);
    this.webhooks = new Webhooks(http);
  }
}
