"""The ``emails`` resource: send, look up, search, schedule, and inspect
messages."""

from __future__ import annotations

from datetime import datetime
from typing import Any, Dict, List, Optional, Union

from .._http import HttpTransport
from .._serialize import _iso, query, serialize_send


class Emails:
    """Accessed as ``axene.emails``."""

    def __init__(self, http: HttpTransport) -> None:
        self._http = http

    def send(self, message: Dict[str, Any]) -> Dict[str, Any]:
        """Send a single email.

        ``message`` keys: ``from`` (required), ``to`` (required), ``subject``
        (required), and optionally ``html``, ``text``, ``cc``, ``bcc``,
        ``reply_to``, ``headers``, ``tags``, ``send_at`` (``datetime`` or ISO
        string), ``attachments``. ``from``/``to``/``cc``/``bcc`` accept a
        string, a ``{"email", "name"}`` dict, or a list.
        """
        return self._http.request("POST", "/v1/emails/", serialize_send(message))

    def send_batch(self, messages: List[Dict[str, Any]]) -> Dict[str, Any]:
        """Send up to your plan's batch limit. The API accepts a bare array."""
        return self._http.request("POST", "/v1/emails/batch", [serialize_send(m) for m in messages])

    def validate(self, message: Dict[str, Any]) -> Dict[str, Any]:
        """Dry-run a send: check whether ``message`` would be accepted (sender
        registered, domain verified, plan limits, account not restricted)
        without actually sending it. Returns ``valid``, ``can_send``,
        ``issues``, ``plan`` and ``usage``.
        """
        return self._http.request("POST", "/v1/emails/validate", serialize_send(message))

    def list(
        self,
        status: Optional[str] = None,
        page: int = 0,
        limit: int = 20,
    ) -> List[Dict[str, Any]]:
        """List recent emails, newest first (zero-based ``page``)."""
        params = {"status": status, "page": page, "limit": limit}
        return self._http.request("GET", f"/v1/emails/{query(params)}")

    def get(self, email_id: str) -> Dict[str, Any]:
        """Fetch a single email with its bodies and events."""
        return self._http.request("GET", f"/v1/emails/{email_id}")

    def events(self, email_id: str) -> List[Dict[str, Any]]:
        """List delivery / open / click / bounce events for an email."""
        return self._http.request("GET", f"/v1/emails/{email_id}/events")

    def retry(self, email_id: str) -> Dict[str, Any]:
        """Re-send a bounced, rejected, or failed email as a new message."""
        return self._http.request("POST", f"/v1/emails/{email_id}/retry")

    def search(
        self,
        q: Optional[str] = None,
        status: Optional[str] = None,
        tag: Optional[str] = None,
        page: int = 0,
        limit: int = 20,
    ) -> List[Dict[str, Any]]:
        """Search emails.

        ``q`` supports inline tokens (``to:``, ``from:``, ``status:``,
        ``domain:``, ``tag:``); leftover words are matched as free text.
        """
        params = {"q": q, "status": status, "tag": tag, "page": page, "limit": limit}
        return self._http.request("GET", f"/v1/emails/search{query(params)}")

    def list_scheduled(self) -> List[Dict[str, Any]]:
        """List emails scheduled for future delivery, soonest first."""
        return self._http.request("GET", "/v1/emails/scheduled")

    def cancel_scheduled(self, email_id: str) -> Dict[str, Any]:
        """Cancel a scheduled email."""
        return self._http.request("DELETE", f"/v1/emails/scheduled/{email_id}")

    def send_scheduled_now(self, email_id: str) -> Dict[str, Any]:
        """Send a scheduled email immediately instead of waiting."""
        return self._http.request("POST", f"/v1/emails/scheduled/{email_id}/send-now")

    def updates(self, since: Union[str, datetime]) -> List[Dict[str, Any]]:
        """Poll for emails whose status changed at or after ``since`` (a
        ``datetime`` or ISO 8601 string). Capped at 50 rows.
        """
        return self._http.request("GET", f"/v1/emails/updates{query({'since': _iso(since)})}")

    def get_saved_searches(self) -> List[Dict[str, Any]]:
        """Get the caller's saved searches."""
        result = self._http.request("GET", "/v1/emails/saved-searches")
        return result.get("searches", []) if isinstance(result, dict) else result

    def set_saved_searches(self, searches: List[Dict[str, Any]]) -> List[Dict[str, Any]]:
        """Replace the caller's saved searches (max 50)."""
        result = self._http.request("PUT", "/v1/emails/saved-searches", {"searches": searches})
        return result.get("searches", []) if isinstance(result, dict) else result
