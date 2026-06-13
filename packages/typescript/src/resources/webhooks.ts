/**
 * The `webhooks` resource: manage event subscriptions and inspect deliveries.
 * @module
 */
import type { HttpTransport } from '../http';
import { query } from '../internal/query';
import { prune } from '../internal/serialize';
import type {
  CreateWebhookParams,
  Page,
  UpdateWebhookParams,
  Webhook,
  WebhookDelivery,
  WebhookDeliveryDetail,
} from '../types';

/** Accessed as `axene.webhooks`. */
export class Webhooks {
  /** @internal */
  constructor(private readonly http: HttpTransport) {}

  /** List your active webhooks. */
  list(): Promise<Webhook[]> {
    return this.http.request<Webhook[]>('GET', '/v1/webhooks/');
  }

  /** Create a webhook. The signing `secret` is generated and returned. */
  create(params: CreateWebhookParams): Promise<Webhook> {
    return this.http.request<Webhook>('POST', '/v1/webhooks/', { url: params.url, events: params.events });
  }

  /** Update a webhook's url, events, or active state (partial). */
  update(id: string, params: UpdateWebhookParams): Promise<Webhook> {
    return this.http.request<Webhook>(
      'PATCH',
      `/v1/webhooks/${encodeURIComponent(id)}`,
      prune({ url: params.url, events: params.events, is_active: params.isActive }),
    );
  }

  /** Delete a webhook. */
  delete(id: string): Promise<void> {
    return this.http.request<void>('DELETE', `/v1/webhooks/${encodeURIComponent(id)}`);
  }

  /** Queue a sample `email.delivered` delivery to test the endpoint. */
  test(id: string): Promise<{ queued: boolean; url: string }> {
    return this.http.request('POST', `/v1/webhooks/${encodeURIComponent(id)}/test`);
  }

  /** List delivery attempts for a webhook (paginated envelope). */
  listDeliveries(
    id: string,
    params: { page?: number; limit?: number; status?: string } = {},
  ): Promise<Page<WebhookDelivery>> {
    return this.http.request<Page<WebhookDelivery>>(
      'GET',
      `/v1/webhooks/${encodeURIComponent(id)}/deliveries${query(params)}`,
    );
  }

  /** Fetch one delivery with its full payload and the endpoint's response. */
  getDelivery(id: string, deliveryId: string): Promise<WebhookDeliveryDetail> {
    return this.http.request<WebhookDeliveryDetail>(
      'GET',
      `/v1/webhooks/${encodeURIComponent(id)}/deliveries/${encodeURIComponent(deliveryId)}`,
    );
  }
}
