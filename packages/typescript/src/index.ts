/**
 * Axene Mailer SDK for TypeScript / JavaScript.
 *
 * Professional email for Africa: send receipts, confirmations, and campaigns
 * from your own domain. Priced in KES, billed via M-Pesa.
 *
 * @example
 * ```ts
 * import { Axene } from '@axene/mailer';
 * const axene = new Axene({ apiKey: process.env.AXENE_API_KEY! });
 * const { id } = await axene.emails.send({
 *   from: 'hello@yourdomain.com',
 *   to: 'customer@example.com',
 *   subject: 'Your receipt',
 *   html: '<p>Thanks for your order.</p>',
 * });
 * ```
 *
 * @packageDocumentation
 */
export { Axene } from './client';
export { AxeneError } from './errors';
export { Emails } from './resources/emails';
export { Domains } from './resources/domains';
export type {
  AxeneOptions,
  Address,
  Attachment,
  SendEmailParams,
  SendEmailResponse,
  BatchResponse,
  Email,
  EmailEvent,
  ValidationResult,
  Domain,
} from './types';
