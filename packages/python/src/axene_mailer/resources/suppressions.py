"""The ``suppressions`` resource: manage the do-not-send list."""

from __future__ import annotations

from typing import Any, Dict, Optional

from .._http import HttpTransport
from .._serialize import query


class Suppressions:
    """Accessed as ``axene.suppressions``."""

    def __init__(self, http: HttpTransport) -> None:
        self._http = http

    def list(
        self,
        page: int = 0,
        limit: int = 50,
        search: Optional[str] = None,
    ) -> Dict[str, Any]:
        """List suppressed addresses.

        Returns a paginated envelope ``{items, total, page, limit}`` (zero-based
        ``page``).
        """
        params = {"page": page, "limit": limit, "search": search}
        return self._http.request("GET", f"/v1/suppressions{query(params)}")

    def add(self, email: str, reason: str = "manual") -> Dict[str, Any]:
        """Suppress a single address.

        The address maps to the wire field ``email_address``.
        """
        return self._http.request(
            "POST",
            "/v1/suppressions",
            {"email_address": email, "reason": reason},
        )

    def bulk_upload(
        self,
        file_bytes: bytes,
        filename: str = "suppressions.txt",
    ) -> Dict[str, Any]:
        """Bulk-import suppressions from a file (one email per line).

        Sent as ``multipart/form-data`` under the field ``file``.
        """
        return self._http.upload("/v1/suppressions/bulk", file_bytes, filename)

    def remove(self, suppression_id: str) -> None:
        """Remove an address from the suppression list."""
        return self._http.request("DELETE", f"/v1/suppressions/{suppression_id}")
