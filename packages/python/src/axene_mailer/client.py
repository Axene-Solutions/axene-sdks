"""The Axene client: composes the HTTP transport with the resource groups."""

from __future__ import annotations

from typing import Optional

from ._http import HttpTransport
from .resources.domains import Domains
from .resources.emails import Emails


class Axene:
    """Axene Mailer API client.

    Example::

        from axene_mailer import Axene

        axene = Axene(api_key="axm_k_your_api_key")
        axene.emails.send({
            "from": "hello@yourdomain.com",
            "to": "customer@example.com",
            "subject": "Your receipt",
            "html": "<p>Thanks for your order.</p>",
        })
    """

    def __init__(
        self,
        api_key: str,
        base_url: Optional[str] = None,
        max_retries: int = 3,
        timeout: float = 30.0,
    ) -> None:
        http = HttpTransport(api_key, base_url=base_url, max_retries=max_retries, timeout=timeout)
        #: Send and inspect emails.
        self.emails = Emails(http)
        #: Inspect your sending domains.
        self.domains = Domains(http)
