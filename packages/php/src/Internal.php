<?php

declare(strict_types=1);

namespace Axene\Mailer;

/**
 * Internal helpers that translate the SDK's ergonomic input into the exact
 * JSON shape the API expects, plus query-string building. Not part of the
 * public API.
 *
 * @internal
 */
final class Internal
{
    /**
     * Normalize a single address. A bare string becomes `['email' => $value]`;
     * an associative array is passed through.
     *
     * @param string|array<string, mixed> $address
     * @return array<string, mixed>
     */
    public static function toAddress(string|array $address): array
    {
        return is_string($address) ? ['email' => $address] : $address;
    }

    /**
     * Normalize one-or-many addresses into a list, or null if absent.
     *
     * @param string|array<mixed>|null $address
     * @return list<array<string, mixed>>|null
     */
    public static function toAddressList(string|array|null $address): ?array
    {
        if ($address === null) {
            return null;
        }

        // A list (sequential array) of addresses, or a single address.
        $items = (is_array($address) && array_is_list($address)) ? $address : [$address];

        return array_map(static fn ($a) => self::toAddress($a), $items);
    }

    /**
     * Drop keys whose value is null so they are omitted from the JSON body.
     *
     * @param array<string, mixed> $body
     * @return array<string, mixed>
     */
    public static function prune(array $body): array
    {
        return array_filter($body, static fn ($v) => $v !== null);
    }

    /**
     * Build the JSON body for a send / validate request, honouring the
     * `from` -> `from_` wire mapping and address sugar. Drops null keys.
     *
     * @param array<string, mixed> $params
     * @return array<string, mixed>
     */
    public static function serializeSend(array $params): array
    {
        return self::prune([
            'from_' => isset($params['from']) ? self::toAddress($params['from']) : null,
            'to' => self::toAddressList($params['to'] ?? null),
            'subject' => $params['subject'] ?? null,
            'html' => $params['html'] ?? null,
            'text' => $params['text'] ?? null,
            'cc' => self::toAddressList($params['cc'] ?? null),
            'bcc' => self::toAddressList($params['bcc'] ?? null),
            'reply_to' => isset($params['replyTo']) ? self::toAddress($params['replyTo']) : null,
            'headers' => $params['headers'] ?? null,
            'tags' => $params['tags'] ?? null,
            'send_at' => $params['sendAt'] ?? null,
            'attachments' => $params['attachments'] ?? null,
        ]);
    }

    /**
     * Build a URL query string, skipping null values. Returns "" when nothing
     * is set, or "?a=1&b=2" otherwise.
     *
     * @param array<string, mixed> $params
     */
    public static function query(array $params): string
    {
        $filtered = array_filter($params, static fn ($v) => $v !== null);
        if ($filtered === []) {
            return '';
        }

        // Cast booleans to "true"/"false" rather than "1"/"".
        foreach ($filtered as $k => $v) {
            if (is_bool($v)) {
                $filtered[$k] = $v ? 'true' : 'false';
            }
        }

        return '?' . http_build_query($filtered);
    }
}
