import { describe, it, expect, vi } from 'vitest';
import { Axene, AxeneError } from '../src/index';

/** Build a fake fetch that records calls and returns a scripted sequence of responses. */
function mockFetch(responses: Array<{ status: number; body?: unknown; headers?: Record<string, string> }>) {
  const calls: Array<{ url: string; init: RequestInit }> = [];
  let i = 0;
  const fn = vi.fn(async (url: string, init: RequestInit) => {
    calls.push({ url, init });
    const r = responses[Math.min(i, responses.length - 1)];
    i++;
    const headers = new Headers({ 'content-type': 'application/json', ...(r.headers ?? {}) });
    return {
      ok: r.status >= 200 && r.status < 300,
      status: r.status,
      headers,
      json: async () => r.body,
    } as unknown as Response;
  });
  return { fn: fn as unknown as typeof fetch, calls };
}

function client(fetchImpl: typeof fetch, extra = {}) {
  return new Axene({ apiKey: 'axm_k_test', fetch: fetchImpl, maxRetries: 3, ...extra });
}

describe('Axene constructor', () => {
  it('requires an apiKey', () => {
    // @ts-expect-error intentionally missing
    expect(() => new Axene({ fetch: globalThis.fetch })).toThrow(/apiKey/);
  });
});

describe('emails.send', () => {
  it('POSTs to /v1/emails/ with bearer auth and maps `from` -> `from_`', async () => {
    const { fn, calls } = mockFetch([{ status: 202, body: { id: 'em_1', status: 'queued' } }]);
    const res = await client(fn).emails.send({
      from: { email: 'hello@shop.co', name: 'Shop' },
      to: 'a@example.com',
      subject: 'Hi',
      html: '<p>x</p>',
    });

    expect(res).toEqual({ id: 'em_1', status: 'queued' });
    expect(calls).toHaveLength(1);
    const { url, init } = calls[0]!;
    expect(url).toBe('https://mail.axene.io/v1/emails/');
    expect(init.method).toBe('POST');
    expect((init.headers as Record<string, string>).Authorization).toBe('Bearer axm_k_test');

    const body = JSON.parse(init.body as string);
    expect(body.from_).toEqual({ email: 'hello@shop.co', name: 'Shop' }); // mapped + named
    expect(body.from).toBeUndefined();
    expect(body.to).toEqual([{ email: 'a@example.com' }]); // string normalized + arrayified
    expect(body.subject).toBe('Hi');
    expect('text' in body).toBe(false); // undefined fields pruned
  });

  it('accepts string, object, and array recipients', async () => {
    const { fn, calls } = mockFetch([{ status: 202, body: { id: 'x', status: 'queued' } }]);
    await client(fn).emails.send({
      from: 'f@x.co',
      to: ['a@x.co', { email: 'b@x.co', name: 'Bee' }],
      cc: 'c@x.co',
      subject: 's',
    });
    const body = JSON.parse(calls[0]!.init.body as string);
    expect(body.to).toEqual([{ email: 'a@x.co' }, { email: 'b@x.co', name: 'Bee' }]);
    expect(body.cc).toEqual([{ email: 'c@x.co' }]);
  });

  it('serializes a Date sendAt to ISO', async () => {
    const { fn, calls } = mockFetch([{ status: 202, body: { id: 'x', status: 'scheduled' } }]);
    const when = new Date('2030-01-01T00:00:00.000Z');
    await client(fn).emails.send({ from: 'f@x.co', to: 'a@x.co', subject: 's', sendAt: when });
    expect(JSON.parse(calls[0]!.init.body as string).send_at).toBe('2030-01-01T00:00:00.000Z');
  });
});

describe('errors', () => {
  it('throws AxeneError with code/message on 4xx', async () => {
    const { fn } = mockFetch([{ status: 422, body: { detail: { code: 'invalid', message: 'bad from' } } }]);
    await expect(client(fn).emails.send({ from: 'f@x.co', to: 'a@x.co', subject: 's' })).rejects.toMatchObject({
      name: 'AxeneError',
      status: 422,
      code: 'invalid',
      message: 'bad from',
    });
  });

  it('does not retry 4xx', async () => {
    const { fn, calls } = mockFetch([{ status: 401, body: { detail: 'nope' } }]);
    await expect(client(fn).emails.send({ from: 'f@x.co', to: 'a@x.co', subject: 's' })).rejects.toBeInstanceOf(
      AxeneError,
    );
    expect(calls).toHaveLength(1);
  });
});

