"""The ``domains`` resource: register, verify, inspect, and transfer sending
domains."""

from __future__ import annotations

from typing import Any, Dict, List, Optional

from .._http import HttpTransport
from .._serialize import query


class Domains:
    """Accessed as ``axene.domains``."""

    def __init__(self, http: HttpTransport) -> None:
        self._http = http

    def list(self) -> List[Dict[str, Any]]:
        """List your sending domains and their verification status."""
        return self._http.request("GET", "/v1/domains/")

    def create(self, name: str) -> Dict[str, Any]:
        """Register a new sending domain. Returns the DNS records to publish."""
        return self._http.request("POST", "/v1/domains/", {"name": name})

    def get(self, domain_id: str) -> Dict[str, Any]:
        """Fetch a domain with its DKIM selector and DNS records."""
        return self._http.request("GET", f"/v1/domains/{domain_id}")

    def delete(self, domain_id: str) -> None:
        """Delete a domain."""
        return self._http.request("DELETE", f"/v1/domains/{domain_id}")

    def verify(self, domain_id: str) -> Dict[str, Any]:
        """Re-check DNS and verify the domain."""
        return self._http.request("POST", f"/v1/domains/{domain_id}/verify")

    def health(self, domain_id: str) -> Dict[str, Any]:
        """Run live DNS health checks (DKIM, SPF, DMARC, return-path, MX)."""
        return self._http.request("GET", f"/v1/domains/{domain_id}/health")

    def diagnose(self, domain_id: str) -> Dict[str, Any]:
        """Diagnose configuration issues and get a health score."""
        return self._http.request("GET", f"/v1/domains/{domain_id}/diagnose")

    def mx_status(self, domain_id: str) -> Dict[str, Any]:
        """Current MX status for inbound/forwarding (shape varies by provider)."""
        return self._http.request("GET", f"/v1/domains/{domain_id}/mx-status")

    def published_records(self, domain_id: str) -> Dict[str, Any]:
        """The values currently published in DNS for each of the domain's records."""
        return self._http.request("GET", f"/v1/domains/{domain_id}/published-records")

    def rotate_dkim(self, domain_id: str) -> Dict[str, Any]:
        """Rotate the domain's DKIM key, returning the new record to publish."""
        return self._http.request("POST", f"/v1/domains/{domain_id}/rotate-dkim")

    def transfer(
        self,
        domain_id: str,
        target_email: str,
        note: Optional[str] = None,
    ) -> Dict[str, Any]:
        """Initiate a transfer of this domain to another Axene account."""
        return self._http.request(
            "POST",
            f"/v1/domains/{domain_id}/transfer",
            {"target_email": target_email, "note": note},
        )

    def check_availability(self, name: str) -> Dict[str, Any]:
        """Check whether a domain name is available to add (checks public DNS)."""
        return self._http.request("GET", f"/v1/domains/check-availability{query({'name': name})}")

    def check(self, name: str) -> Dict[str, Any]:
        """Check whether a domain name already exists in your account."""
        return self._http.request("GET", f"/v1/domains/check/{name}")
