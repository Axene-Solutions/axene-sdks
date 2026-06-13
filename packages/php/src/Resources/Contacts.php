<?php

declare(strict_types=1);

namespace Axene\Mailer\Resources;

use Axene\Mailer\Internal;
use Axene\Mailer\Transport;

/**
 * The `contacts` resource: manage subscriber lists, their contacts, CSV
 * imports, and templated bulk sends. Accessed as `$client->contacts`.
 */
final class Contacts
{
    /** @internal */
    public function __construct(private readonly Transport $transport)
    {
    }

    /**
     * List all subscriber lists in the active workspace.
     *
     * @return list<array<string, mixed>>
     */
    public function listLists(): array
    {
        return $this->transport->request('GET', '/v1/contacts/');
    }

    /**
     * Create a subscriber list.
     *
     * @param array{name: string, description?: string|null, iconSeed?: string|null} $params
     * @return array<string, mixed>
     */
    public function createList(array $params): array
    {
        return $this->transport->request('POST', '/v1/contacts/', Internal::prune([
            'name' => $params['name'] ?? null,
            'description' => $params['description'] ?? null,
            'icon_seed' => $params['iconSeed'] ?? null,
        ]));
    }

    /**
     * Get a list with a page of its contacts (zero-based `page`).
     *
     * @param array{page?: int, limit?: int} $params
     * @return array<string, mixed>
     */
    public function getList(string $id, array $params = []): array
    {
        return $this->transport->request('GET', '/v1/contacts/' . rawurlencode($id) . Internal::query($params));
    }

    /**
     * Update a list's name, description, or icon (partial).
     *
     * @param array{name?: string, description?: string|null, iconSeed?: string|null} $params
     * @return array<string, mixed>
     */
    public function updateList(string $id, array $params): array
    {
        return $this->transport->request('PATCH', '/v1/contacts/' . rawurlencode($id), Internal::prune([
            'name' => $params['name'] ?? null,
            'description' => $params['description'] ?? null,
            'icon_seed' => $params['iconSeed'] ?? null,
        ]));
    }

    /**
     * Delete a list and all of its contacts.
     */
    public function deleteList(string $id): void
    {
        $this->transport->request('DELETE', '/v1/contacts/' . rawurlencode($id));
    }

    /**
     * Add a single contact to a list.
     *
     * @param array{email: string, name?: string|null, metadata?: array<string, mixed>|null} $params
     * @return array<string, mixed>
     */
    public function addContact(string $listId, array $params): array
    {
        return $this->transport->request(
            'POST',
            '/v1/contacts/' . rawurlencode($listId) . '/contacts',
            Internal::prune([
                'email' => $params['email'] ?? null,
                'name' => $params['name'] ?? null,
                'metadata' => $params['metadata'] ?? null,
            ]),
        );
    }

    /**
     * Remove a contact from a list.
     */
    public function removeContact(string $listId, string $contactId): void
    {
        $this->transport->request(
            'DELETE',
            '/v1/contacts/' . rawurlencode($listId) . '/contacts/' . rawurlencode($contactId),
        );
    }

    /**
     * Import contacts from a CSV file (header row required). Sends the file as
     * multipart/form-data under the field name `file`.
     *
     * @param string $fileContents Raw bytes of the CSV file.
     * @return array<string, mixed>
     */
    public function uploadCsv(string $listId, string $fileContents, string $filename = 'contacts.csv'): array
    {
        return $this->transport->upload(
            '/v1/contacts/' . rawurlencode($listId) . '/upload',
            $fileContents,
            $filename,
        );
    }

    /**
     * Send a templated email to every contact in a list. The list id is
     * injected as `contact_list_id` automatically. Subject/html/text may use
     * `{{email}}`, `{{name}}`, and `{{metadata_key}}` placeholders.
     *
     * @param array{senderAddressId: string, subject: string, html?: string|null, text?: string|null, tags?: list<string>|null} $params
     * @return array<string, mixed>
     */
    public function bulkSend(string $listId, array $params): array
    {
        return $this->transport->request(
            'POST',
            '/v1/contacts/' . rawurlencode($listId) . '/send',
            Internal::prune([
                'contact_list_id' => $listId,
                'sender_address_id' => $params['senderAddressId'] ?? null,
                'subject' => $params['subject'] ?? null,
                'html' => $params['html'] ?? null,
                'text' => $params['text'] ?? null,
                'tags' => $params['tags'] ?? null,
            ]),
        );
    }
}
