"""The ``emails`` resource: send, look up, and inspect messages."""

from __future__ import annotations

from typing import Any, Dict, List

from .._http import HttpTransport
from .._serialize import serialize_send


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

    def get(self, email_id: str) -> Dict[str, Any]:
        """Fetch a single email and its current status."""
        return self._http.request("GET", f"/v1/emails/{email_id}")

    def events(self, email_id: str) -> List[Dict[str, Any]]:
        """List delivery / open / click / bounce events for an email."""
        return self._http.request("GET", f"/v1/emails/{email_id}/events")

    def validate(self, message: Dict[str, Any]) -> Dict[str, Any]:
        """Dry-run a send: check whether ``message`` would be accepted (sender
        registered, domain verified, plan limits, account not restricted)
        without actually sending it. Returns ``valid``, ``can_send``,
        ``issues``, ``plan`` and ``usage``.
        """
        return self._http.request("POST", "/v1/emails/validate", serialize_send(message))
