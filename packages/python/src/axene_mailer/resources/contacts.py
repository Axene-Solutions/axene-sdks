"""The ``contacts`` resource: manage subscriber lists, their contacts, CSV
imports, and templated bulk sends."""

from __future__ import annotations

from typing import Any, Dict, List, Optional

from .._http import HttpTransport
from .._serialize import prune, query


class Contacts:
    """Accessed as ``axene.contacts``."""

    def __init__(self, http: HttpTransport) -> None:
        self._http = http

    def list_lists(self) -> List[Dict[str, Any]]:
        """List all subscriber lists in the active workspace."""
        return self._http.request("GET", "/v1/contacts/")

    def create_list(
        self,
        name: str,
        description: Optional[str] = None,
        icon_seed: Optional[str] = None,
    ) -> Dict[str, Any]:
        """Create a subscriber list."""
        body = prune({"name": name, "description": description, "icon_seed": icon_seed})
        return self._http.request("POST", "/v1/contacts/", body)

    def get_list(self, list_id: str, page: int = 0, limit: int = 50) -> Dict[str, Any]:
        """Get a list with a page of its contacts (zero-based ``page``)."""
        return self._http.request("GET", f"/v1/contacts/{list_id}{query({'page': page, 'limit': limit})}")

    def update_list(
        self,
        list_id: str,
        name: Optional[str] = None,
        description: Optional[str] = None,
        icon_seed: Optional[str] = None,
    ) -> Dict[str, Any]:
        """Update a list's name, description, or icon (partial)."""
        body = prune({"name": name, "description": description, "icon_seed": icon_seed})
        return self._http.request("PATCH", f"/v1/contacts/{list_id}", body)

    def delete_list(self, list_id: str) -> None:
        """Delete a list and all of its contacts."""
        return self._http.request("DELETE", f"/v1/contacts/{list_id}")

    def add_contact(
        self,
        list_id: str,
        email: str,
        name: Optional[str] = None,
        metadata: Optional[Dict[str, Any]] = None,
    ) -> Dict[str, Any]:
        """Add a single contact to a list."""
        body = prune({"email": email, "name": name, "metadata": metadata})
        return self._http.request("POST", f"/v1/contacts/{list_id}/contacts", body)

    def remove_contact(self, list_id: str, contact_id: str) -> None:
        """Remove a contact from a list."""
        return self._http.request("DELETE", f"/v1/contacts/{list_id}/contacts/{contact_id}")

    def upload_csv(
        self,
        list_id: str,
        file_bytes: bytes,
        filename: str = "contacts.csv",
    ) -> Dict[str, Any]:
        """Import contacts from a CSV file (header row required).

        The email column is auto-detected; other columns become contact
        metadata. Sent as ``multipart/form-data`` under the field ``file``.
        """
        return self._http.upload(f"/v1/contacts/{list_id}/upload", file_bytes, filename)

    def bulk_send(
        self,
        list_id: str,
        sender_address_id: str,
        subject: str,
        html: Optional[str] = None,
        text: Optional[str] = None,
        tags: Optional[List[str]] = None,
    ) -> Dict[str, Any]:
        """Send a templated email to every contact in a list.

        ``contact_list_id`` is injected automatically from ``list_id``.
        Subject/html/text may use ``{{email}}``, ``{{name}}``, and
        ``{{metadata_key}}`` placeholders.
        """
        body = prune(
            {
                "contact_list_id": list_id,
                "sender_address_id": sender_address_id,
                "subject": subject,
                "html": html,
                "text": text,
                "tags": tags,
            }
        )
        return self._http.request("POST", f"/v1/contacts/{list_id}/send", body)
