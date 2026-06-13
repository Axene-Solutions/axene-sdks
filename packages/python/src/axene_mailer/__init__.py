"""Axene Mailer SDK for Python.

Professional email for Africa: send receipts, confirmations, and campaigns from
your own domain. Priced in KES, billed via M-Pesa.

    from axene_mailer import Axene

    axene = Axene(api_key="axm_k_your_api_key")
    axene.emails.send({
        "from": "hello@yourdomain.com",
        "to": "customer@example.com",
        "subject": "Your receipt",
        "html": "<p>Thanks for your order.</p>",
    })
"""

from .client import Axene
from .errors import AxeneError

__all__ = ["Axene", "AxeneError"]
__version__ = "0.1.0"
