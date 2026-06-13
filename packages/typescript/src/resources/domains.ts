/**
 * The `domains` resource: inspect your sending domains.
 * @module
 */
import type { HttpTransport } from '../http';
import type { Domain } from '../types';

/** Accessed as `axene.domains`. */
export class Domains {
  /** @internal */
  constructor(private readonly http: HttpTransport) {}

  /** List your sending domains and their verification status. */
  list(): Promise<Domain[]> {
    return this.http.request<Domain[]>('GET', '/v1/domains/');
  }
}
