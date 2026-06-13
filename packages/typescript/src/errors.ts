/**
 * Error types raised by the SDK.
 * @module
 */

/**
 * Thrown for any non-2xx API response, or for a transport failure that
 * survives all retries.
 *
 * Inspect {@link AxeneError.status} and {@link AxeneError.code} to branch on
 * specific failures (for example a `422` with code `"invalid"`).
 */
export class AxeneError extends Error {
  /** HTTP status code. `0` indicates a transport/network failure (no response). */
  readonly status: number;
  /** Machine-readable error code from the API body, when present. */
  readonly code?: string;
  /** The raw parsed response body, for debugging. */
  readonly detail?: unknown;

  constructor(status: number, message: string, code?: string, detail?: unknown) {
    super(message);
    this.name = 'AxeneError';
    this.status = status;
    this.code = code;
    this.detail = detail;
    // Restore the prototype chain when targeting older runtimes.
    Object.setPrototypeOf(this, AxeneError.prototype);
  }
}
