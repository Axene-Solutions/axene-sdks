"""The ``templates`` resource: reusable email templates. Starter plan and up."""

from __future__ import annotations

from typing import Any, Dict, List, Optional

from .._http import HttpTransport
from .._serialize import prune


class Templates:
    """Accessed as ``axene.templates``."""

    def __init__(self, http: HttpTransport) -> None:
        self._http = http

    def list(self) -> List[Dict[str, Any]]:
        """List all templates, most recently updated first."""
        return self._http.request("GET", "/v1/templates/")

    def create(
        self,
        name: str,
        subject: Optional[str] = None,
        html: Optional[str] = None,
        text: Optional[str] = None,
        blocks_json: Optional[Dict[str, Any]] = None,
    ) -> Dict[str, Any]:
        """Create a template.

        ``html`` maps to ``html_body`` and ``text`` to ``text_body`` on the
        wire. ``variables`` are derived server-side from ``{{name}}``
        placeholders, so you do not pass them.
        """
        body = prune(
            {
                "name": name,
                "subject": subject,
                "html_body": html,
                "text_body": text,
                "blocks_json": blocks_json,
            }
        )
        return self._http.request("POST", "/v1/templates/", body)

    def get(self, template_id: str) -> Dict[str, Any]:
        """Fetch a single template."""
        return self._http.request("GET", f"/v1/templates/{template_id}")

    def update(
        self,
        template_id: str,
        name: Optional[str] = None,
        subject: Optional[str] = None,
        html: Optional[str] = None,
        text: Optional[str] = None,
        blocks_json: Optional[Dict[str, Any]] = None,
    ) -> Dict[str, Any]:
        """Update a template (partial). ``html``/``text`` map to
        ``html_body``/``text_body``.
        """
        body = prune(
            {
                "name": name,
                "subject": subject,
                "html_body": html,
                "text_body": text,
                "blocks_json": blocks_json,
            }
        )
        return self._http.request("PATCH", f"/v1/templates/{template_id}", body)

    def delete(self, template_id: str) -> None:
        """Delete a template."""
        return self._http.request("DELETE", f"/v1/templates/{template_id}")

    def duplicate(self, template_id: str) -> Dict[str, Any]:
        """Duplicate a template (the copy's ``blocks_json`` is not carried over)."""
        return self._http.request("POST", f"/v1/templates/{template_id}/duplicate")
