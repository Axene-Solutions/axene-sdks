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
export { Contacts } from './resources/contacts';
export { Suppressions } from './resources/suppressions';
export { Templates } from './resources/templates';
export { Webhooks } from './resources/webhooks';
export type {
  AxeneOptions,
  Address,
  Attachment,
  SendEmailParams,
  SendEmailResponse,
  BatchResponse,
  Email,
  EmailDetail,
  EmailEvent,
  EmailSearchHit,
  ScheduledEmail,
  ListEmailsParams,
  SearchEmailsParams,
  ValidationResult,
  ValidationIssue,
  ValidationUsage,
  Page,
  DomainListItem,
  Domain,
  DnsRecord,
  DomainHealth,
  DomainHealthCheck,
  DomainDiagnosis,
  DkimRotation,
  DomainTransfer,
  DomainAvailability,
  DomainCheck,
  ContactList,
  Contact,
  ContactListDetail,
  CreateListParams,
  UpdateListParams,
  AddContactParams,
  CsvImportResult,
  BulkSendParams,
  BulkSendResult,
  Suppression,
  AddSuppressionParams,
  BulkSuppressionResult,
  Template,
  CreateTemplateParams,
  UpdateTemplateParams,
  Webhook,
  CreateWebhookParams,
  UpdateWebhookParams,
  WebhookDelivery,
  WebhookDeliveryDetail,
} from './types';
