/**
 * The `suppressions` resource: manage the do-not-send list.
 * @module
 */
import type { HttpTransport } from '../http';
import { query } from '../internal/query';
import type { AddSuppressionParams, BulkSuppressionResult, Page, Suppression } from '../types';

/** Accessed as `axene.suppressions`. */
export class Suppressions {
  /** @internal */
  constructor(private readonly http: HttpTransport) {}

  /** List suppressed addresses (paginated envelope; zero-based `page`). */
  list(params: { page?: number; limit?: number; search?: string } = {}): Promise<Page<Suppression>> {
    return this.http.request<Page<Suppression>>('GET', `/v1/suppressions${query(params)}`);
  }

  /** Suppress a single address. */
  add(params: AddSuppressionParams): Promise<Suppression> {
    return this.http.request<Suppression>('POST', '/v1/suppressions', {
      email_address: params.email,
      reason: params.reason ?? 'manual',
    });
  }

  /** Bulk-import suppressions from a file (one email per line). */
  bulkUpload(file: Uint8Array, filename = 'suppressions.txt'): Promise<BulkSuppressionResult> {
    return this.http.upload<BulkSuppressionResult>('/v1/suppressions/bulk', file, filename);
  }

  /** Remove an address from the suppression list. */
  remove(id: string): Promise<void> {
    return this.http.request<void>('DELETE', `/v1/suppressions/${encodeURIComponent(id)}`);
  }
}
