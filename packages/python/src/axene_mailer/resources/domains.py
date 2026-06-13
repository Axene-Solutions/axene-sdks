"""The ``domains`` resource: inspect your sending domains."""

from __future__ import annotations

from typing import Any, Dict, List

from .._http import HttpTransport


class Domains:
    """Accessed as ``axene.domains``."""

    def __init__(self, http: HttpTransport) -> None:
        self._http = http

    def list(self) -> List[Dict[str, Any]]:
        """List your sending domains and their verification status."""
        return self._http.request("GET", "/v1/domains/")
