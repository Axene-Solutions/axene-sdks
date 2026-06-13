<?php

declare(strict_types=1);

namespace Axene\Mailer;

use Exception;
use ReflectionProperty;
use Throwable;

/**
 * Thrown for any non-2xx API response, or for a transport failure that
 * survives all retries.
 *
 * Inspect {@see AxeneException::getStatus()} and {@see AxeneException::getCode()}
 * to branch on specific failures (for example a 422 with code "invalid").
 *
 * Note: the inherited {@see Exception::getCode()} is final, so the API's
 * string error code is stored into the inherited `code` slot at construction.
 * That keeps the documented `getCode(): ?string` accessor working without
 * shadowing the final parent method.
 */
final class AxeneException extends Exception
{
    /** @param mixed $detail The raw parsed response body, for debugging. */
    public function __construct(
        private readonly int $status,
        string $message,
        ?string $code = null,
        private readonly mixed $detail = null,
        ?Throwable $previous = null,
    ) {
        parent::__construct($message, 0, $previous);

        // Exception::$code is loosely typed; place the string API code there so
        // the inherited final getCode() returns it.
        if ($code !== null) {
            $prop = new ReflectionProperty(Exception::class, 'code');
            $prop->setValue($this, $code);
        }
    }

    /**
     * HTTP status code. 0 indicates a transport/network failure (no response).
     */
    public function getStatus(): int
    {
        return $this->status;
    }

    /**
     * The raw parsed response body, for debugging.
     */
    public function getDetail(): mixed
    {
        return $this->detail;
    }
}
