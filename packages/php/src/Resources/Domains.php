<?php

declare(strict_types=1);

namespace Axene\Mailer\Resources;

use Axene\Mailer\Internal;
use Axene\Mailer\Transport;

/**
 * The `domains` resource: register, verify, inspect, and transfer sending
 * domains. Accessed as `$client->domains`.
 */
final class Domains
{
    /** @internal */
    public function __construct(private readonly Transport $transport)
    {
    }

    /**
     * List your sending domains and their verification status.
     *
     * @return list<array<string, mixed>>
     */
    public function list(): array
    {
        return $this->transport->request('GET', '/v1/domains/');
    }

    /**
     * Register a new sending domain. Returns the DNS records to publish.
     *
     * @return array<string, mixed>
     */
    public function create(string $name): array
    {
        return $this->transport->request('POST', '/v1/domains/', ['name' => $name]);
    }

    /**
     * Fetch a domain with its DKIM selector and DNS records.
     *
     * @return array<string, mixed>
     */
    public function get(string $id): array
    {
        return $this->transport->request('GET', '/v1/domains/' . rawurlencode($id));
    }

    /**
     * Delete a domain.
     */
    public function delete(string $id): void
    {
        $this->transport->request('DELETE', '/v1/domains/' . rawurlencode($id));
    }

    /**
     * Re-check DNS and verify the domain.
     *
     * @return array<string, mixed>
     */
    public function verify(string $id): array
    {
        return $this->transport->request('POST', '/v1/domains/' . rawurlencode($id) . '/verify');
    }

    /**
     * Run live DNS health checks (DKIM, SPF, DMARC, return-path, MX).
     *
     * @return array<string, mixed>
     */
    public function health(string $id): array
    {
        return $this->transport->request('GET', '/v1/domains/' . rawurlencode($id) . '/health');
    }

    /**
     * Diagnose configuration issues and get a health score.
     *
     * @return array<string, mixed>
     */
    public function diagnose(string $id): array
    {
        return $this->transport->request('GET', '/v1/domains/' . rawurlencode($id) . '/diagnose');
    }

    /**
     * Current MX status for inbound/forwarding (shape varies by provider).
     *
     * @return array<string, mixed>
     */
    public function mxStatus(string $id): array
    {
        return $this->transport->request('GET', '/v1/domains/' . rawurlencode($id) . '/mx-status');
    }

    /**
     * The values currently published in DNS for each of the domain's records.
     *
     * @return array<string, mixed>
     */
    public function publishedRecords(string $id): array
    {
        return $this->transport->request('GET', '/v1/domains/' . rawurlencode($id) . '/published-records');
    }

    /**
     * Rotate the domain's DKIM key, returning the new record to publish.
     *
     * @return array<string, mixed>
     */
    public function rotateDkim(string $id): array
    {
        return $this->transport->request('POST', '/v1/domains/' . rawurlencode($id) . '/rotate-dkim');
    }

    /**
     * Initiate a transfer of this domain to another Axene account.
     *
     * @param array{targetEmail: string, note?: string|null} $params
     * @return array<string, mixed>
     */
    public function transfer(string $id, array $params): array
    {
        return $this->transport->request('POST', '/v1/domains/' . rawurlencode($id) . '/transfer', [
            'target_email' => $params['targetEmail'],
            'note' => $params['note'] ?? null,
        ]);
    }

    /**
     * Check whether a domain name is available to add (checks public DNS).
     *
     * @return array<string, mixed>
     */
    public function checkAvailability(string $name): array
    {
        return $this->transport->request('GET', '/v1/domains/check-availability' . Internal::query(['name' => $name]));
    }

    /**
     * Check whether a domain name already exists in your account.
     *
     * @return array<string, mixed>
     */
    public function check(string $name): array
    {
        return $this->transport->request('GET', '/v1/domains/check/' . rawurlencode($name));
    }
}
