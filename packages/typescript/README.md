# @axene/mailer

Official TypeScript / JavaScript SDK for [Axene Mailer](https://axene.io).
Send receipts, confirmations, and campaigns from your own domain — priced in KES, billed via M-Pesa.

Zero runtime dependencies. Works in Node 18+, Bun, Deno, and edge runtimes (anywhere `fetch` exists).

## Install

```bash
npm install @axene/mailer
```

## Usage

```ts
import { Axene } from '@axene/mailer';

const axene = new Axene({ apiKey: process.env.AXENE_API_KEY! });

const { id } = await axene.emails.send({
  from: { email: 'hello@yourdomain.com', name: 'Your Shop' },
  to: 'customer@example.com',
  subject: 'Your receipt',
  html: '<p>Thanks for your order.</p>',
  text: 'Thanks for your order.',
});

console.log('queued', id);
```

### Recipients

`to`, `cc`, `bcc` accept a string, an object, or arrays of either:

```ts
to: 'a@example.com'
to: { email: 'a@example.com', name: 'Ada' }
to: ['a@example.com', { email: 'b@example.com', name: 'Bee' }]
```

### More

```ts
await axene.emails.sendBatch([ /* up to your plan's batch limit */ ]);
const email = await axene.emails.get(id);            // status
const events = await axene.emails.events(id);         // delivered / opened / clicked / bounced
await axene.emails.validate('someone@example.com');   // address check
await axene.domains.list();                           // your sending domains

// Scheduling (Starter plan and up)
await axene.emails.send({ from, to, subject, html, sendAt: new Date(Date.now() + 3600_000) });
```

### Errors & retries

Non-2xx responses throw `AxeneError` (`.status`, `.code`, `.message`). The client
automatically retries `429` and `5xx` with exponential backoff (configurable via
`maxRetries`).

```ts
import { Axene, AxeneError } from '@axene/mailer';
try {
  await axene.emails.send({ /* … */ });
} catch (e) {
  if (e instanceof AxeneError) console.error(e.status, e.code, e.message);
}
```

### Options

```ts
new Axene({
  apiKey: '…',                       // required, starts with axm_k_
  baseUrl: 'https://mail.axene.io',  // override if self-hosting
  maxRetries: 3,
  timeoutMs: 30000,
  fetch: customFetch,                // for Node < 18
});
```

Get an API key at [mail.axene.io](https://mail.axene.io). Docs: <https://axene.io/docs/mailer/getting-started/welcome>.

MIT © Axene Solutions
