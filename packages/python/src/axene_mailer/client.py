"""The Axene client: composes the HTTP transport with the resource groups."""

from __future__ import annotations

from typing import Optional

from ._http import HttpTransport
from .resources.contacts import Contacts
from .resources.domains import Domains
from .resources.emails import Emails
from .resources.suppressions import Suppressions
from .resources.templates import Templates
from .resources.webhooks import Webhooks


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
        #: Register, verify, and transfer sending domains.
        self.domains = Domains(http)
        #: Manage subscriber lists, contacts, CSV imports, and bulk sends.
        self.contacts = Contacts(http)
        #: Manage the do-not-send suppression list.
        self.suppressions = Suppressions(http)
        #: Manage reusable email templates.
        self.templates = Templates(http)
        #: Manage webhook subscriptions and inspect deliveries.
        self.webhooks = Webhooks(http)
