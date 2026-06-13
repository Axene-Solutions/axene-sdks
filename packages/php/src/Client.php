<?php

declare(strict_types=1);

namespace Axene\Mailer;

use Axene\Mailer\Resources\Contacts;
use Axene\Mailer\Resources\Domains;
use Axene\Mailer\Resources\Emails;
use Axene\Mailer\Resources\Suppressions;
use Axene\Mailer\Resources\Templates;
use Axene\Mailer\Resources\Webhooks;

/**
 * Axene Mailer API client. Composes the HTTP transport with the resource
 * groups; this is the entry point most code touches.
 *
 * @example
 * ```php
 * use Axene\Mailer\Client;
 *
 * $axene = new Client(getenv('AXENE_API_KEY'));
 * $axene->emails->send([
 *     'from' => 'hello@yourdomain.com',
 *     'to' => 'customer@example.com',
 *     'subject' => 'Your receipt',
 *     'html' => '<p>Thanks for your order.</p>',
 * ]);
 * ```
 */
final class Client
{
    /** Send, search, schedule, and inspect emails. */
    public readonly Emails $emails;

    /** Register, verify, and transfer sending domains. */
    public readonly Domains $domains;

    /** Manage subscriber lists and bulk sends. */
    public readonly Contacts $contacts;

    /** Manage the do-not-send suppression list. */
    public readonly Suppressions $suppressions;

    /** Manage reusable email templates. */
    public readonly Templates $templates;

    /** Manage event webhooks and inspect deliveries. */
    public readonly Webhooks $webhooks;

    /**
     * @param string $apiKey Your API key (required; starts with `axm_k_`).
     * @param array{
     *     baseUrl?: string,
     *     maxRetries?: int,
     *     timeout?: float|int,
     *     http?: \GuzzleHttp\ClientInterface
     * } $options Optional: baseUrl (default https://mail.axene.io), maxRetries
     *     (default 3), timeout in seconds (default 30), and a custom Guzzle
     *     client (mainly for testing).
     */
    public function __construct(string $apiKey, array $options = [])
    {
        $transport = new Transport($apiKey, $options);

        $this->emails = new Emails($transport);
        $this->domains = new Domains($transport);
        $this->contacts = new Contacts($transport);
        $this->suppressions = new Suppressions($transport);
        $this->templates = new Templates($transport);
        $this->webhooks = new Webhooks($transport);
    }
}
