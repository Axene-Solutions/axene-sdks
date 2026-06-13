/**
 * HTTP transport: the single place that talks to the network. Owns
 * authentication, JSON encoding, timeouts, retries with backoff, and turning
 * non-2xx responses into {@link AxeneError}. Resources depend on this, not on
 * `fetch` directly.
 * @module
 */
import { AxeneError } from './errors';
import type { AxeneOptions } from './types';

const DEFAULT_BASE = 'https://mail.axene.io';

const sleep = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

/** Resolved, validated transport configuration. */
type ResolvedConfig = Required<Omit<AxeneOptions, 'fetch'>> & { fetch: typeof fetch };

export class HttpTransport {
  private readonly cfg: ResolvedConfig;

  constructor(options: AxeneOptions) {
    if (!options?.apiKey) throw new Error('Axene: `apiKey` is required.');
    const f = options.fetch ?? globalThis.fetch;
    if (!f) throw new Error('Axene: no global fetch found. Pass `fetch` in options (Node < 18).');
    this.cfg = {
      apiKey: options.apiKey,
      baseUrl: (options.baseUrl ?? DEFAULT_BASE).replace(/\/+$/, ''),
      maxRetries: options.maxRetries ?? 3,
      timeoutMs: options.timeoutMs ?? 30000,
      fetch: f,
    };
  }

  /**
   * Perform a request and parse the JSON response.
   *
   * Retries `429` and `5xx` with exponential backoff (honoring `Retry-After`
   * when present). Throws {@link AxeneError} on a final non-2xx or a transport
   * failure that survives all attempts.
   */
  async request<T>(method: string, path: string, body?: unknown): Promise<T> {
    const url = `${this.cfg.baseUrl}${path}`;
    let lastError: unknown;

    for (let attempt = 1; attempt <= this.cfg.maxRetries; attempt++) {
      const controller = new AbortController();
      const timer = setTimeout(() => controller.abort(), this.cfg.timeoutMs);
      try {
        const res = await this.cfg.fetch(url, {
          method,
          headers: {
            Authorization: `Bearer ${this.cfg.apiKey}`,
            'Content-Type': 'application/json',
            'User-Agent': '@axene/mailer',
          },
          body: body === undefined ? undefined : JSON.stringify(body),
          signal: controller.signal,
        });
        clearTimeout(timer);

        if (this.isRetryable(res.status) && attempt < this.cfg.maxRetries) {
          await sleep(this.backoffMs(res, attempt));
          continue;
        }

        const payload = await this.parseBody(res);
        if (!res.ok) throw this.toError(res.status, payload);
        return payload as T;
      } catch (err) {
        clearTimeout(timer);
        if (err instanceof AxeneError) throw err; // a real API error: do not retry
        lastError = err; // transport/abort error: retry if attempts remain
        if (attempt < this.cfg.maxRetries) {
          await sleep(this.backoffMs(undefined, attempt));
          continue;
        }
      }
    }
    throw new AxeneError(0, `Axene request failed: ${String(lastError)}`);
  }

  /**
   * Upload a single file as `multipart/form-data` under the field name `file`.
   * Used by the CSV import endpoints. Does not set `Content-Type` so the runtime
   * adds the correct multipart boundary. Not retried (uploads are not idempotent).
   */
  async upload<T>(path: string, file: Uint8Array, filename: string): Promise<T> {
    const url = `${this.cfg.baseUrl}${path}`;
    const form = new FormData();
    const blob = new Blob([file.slice()]);
    form.append('file', blob, filename);

    const controller = new AbortController();
    const timer = setTimeout(() => controller.abort(), this.cfg.timeoutMs);
    try {
      const res = await this.cfg.fetch(url, {
        method: 'POST',
        headers: { Authorization: `Bearer ${this.cfg.apiKey}`, 'User-Agent': '@axene/mailer' },
        body: form as unknown as BodyInit,
        signal: controller.signal,
      });
      const payload = await this.parseBody(res);
      if (!res.ok) throw this.toError(res.status, payload);
      return payload as T;
    } finally {
      clearTimeout(timer);
    }
  }

  private isRetryable(status: number): boolean {
    return status === 429 || status >= 500;
  }

  private backoffMs(res: Response | undefined, attempt: number): number {
    const retryAfter = res ? Number(res.headers.get('retry-after')) : NaN;
    if (Number.isFinite(retryAfter) && retryAfter > 0) return retryAfter * 1000;
    return 250 * 2 ** (attempt - 1);
  }

  private async parseBody(res: Response): Promise<unknown> {
    const isJson = (res.headers.get('content-type') ?? '').includes('application/json');
    return isJson ? res.json().catch(() => undefined) : undefined;
  }

  /** Map the API's `{ detail: { code, message } }` (or string) into an {@link AxeneError}. */
  private toError(status: number, payload: unknown): AxeneError {
    const detail = (payload as { detail?: unknown } | undefined)?.detail;
    const code = typeof detail === 'object' && detail ? (detail as { code?: string }).code : undefined;
    const message =
      (typeof detail === 'object' && detail ? (detail as { message?: string }).message : undefined) ??
      (typeof detail === 'string' ? detail : undefined) ??
      `Axene request failed (${status})`;
    return new AxeneError(status, message, code, payload);
  }
}
