<?php

declare(strict_types=1);

namespace Axene\Mailer\Resources;

use Axene\Mailer\Internal;
use Axene\Mailer\Transport;

/**
 * The `templates` resource: reusable email templates (Starter plan and up).
 * Accessed as `$client->templates`.
 *
 * The SDK's `html` and `text` map to the wire fields `html_body` and
 * `text_body` (templates only; emails keep `html`/`text`).
 */
final class Templates
{
    /** @internal */
    public function __construct(private readonly Transport $transport)
    {
    }

    /**
     * List all templates, most recently updated first.
     *
     * @return list<array<string, mixed>>
     */
    public function list(): array
    {
        return $this->transport->request('GET', '/v1/templates/');
    }

    /**
     * Create a template. `variables` are derived server-side from `{{name}}`
     * placeholders in the bodies, so you do not pass them.
     *
     * @param array{name: string, subject?: string|null, html?: string|null, text?: string|null, blocksJson?: array<string, mixed>|null} $params
     * @return array<string, mixed>
     */
    public function create(array $params): array
    {
        return $this->transport->request('POST', '/v1/templates/', Internal::prune([
            'name' => $params['name'] ?? null,
            'subject' => $params['subject'] ?? null,
            'html_body' => $params['html'] ?? null,
            'text_body' => $params['text'] ?? null,
            'blocks_json' => $params['blocksJson'] ?? null,
        ]));
    }

    /**
     * Fetch a single template.
     *
     * @return array<string, mixed>
     */
    public function get(string $id): array
    {
        return $this->transport->request('GET', '/v1/templates/' . rawurlencode($id));
    }

    /**
     * Update a template (partial).
     *
     * @param array{name?: string, subject?: string|null, html?: string|null, text?: string|null, blocksJson?: array<string, mixed>|null} $params
     * @return array<string, mixed>
     */
    public function update(string $id, array $params): array
    {
        return $this->transport->request('PATCH', '/v1/templates/' . rawurlencode($id), Internal::prune([
            'name' => $params['name'] ?? null,
            'subject' => $params['subject'] ?? null,
            'html_body' => $params['html'] ?? null,
            'text_body' => $params['text'] ?? null,
            'blocks_json' => $params['blocksJson'] ?? null,
        ]));
    }

    /**
     * Delete a template.
     */
    public function delete(string $id): void
    {
        $this->transport->request('DELETE', '/v1/templates/' . rawurlencode($id));
    }

    /**
     * Duplicate a template (the copy's `blocks_json` is not carried over).
     *
     * @return array<string, mixed>
     */
    public function duplicate(string $id): array
    {
        return $this->transport->request('POST', '/v1/templates/' . rawurlencode($id) . '/duplicate');
    }
}
