"""The ``webhooks`` resource: manage event subscriptions and inspect
deliveries."""

from __future__ import annotations

from typing import Any, Dict, List, Optional

from .._http import HttpTransport
from .._serialize import prune, query


class Webhooks:
    """Accessed as ``axene.webhooks``."""

    def __init__(self, http: HttpTransport) -> None:
        self._http = http

    def list(self) -> List[Dict[str, Any]]:
        """List your active webhooks."""
        return self._http.request("GET", "/v1/webhooks/")

    def create(self, url: str, events: List[str]) -> Dict[str, Any]:
        """Create a webhook. The signing ``secret`` is generated and returned."""
        return self._http.request("POST", "/v1/webhooks/", {"url": url, "events": events})

    def update(
        self,
        webhook_id: str,
        url: Optional[str] = None,
        events: Optional[List[str]] = None,
        is_active: Optional[bool] = None,
    ) -> Dict[str, Any]:
        """Update a webhook's url, events, or active state (partial).

        ``is_active`` maps to the wire field ``is_active``.
        """
        body = prune({"url": url, "events": events, "is_active": is_active})
        return self._http.request("PATCH", f"/v1/webhooks/{webhook_id}", body)

    def delete(self, webhook_id: str) -> None:
        """Delete a webhook."""
        return self._http.request("DELETE", f"/v1/webhooks/{webhook_id}")

    def test(self, webhook_id: str) -> Dict[str, Any]:
        """Queue a sample ``email.delivered`` delivery to test the endpoint."""
        return self._http.request("POST", f"/v1/webhooks/{webhook_id}/test")

    def list_deliveries(
        self,
        webhook_id: str,
        page: int = 0,
        limit: int = 20,
        status: Optional[str] = None,
    ) -> Dict[str, Any]:
        """List delivery attempts for a webhook (paginated envelope)."""
        params = {"page": page, "limit": limit, "status": status}
        return self._http.request("GET", f"/v1/webhooks/{webhook_id}/deliveries{query(params)}")

    def get_delivery(self, webhook_id: str, delivery_id: str) -> Dict[str, Any]:
        """Fetch one delivery with its full payload and the endpoint's response."""
        return self._http.request("GET", f"/v1/webhooks/{webhook_id}/deliveries/{delivery_id}")
