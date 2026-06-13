/**
 * Internal helpers that translate the SDK's ergonomic types into the exact
 * JSON shape the API expects. Not part of the public API.
 * @module
 */
import type { Address, SendEmailParams } from '../types';

/** Normalize a single address (a bare string becomes `{ email }`). */
function toAddress(a: Address): { email: string; name?: string } {
  return typeof a === 'string' ? { email: a } : a;
}

/** Normalize one-or-many addresses into an array, or `undefined` if absent. */
function toAddressList(
  a: Address | Address[] | undefined,
): { email: string; name?: string }[] | undefined {
  if (a === undefined) return undefined;
  return (Array.isArray(a) ? a : [a]).map(toAddress);
}

/** Drop keys whose value is `undefined` so they are omitted from the JSON body. */
function prune(o: Record<string, unknown>): Record<string, unknown> {
  for (const k of Object.keys(o)) if (o[k] === undefined) delete o[k];
  return o;
}

/**
 * Build the JSON body for a send request.
 *
 * The API names the sender field `from_` on the wire; the SDK exposes a clean
 * `from`, so the mapping happens here in one place.
 */
export function serializeSend(p: SendEmailParams): Record<string, unknown> {
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
