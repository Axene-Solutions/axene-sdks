<?php

declare(strict_types=1);

namespace Axene\Mailer;

use GuzzleHttp\Client as GuzzleClient;
use GuzzleHttp\ClientInterface;
use GuzzleHttp\Exception\ConnectException;
use GuzzleHttp\Psr7\Request;
use GuzzleHttp\Psr7\Utils;
use Psr\Http\Message\ResponseInterface;
use Throwable;

/**
 * The single place that talks to the network. Owns bearer authentication,
 * JSON encode/decode, timeouts, retries on 429/5xx with backoff (honouring
 * Retry-After), multipart uploads (field `file`), and turning non-2xx
 * responses into {@see AxeneException}. Resources depend on this, not on
 * Guzzle directly.
 */
final class Transport
{
    private const DEFAULT_BASE = 'https://mail.axene.io';
    private const USER_AGENT = 'axene-mailer-php';

    private readonly string $apiKey;
    private readonly string $baseUrl;
    private readonly int $maxRetries;
    private readonly float $timeout;
    private readonly ClientInterface $http;

    /**
     * @param array{
     *     baseUrl?: string,
     *     maxRetries?: int,
     *     timeout?: float|int,
     *     http?: ClientInterface
     * } $options
     */
    public function __construct(string $apiKey, array $options = [])
    {
        if ($apiKey === '') {
            throw new \InvalidArgumentException('Axene: `apiKey` is required.');
        }

        $this->apiKey = $apiKey;
        $this->baseUrl = rtrim($options['baseUrl'] ?? self::DEFAULT_BASE, '/');
        $this->maxRetries = $options['maxRetries'] ?? 3;
        $this->timeout = (float) ($options['timeout'] ?? 30.0);
        $this->http = $options['http'] ?? new GuzzleClient();
    }

    /**
     * Perform a JSON request and return the decoded body (associative array)
     * or null for empty (204) responses.
     *
     * Retries 429 and 5xx with exponential backoff (honouring Retry-After when
     * present). Throws {@see AxeneException} on a final non-2xx or a transport
     * failure that survives all attempts.
     *
     * @param array<string, mixed>|list<mixed>|null $body
     * @return mixed
     */
    public function request(string $method, string $path, array|null $body = null): mixed
    {
        $headers = [
            'Authorization' => 'Bearer ' . $this->apiKey,
            'User-Agent' => self::USER_AGENT,
            'Accept' => 'application/json',
        ];

        $encoded = null;
        if ($body !== null) {
            $headers['Content-Type'] = 'application/json';
            $encoded = json_encode($body, JSON_THROW_ON_ERROR);
        }

        $request = new Request($method, $this->baseUrl . $path, $headers, $encoded);

        return $this->send($request, retryable: true);
    }

    /**
     * Upload a single file as multipart/form-data under the field name `file`.
     * Used by the CSV / suppression import endpoints. Not retried (uploads are
     * not idempotent).
     *
     * @return mixed
     */
    public function upload(string $path, string $contents, string $filename): mixed
    {
        $boundary = '----axene' . bin2hex(random_bytes(16));
        $body = "--{$boundary}\r\n"
            . 'Content-Disposition: form-data; name="file"; filename="' . $filename . "\"\r\n"
            . "Content-Type: application/octet-stream\r\n\r\n"
            . $contents . "\r\n"
            . "--{$boundary}--\r\n";

        $request = new Request(
            'POST',
            $this->baseUrl . $path,
            [
                'Authorization' => 'Bearer ' . $this->apiKey,
                'User-Agent' => self::USER_AGENT,
                'Accept' => 'application/json',
                'Content-Type' => 'multipart/form-data; boundary=' . $boundary,
            ],
            Utils::streamFor($body),
        );

        return $this->send($request, retryable: false);
    }

    /**
     * @return mixed
     */
    private function send(Request $request, bool $retryable): mixed
    {
        $maxAttempts = $retryable ? $this->maxRetries : 1;
        $lastError = null;

        for ($attempt = 1; $attempt <= $maxAttempts; $attempt++) {
            try {
                $response = $this->http->send($request, [
                    'http_errors' => false,
                    'timeout' => $this->timeout,
                ]);
            } catch (ConnectException $e) {
                // Transport failure: retry if attempts remain.
                $lastError = $e;
                if ($attempt < $maxAttempts) {
                    $this->sleep($this->backoffSeconds(null, $attempt));
                    continue;
                }
                break;
            }

            $status = $response->getStatusCode();

            if ($retryable && $this->isRetryable($status) && $attempt < $maxAttempts) {
                $this->sleep($this->backoffSeconds($response, $attempt));
                continue;
            }

            $payload = $this->parseBody($response);
            if ($status >= 200 && $status < 300) {
                return $payload;
            }

            throw $this->toError($status, $payload);
        }

        throw new AxeneException(0, 'Axene request failed: ' . $this->describe($lastError), null, null, $lastError);
    }

    private function isRetryable(int $status): bool
    {
        return $status === 429 || $status >= 500;
    }

    private function backoffSeconds(?ResponseInterface $response, int $attempt): float
    {
        if ($response !== null && $response->hasHeader('Retry-After')) {
            $retryAfter = (float) $response->getHeaderLine('Retry-After');
            if ($retryAfter > 0) {
                return $retryAfter;
            }
        }

        return 0.25 * (2 ** ($attempt - 1));
    }

    private function sleep(float $seconds): void
    {
        usleep((int) round($seconds * 1_000_000));
    }

    /**
     * @return mixed
     */
    private function parseBody(ResponseInterface $response): mixed
    {
        $contentType = $response->getHeaderLine('Content-Type');
        if (!str_contains($contentType, 'application/json')) {
            return null;
        }

        $raw = (string) $response->getBody();
        if ($raw === '') {
            return null;
        }

        try {
            return json_decode($raw, true, 512, JSON_THROW_ON_ERROR);
        } catch (Throwable) {
            return null;
        }
    }

    /**
     * Map the API's `{ detail: { code, message } }` (or string) into an
     * {@see AxeneException}.
     *
     * @param mixed $payload
     */
    private function toError(int $status, mixed $payload): AxeneException
    {
        $detail = is_array($payload) ? ($payload['detail'] ?? null) : null;

        $code = null;
        $message = null;
        if (is_array($detail)) {
            $code = isset($detail['code']) && is_string($detail['code']) ? $detail['code'] : null;
            $message = isset($detail['message']) && is_string($detail['message']) ? $detail['message'] : null;
        } elseif (is_string($detail)) {
            $message = $detail;
        }

        $message ??= "Axene request failed ({$status})";

        return new AxeneException($status, $message, $code, $payload);
    }

    private function describe(?Throwable $error): string
    {
        return $error?->getMessage() ?? 'unknown error';
    }
}
