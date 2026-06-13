<?php

declare(strict_types=1);

namespace Axene\Mailer\Resources;

use Axene\Mailer\Internal;
use Axene\Mailer\Transport;

/**
 * The `webhooks` resource: manage event subscriptions and inspect deliveries.
 * Accessed as `$client->webhooks`.
 */
final class Webhooks
{
    /** @internal */
    public function __construct(private readonly Transport $transport)
    {
    }

    /**
     * List your active webhooks.
     *
     * @return list<array<string, mixed>>
     */
    public function list(): array
    {
        return $this->transport->request('GET', '/v1/webhooks/');
    }

    /**
     * Create a webhook. The signing `secret` is generated and returned.
     *
     * @param array{url: string, events: list<string>} $params
     * @return array<string, mixed>
     */
    public function create(array $params): array
    {
        return $this->transport->request('POST', '/v1/webhooks/', [
            'url' => $params['url'],
            'events' => $params['events'],
        ]);
    }

    /**
     * Update a webhook's url, events, or active state (partial). The SDK's
     * `isActive` maps to the wire field `is_active`.
     *
     * @param array{url?: string, events?: list<string>, isActive?: bool} $params
     * @return array<string, mixed>
     */
    public function update(string $id, array $params): array
    {
        return $this->transport->request('PATCH', '/v1/webhooks/' . rawurlencode($id), Internal::prune([
            'url' => $params['url'] ?? null,
            'events' => $params['events'] ?? null,
            'is_active' => $params['isActive'] ?? null,
        ]));
    }

    /**
     * Delete a webhook.
     */
    public function delete(string $id): void
    {
        $this->transport->request('DELETE', '/v1/webhooks/' . rawurlencode($id));
    }

    /**
     * Queue a sample `email.delivered` delivery to test the endpoint.
     *
     * @return array<string, mixed>
     */
    public function test(string $id): array
    {
        return $this->transport->request('POST', '/v1/webhooks/' . rawurlencode($id) . '/test');
    }

    /**
     * List delivery attempts for a webhook. Returns a paginated envelope
     * `{ items, total, page, limit }`.
     *
     * @param array{page?: int, limit?: int, status?: string} $params
     * @return array<string, mixed>
     */
    public function listDeliveries(string $id, array $params = []): array
    {
        return $this->transport->request(
            'GET',
            '/v1/webhooks/' . rawurlencode($id) . '/deliveries' . Internal::query($params),
        );
    }

    /**
     * Fetch one delivery with its full payload and the endpoint's response.
     *
     * @return array<string, mixed>
     */
    public function getDelivery(string $id, string $deliveryId): array
    {
        return $this->transport->request(
            'GET',
            '/v1/webhooks/' . rawurlencode($id) . '/deliveries/' . rawurlencode($deliveryId),
        );
    }
}
