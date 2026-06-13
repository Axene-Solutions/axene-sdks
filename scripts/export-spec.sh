#!/usr/bin/env bash
# Regenerate spec/openapi.json from the Axene Mailer backend, keeping only the
# public (API-key) surface. Run from the repo root.
set -euo pipefail
BACKEND="${1:-../axene-mailer/backend}"
"$BACKEND/venv/bin/python" - "$BACKEND" <<'PY'
import json, sys, os
sys.path.insert(0, sys.argv[1])
os.environ.setdefault("JWT_SECRET", "x"*64)
os.environ.setdefault("DATABASE_URL", "postgresql+asyncpg://u:p@localhost/db")
from app.main import app
spec = app.openapi()
DROP = ("/internal", "/admin", "/auth", "/oauth", "/v1/admin", "/v1/billing/admin")
KEEP = ("/v1/emails","/v1/domains","/v1/senders","/v1/contacts","/v1/templates",
        "/v1/webhooks","/v1/suppressions","/v1/forwards","/v1/broadcasts","/v1/api-keys")
spec["paths"] = {p:v for p,v in spec["paths"].items()
                 if not any(p.startswith(d) for d in DROP) and any(p.startswith(k) for k in KEEP)}
spec["info"]["title"] = "Axene Mailer API (public)"
json.dump(spec, open("spec/openapi.json","w"), indent=2)
print("wrote spec/openapi.json with", len(spec["paths"]), "public paths")
PY
