"""Error types raised by the SDK."""

from __future__ import annotations

from typing import Any, Optional


class AxeneError(Exception):
    """Raised for any non-2xx API response, or a transport failure that
    survives all retries.

    Inspect :attr:`status` and :attr:`code` to branch on specific failures
    (for example a ``422`` with code ``"invalid"``).
    """

    def __init__(self, status: int, message: str, code: Optional[str] = None, detail: Any = None) -> None:
        super().__init__(message)
        #: HTTP status code. ``0`` indicates a transport/network failure.
        self.status = status
        #: Machine-readable error code from the API body, when present.
        self.code = code
        #: The raw parsed response body, for debugging.
        self.detail = detail
