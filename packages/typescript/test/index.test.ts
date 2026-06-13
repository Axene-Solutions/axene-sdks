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

  it('emails.validate posts the address', async () => {
    const { fn, calls } = mockFetch([{ status: 200, body: { email: 'a@x.co', valid: true } }]);
    await client(fn).emails.validate('a@x.co');
    expect(calls[0]!.url).toBe('https://mail.axene.io/v1/emails/validate');
    expect(JSON.parse(calls[0]!.init.body as string)).toEqual({ email: 'a@x.co' });
  });
});
