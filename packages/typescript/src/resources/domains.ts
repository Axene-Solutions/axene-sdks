/**
 * The `domains` resource: register, verify, inspect, and transfer sending domains.
 * @module
 */
import type { HttpTransport } from '../http';
import { query } from '../internal/query';
import type {
  DkimRotation,
  Domain,
  DomainAvailability,
  DomainCheck,
  DomainDiagnosis,
  DomainHealth,
  DomainListItem,
  DomainTransfer,
} from '../types';

/** Accessed as `axene.domains`. */
export class Domains {
  /** @internal */
  constructor(private readonly http: HttpTransport) {}

  /** List your sending domains and their verification status. */
  list(): Promise<DomainListItem[]> {
    return this.http.request<DomainListItem[]>('GET', '/v1/domains/');
  }

  /** Register a new sending domain. Returns the DNS records to publish. */
  create(name: string): Promise<Domain> {
    return this.http.request<Domain>('POST', '/v1/domains/', { name });
  }

  /** Fetch a domain with its DKIM selector and DNS records. */
  get(id: string): Promise<Domain> {
    return this.http.request<Domain>('GET', `/v1/domains/${encodeURIComponent(id)}`);
  }

  /** Delete a domain. */
  delete(id: string): Promise<void> {
    return this.http.request<void>('DELETE', `/v1/domains/${encodeURIComponent(id)}`);
  }

  /** Re-check DNS and verify the domain. */
  verify(id: string): Promise<Domain> {
    return this.http.request<Domain>('POST', `/v1/domains/${encodeURIComponent(id)}/verify`);
  }

  /** Run live DNS health checks (DKIM, SPF, DMARC, return-path, MX). */
  health(id: string): Promise<DomainHealth> {
    return this.http.request<DomainHealth>('GET', `/v1/domains/${encodeURIComponent(id)}/health`);
  }

  /** Diagnose configuration issues and get a health score. */
  diagnose(id: string): Promise<DomainDiagnosis> {
    return this.http.request<DomainDiagnosis>('GET', `/v1/domains/${encodeURIComponent(id)}/diagnose`);
  }

  /** Current MX status for inbound/forwarding (shape varies by provider). */
  mxStatus(id: string): Promise<Record<string, unknown>> {
    return this.http.request('GET', `/v1/domains/${encodeURIComponent(id)}/mx-status`);
  }

  /** The values currently published in DNS for each of the domain's records. */
  publishedRecords(id: string): Promise<Record<string, unknown>> {
    return this.http.request('GET', `/v1/domains/${encodeURIComponent(id)}/published-records`);
  }

  /** Rotate the domain's DKIM key, returning the new record to publish. */
  rotateDkim(id: string): Promise<DkimRotation> {
    return this.http.request<DkimRotation>('POST', `/v1/domains/${encodeURIComponent(id)}/rotate-dkim`);
  }

  /** Initiate a transfer of this domain to another Axene account. */
  transfer(id: string, params: { targetEmail: string; note?: string | null }): Promise<DomainTransfer> {
    return this.http.request<DomainTransfer>('POST', `/v1/domains/${encodeURIComponent(id)}/transfer`, {
      target_email: params.targetEmail,
      note: params.note ?? null,
    });
  }

  /** Check whether a domain name is available to add (checks public DNS). */
  checkAvailability(name: string): Promise<DomainAvailability> {
    return this.http.request<DomainAvailability>('GET', `/v1/domains/check-availability${query({ name })}`);
  }

  /** Check whether a domain name already exists in your account. */
  check(name: string): Promise<DomainCheck> {
    return this.http.request<DomainCheck>('GET', `/v1/domains/check/${encodeURIComponent(name)}`);
  }
}
