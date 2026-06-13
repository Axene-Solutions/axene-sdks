<?php

declare(strict_types=1);

namespace Axene\Mailer\Resources;

use Axene\Mailer\Internal;
use Axene\Mailer\Transport;

/**
 * The `suppressions` resource: manage the do-not-send list. Accessed as
 * `$client->suppressions`.
 */
final class Suppressions
{
    /** @internal */
    public function __construct(private readonly Transport $transport)
    {
    }

    /**
     * List suppressed addresses. Returns a paginated envelope
     * `{ items, total, page, limit }`; zero-based `page`.
     *
     * @param array{page?: int, limit?: int, search?: string} $params
     * @return array<string, mixed>
     */
    public function list(array $params = []): array
    {
        return $this->transport->request('GET', '/v1/suppressions' . Internal::query($params));
    }

    /**
     * Suppress a single address. The SDK's `email` maps to the wire field
     * `email_address`.
     *
     * @param array{email: string, reason?: string} $params
     * @return array<string, mixed>
     */
    public function add(array $params): array
    {
        return $this->transport->request('POST', '/v1/suppressions', [
            'email_address' => $params['email'],
            'reason' => $params['reason'] ?? 'manual',
        ]);
    }

    /**
     * Bulk-import suppressions from a file (one email per line). Sends the file
     * as multipart/form-data under the field name `file`.
     *
     * @param string $fileContents Raw bytes of the file.
     * @return array<string, mixed>
     */
    public function bulkUpload(string $fileContents, string $filename = 'suppressions.txt'): array
    {
        return $this->transport->upload('/v1/suppressions/bulk', $fileContents, $filename);
    }

    /**
     * Remove an address from the suppression list.
     */
    public function remove(string $id): void
    {
        $this->transport->request('DELETE', '/v1/suppressions/' . rawurlencode($id));
    }
}
