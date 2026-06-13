"""Internal helpers that translate ergonomic inputs into the exact JSON the
API expects. Not part of the public API."""

from __future__ import annotations

from datetime import datetime
from typing import Any, Dict, List, Optional, Union

Address = Union[str, Dict[str, Any]]


def _to_address(a: Address) -> Dict[str, Any]:
    """A bare string becomes ``{"email": ...}``."""
    return {"email": a} if isinstance(a, str) else a


def _to_address_list(a: Optional[Union[Address, List[Address]]]) -> Optional[List[Dict[str, Any]]]:
    if a is None:
        return None
    items = a if isinstance(a, list) else [a]
    return [_to_address(x) for x in items]


def _iso(value: Any) -> Optional[str]:
    if value is None:
        return None
    return value.isoformat() if isinstance(value, datetime) else value


def prune(o: Dict[str, Any]) -> Dict[str, Any]:
    """Drop keys whose value is ``None`` so they are omitted from the JSON body."""
    return {k: v for k, v in o.items() if v is not None}


def query(params: Dict[str, Any]) -> str:
    """Build a URL query string, skipping ``None`` values.

    Returns ``""`` when nothing is set, or ``"?a=1&b=2"`` otherwise. ``datetime``
    values are serialized to ISO 8601.
    """
    from urllib.parse import urlencode

    pairs = []
    for k, v in params.items():
        if v is None:
            continue
        pairs.append((k, _iso(v) if isinstance(v, datetime) else str(v)))
    encoded = urlencode(pairs)
    return f"?{encoded}" if encoded else ""


def serialize_send(p: Dict[str, Any]) -> Dict[str, Any]:
    """Build the JSON body for a send.

    The API names the sender field ``from_`` on the wire; callers pass a clean
    ``"from"`` key, so the mapping happens here. Keys with ``None`` values are
    omitted.
    """
    body = {
        "from_": _to_address(p["from"]),
        "to": _to_address_list(p["to"]),
        "subject": p["subject"],
        "html": p.get("html"),
        "text": p.get("text"),
        "cc": _to_address_list(p.get("cc")),
        "bcc": _to_address_list(p.get("bcc")),
        "reply_to": _to_address(p["reply_to"]) if p.get("reply_to") else None,
        "headers": p.get("headers"),
        "tags": p.get("tags"),
        "send_at": _iso(p.get("send_at")),
        "attachments": p.get("attachments"),
    }
    return {k: v for k, v in body.items() if v is not None}
