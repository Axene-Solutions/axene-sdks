"""HTTP transport: the single place that talks to the network. Owns
authentication, JSON encoding, timeouts, retries with backoff, and turning
non-2xx responses into :class:`AxeneError`. Uses only the standard library so
the package has no runtime dependencies.
"""

from __future__ import annotations

import json
import time
import urllib.error
import urllib.request
from typing import Any, Optional

from .errors import AxeneError

_DEFAULT_BASE = "https://mail.axene.io"
_USER_AGENT = "axene-mailer-python/0.1.0"


class HttpTransport:
    """Performs authenticated JSON requests, retrying ``429`` and ``5xx``."""

    def __init__(
        self,
        api_key: str,
        base_url: Optional[str] = None,
        max_retries: int = 3,
        timeout: float = 30.0,
    ) -> None:
        if not api_key:
            raise ValueError("api_key is required")
        self._api_key = api_key
        self._base_url = (base_url or _DEFAULT_BASE).rstrip("/")
        self._max_retries = max(1, max_retries)
        self._timeout = timeout

    def request(self, method: str, path: str, body: Any = None) -> Any:
        """Send a request and return the parsed JSON response."""
        url = f"{self._base_url}{path}"
        data = None if body is None else json.dumps(body).encode("utf-8")
        last_error: Optional[Exception] = None

        for attempt in range(1, self._max_retries + 1):
            req = urllib.request.Request(url, data=data, method=method)
            req.add_header("Authorization", f"Bearer {self._api_key}")
            req.add_header("Content-Type", "application/json")
            req.add_header("User-Agent", _USER_AGENT)
            try:
                with urllib.request.urlopen(req, timeout=self._timeout) as resp:
                    raw = resp.read().decode("utf-8")
                    return json.loads(raw) if raw else None
            except urllib.error.HTTPError as e:  # the server responded with a non-2xx
                status = e.code
                if self._is_retryable(status) and attempt < self._max_retries:
                    time.sleep(self._backoff(e, attempt))
                    continue
                raise self._to_error(status, e.read().decode("utf-8", "replace"))
            except urllib.error.URLError as e:  # transport / DNS / timeout
                last_error = e
                if attempt < self._max_retries:
                    time.sleep(self._backoff(None, attempt))
                    continue

        raise AxeneError(0, f"Axene request failed: {last_error}")

    @staticmethod
    def _is_retryable(status: int) -> bool:
        return status == 429 or status >= 500

    @staticmethod
    def _backoff(err: Optional[urllib.error.HTTPError], attempt: int) -> float:
        if err is not None:
            retry_after = err.headers.get("Retry-After") if err.headers else None
            if retry_after and retry_after.isdigit():
                return float(retry_after)
        return 0.25 * (2 ** (attempt - 1))

    @staticmethod
    def _to_error(status: int, raw: str) -> AxeneError:
        """Map the API's ``{"detail": {"code", "message"}}`` (or string) body."""
        message = f"Axene request failed ({status})"
        code: Optional[str] = None
        payload: Any = None
        try:
            payload = json.loads(raw)
            detail = payload.get("detail") if isinstance(payload, dict) else None
            if isinstance(detail, dict):
                message = detail.get("message", message)
                code = detail.get("code")
            elif isinstance(detail, str):
                message = detail
        except (ValueError, AttributeError):
            pass  # non-JSON body: keep the generic message
        return AxeneError(status, message, code, payload)
