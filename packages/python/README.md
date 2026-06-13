# axene-mailer (Python)

Official Python client for [Axene Mailer](https://axene.io). Send receipts,
confirmations, and campaigns from your own domain, priced in KES, billed via M-Pesa.

Pure standard library: no runtime dependencies. Python 3.8+.

## Install

```bash
pip install axene-mailer
```

## Usage

```python
from axene_mailer import Axene

axene = Axene(api_key="axm_k_your_api_key")

res = axene.emails.send({
    "from": {"email": "hello@yourdomain.com", "name": "Your Shop"},
    "to": "customer@example.com",
    "subject": "Your receipt",
    "html": "<p>Thanks for your order.</p>",
    "text": "Thanks for your order.",
})
print("queued", res["id"])
```

`from`, `to`, `cc`, `bcc` accept a string, a `{"email", "name"}` dict, or a list of either.

### More

```python
axene.emails.send_batch([{...}, {...}])      # bare array under the hood
axene.emails.get(res["id"])                   # status
axene.emails.validate({...})                  # dry-run: would this send?
axene.domains.list()                          # your sending domains

# Scheduling (Starter plan and up)
from datetime import datetime, timedelta, timezone
axene.emails.send({..., "send_at": datetime.now(timezone.utc) + timedelta(hours=1)})
```

### Errors and retries

Non-2xx responses raise `AxeneError` (`.status`, `.code`, `.args[0]` message).
The client retries 429 and 5xx with exponential backoff (configurable via
`max_retries`).

Get an API key at [mail.axene.io](https://mail.axene.io). Docs: <https://axene.io/docs/mailer/getting-started/welcome>.

MIT (c) Axene Solutions
