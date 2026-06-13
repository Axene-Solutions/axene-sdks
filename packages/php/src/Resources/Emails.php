<?php

declare(strict_types=1);

namespace Axene\Mailer\Resources;

use Axene\Mailer\Internal;
use Axene\Mailer\Transport;

/**
 * The `emails` resource: send, look up, search, schedule, and inspect messages.
 * Accessed as `$client->emails`.
 */
final class Emails
{
    /** @internal */
    public function __construct(private readonly Transport $transport)
    {
    }

    /**
     * Send a single email.
     *
     * The `from`, `to`, `cc`, `bcc`, and `replyTo` fields accept either an
     * address array (`['email' => ..., 'name' => ...]`) or a bare string,
     * which is sugar for `['email' => $string]`.
     *
     * @param array<string, mixed> $params
     * @return array<string, mixed>
     */
    public function send(array $params): array
    {
        return $this->transport->request('POST', '/v1/emails/', Internal::serializeSend($params));
    }

    /**
     * Send up to your plan's batch limit in one call. The API accepts a bare
     * array of messages and returns a per-message result set.
     *
     * @param list<array<string, mixed>> $messages
     * @return array<string, mixed>
     */
    public function sendBatch(array $messages): array
    {
        $body = array_map(static fn (array $m) => Internal::serializeSend($m), $messages);

        return $this->transport->request('POST', '/v1/emails/batch', $body);
    }

    /**
     * Dry-run a send: check whether `message` would be accepted (sender
     * registered, domain verified, plan limits) without actually sending it.
     *
     * @param array<string, mixed> $message
     * @return array<string, mixed>
     */
    public function validate(array $message): array
    {
        return $this->transport->request('POST', '/v1/emails/validate', Internal::serializeSend($message));
    }

    /**
     * List recent emails, newest first. Zero-based `page`.
     *
     * @param array{status?: string, page?: int, limit?: int} $params
     * @return list<array<string, mixed>>
     */
    public function list(array $params = []): array
    {
        return $this->transport->request('GET', '/v1/emails/' . Internal::query($params));
    }

    /**
     * Fetch a single email with its bodies and events.
     *
     * @return array<string, mixed>
     */
    public function get(string $id): array
    {
        return $this->transport->request('GET', '/v1/emails/' . rawurlencode($id));
    }

    /**
     * List delivery / open / click / bounce events for an email.
     *
     * @return list<array<string, mixed>>
     */
    public function events(string $id): array
    {
        return $this->transport->request('GET', '/v1/emails/' . rawurlencode($id) . '/events');
    }

    /**
     * Re-send a bounced, rejected, or failed email as a new message.
     *
     * @return array<string, mixed>
     */
    public function retry(string $id): array
    {
        return $this->transport->request('POST', '/v1/emails/' . rawurlencode($id) . '/retry');
    }

    /**
     * Search emails. `q` supports inline tokens (`to:`, `from:`, `status:`,
     * `domain:`, `tag:`); leftover words are matched as free text.
     *
     * @param array{q?: string, status?: string, tag?: string, page?: int, limit?: int} $params
     * @return list<array<string, mixed>>
     */
    public function search(array $params = []): array
    {
        return $this->transport->request('GET', '/v1/emails/search' . Internal::query($params));
    }

    /**
     * List emails scheduled for future delivery, soonest first.
     *
     * @return list<array<string, mixed>>
     */
    public function listScheduled(): array
    {
        return $this->transport->request('GET', '/v1/emails/scheduled');
    }

    /**
     * Cancel a scheduled email.
     *
     * @return array<string, mixed>
     */
    public function cancelScheduled(string $id): array
    {
        return $this->transport->request('DELETE', '/v1/emails/scheduled/' . rawurlencode($id));
    }

    /**
     * Send a scheduled email immediately instead of waiting.
     *
     * @return array<string, mixed>
     */
    public function sendScheduledNow(string $id): array
    {
        return $this->transport->request('POST', '/v1/emails/scheduled/' . rawurlencode($id) . '/send-now');
    }

    /**
     * Poll for emails whose status changed at or after `since` (ISO 8601).
     * Capped at 50 rows; use for live status updates.
     *
     * @return list<array<string, mixed>>
     */
    public function updates(string $since): array
    {
        return $this->transport->request('GET', '/v1/emails/updates' . Internal::query(['since' => $since]));
    }

    /**
     * Get the caller's saved searches.
     *
     * @return list<array<string, mixed>>
     */
    public function getSavedSearches(): array
    {
        $response = $this->transport->request('GET', '/v1/emails/saved-searches');

        return $response['searches'] ?? [];
    }

    /**
     * Replace the caller's saved searches (max 50).
     *
     * @param list<array<string, mixed>> $searches
     * @return list<array<string, mixed>>
     */
    public function setSavedSearches(array $searches): array
    {
        $response = $this->transport->request('PUT', '/v1/emails/saved-searches', ['searches' => $searches]);

        return $response['searches'] ?? [];
    }
}