describe('retries', () => {
  it('retries 5xx then succeeds', async () => {
    const { fn, calls } = mockFetch([
      { status: 503 },
      { status: 503 },
      { status: 202, body: { id: 'ok', status: 'queued' } },
    ]);
    const res = await client(fn).emails.send({ from: 'f@x.co', to: 'a@x.co', subject: 's' });
    expect(res.id).toBe('ok');
    expect(calls).toHaveLength(3);
  });

  it('retries 429', async () => {
    const { fn, calls } = mockFetch([{ status: 429 }, { status: 202, body: { id: 'ok', status: 'queued' } }]);
    await client(fn).emails.send({ from: 'f@x.co', to: 'a@x.co', subject: 's' });
    expect(calls).toHaveLength(2);
  });
});

describe('other endpoints', () => {
  it('emails.get fetches by id', async () => {
    const { fn, calls } = mockFetch([{ status: 200, body: { id: 'em_9', status: 'delivered' } }]);
    await client(fn).emails.get('em_9');
    expect(calls[0]!.url).toBe('https://mail.axene.io/v1/emails/em_9');
    expect(calls[0]!.init.method).toBe('GET');
  });

  it('emails.validate dry-runs a full message', async () => {
    const { fn, calls } = mockFetch([
      { status: 200, body: { valid: true, can_send: true, issues: [], plan: 'free', usage: { daily: 0, daily_limit: 100, monthly: 0, monthly_limit: 3000 } } },
    ]);
    const res = await client(fn).emails.validate({ from: 'f@x.co', to: 'a@x.co', subject: 's' });
    expect(res.can_send).toBe(true);
    expect(calls[0]!.url).toBe('https://mail.axene.io/v1/emails/validate');
    const body = JSON.parse(calls[0]!.init.body as string);
    expect(body.from_).toEqual({ email: 'f@x.co' }); // full send body, not { email }
    expect(body.to).toEqual([{ email: 'a@x.co' }]);
  });

  it('emails.sendBatch posts a bare array', async () => {
    const { fn, calls } = mockFetch([
      { status: 202, body: { total: 1, sent: 1, failed: 0, results: [{ id: 'a', status: 'queued' }] } },
    ]);
    const res = await client(fn).emails.sendBatch([{ from: 'f@x.co', to: 'a@x.co', subject: 's' }]);
    expect(res.total).toBe(1);
    expect(res.results[0]!.id).toBe('a');
    expect(calls[0]!.url).toBe('https://mail.axene.io/v1/emails/batch');
    const body = JSON.parse(calls[0]!.init.body as string);
    expect(Array.isArray(body)).toBe(true); // bare array, not { emails: [...] }
    expect(body[0].from_).toEqual({ email: 'f@x.co' });
  });

  it('emails.search builds a query string and returns hits', async () => {
    const { fn, calls } = mockFetch([{ status: 200, body: [{ id: 'em_1', status: 'delivered' }] }]);
    await client(fn).emails.search({ q: 'status:bounced', page: 1, limit: 10 });
    expect(calls[0]!.url).toBe('https://mail.axene.io/v1/emails/search?q=status%3Abounced&page=1&limit=10');
  });

  it('emails.updates requires since and serializes a Date', async () => {
    const { fn, calls } = mockFetch([{ status: 200, body: [] }]);
    await client(fn).emails.updates(new Date('2030-01-01T00:00:00.000Z'));
    expect(calls[0]!.url).toBe('https://mail.axene.io/v1/emails/updates?since=2030-01-01T00%3A00%3A00.000Z');
  });
});

describe('contacts', () => {
  it('createList maps iconSeed -> icon_seed and prunes undefined', async () => {
    const { fn, calls } = mockFetch([{ status: 201, body: { id: 'cl_1', name: 'VIPs', contact_count: 0 } }]);
    await client(fn).contacts.createList({ name: 'VIPs', iconSeed: 'abc' });
    expect(calls[0]!.url).toBe('https://mail.axene.io/v1/contacts/');
    const body = JSON.parse(calls[0]!.init.body as string);
    expect(body).toEqual({ name: 'VIPs', icon_seed: 'abc' }); // description pruned
  });

  it('bulkSend injects contact_list_id and renames sender field', async () => {
    const { fn, calls } = mockFetch([{ status: 200, body: { queued: 2, skipped: 0, errors: [] } }]);
    await client(fn).contacts.bulkSend('cl_9', { senderAddressId: 'sa_1', subject: 'Hi {{name}}' });
    const body = JSON.parse(calls[0]!.init.body as string);
    expect(body.contact_list_id).toBe('cl_9');
    expect(body.sender_address_id).toBe('sa_1');
    expect(calls[0]!.url).toBe('https://mail.axene.io/v1/contacts/cl_9/send');
  });
});

describe('suppressions', () => {
  it('add maps email -> email_address with default reason', async () => {
    const { fn, calls } = mockFetch([{ status: 201, body: { id: 's_1', email_address: 'x@y.co', reason: 'manual' } }]);
    await client(fn).suppressions.add({ email: 'x@y.co' });
    const body = JSON.parse(calls[0]!.init.body as string);
    expect(body).toEqual({ email_address: 'x@y.co', reason: 'manual' });
  });

  it('list parses the {items,total,page,limit} envelope', async () => {
    const { fn } = mockFetch([{ status: 200, body: { items: [{ id: 's_1' }], total: 1, page: 0, limit: 50 } }]);
    const page = await client(fn).suppressions.list({ search: 'spam' });
    expect(page.total).toBe(1);
    expect(page.items[0]!.id).toBe('s_1');
  });
});

describe('templates', () => {
  it('create maps html/text -> html_body/text_body', async () => {
    const { fn, calls } = mockFetch([{ status: 201, body: { id: 't_1', name: 'Welcome' } }]);
    await client(fn).templates.create({ name: 'Welcome', html: '<p>hi</p>', text: 'hi' });
    const body = JSON.parse(calls[0]!.init.body as string);
    expect(body).toEqual({ name: 'Welcome', html_body: '<p>hi</p>', text_body: 'hi' });
  });
});

describe('webhooks', () => {
  it('create posts url + events', async () => {
    const { fn, calls } = mockFetch([{ status: 201, body: { id: 'wh_1', url: 'https://x.co/h', events: ['email.delivered'] } }]);
    await client(fn).webhooks.create({ url: 'https://x.co/h', events: ['email.delivered'] });
    const body = JSON.parse(calls[0]!.init.body as string);
    expect(body).toEqual({ url: 'https://x.co/h', events: ['email.delivered'] });
  });

  it('update maps isActive -> is_active and prunes', async () => {
    const { fn, calls } = mockFetch([{ status: 200, body: { id: 'wh_1', is_active: false } }]);
    await client(fn).webhooks.update('wh_1', { isActive: false });
    const body = JSON.parse(calls[0]!.init.body as string);
    expect(body).toEqual({ is_active: false }); // url/events pruned
    expect(calls[0]!.init.method).toBe('PATCH');
  });

  it('listDeliveries parses the envelope with a status filter', async () => {
    const { fn, calls } = mockFetch([{ status: 200, body: { items: [], total: 0, page: 0, limit: 20 } }]);
    await client(fn).webhooks.listDeliveries('wh_1', { status: 'failed' });
    expect(calls[0]!.url).toBe('https://mail.axene.io/v1/webhooks/wh_1/deliveries?status=failed');
  });
});

describe('domains', () => {
  it('create posts the name and parses dns_records', async () => {
    const { fn, calls } = mockFetch([
      { status: 201, body: { id: 'd_1', name: 'shop.co', status: 'pending', dkim_selector: 'axene', dns_records: [] } },
    ]);
    const d = await client(fn).domains.create('shop.co');
    expect(d.dkim_selector).toBe('axene');
    const body = JSON.parse(calls[0]!.init.body as string);
    expect(body).toEqual({ name: 'shop.co' });
  });

  it('transfer renames targetEmail -> target_email', async () => {
    const { fn, calls } = mockFetch([{ status: 200, body: { id: 'dt_1', target_email: 'new@owner.co', status: 'pending' } }]);
    await client(fn).domains.transfer('d_1', { targetEmail: 'new@owner.co' });
    const body = JSON.parse(calls[0]!.init.body as string);
    expect(body).toEqual({ target_email: 'new@owner.co', note: null });
  });
});
