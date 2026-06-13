# Axene Mailer SDKs

Official client libraries for [Axene Mailer](https://axene.io): professional email for Africa.
Send receipts, confirmations, and campaigns from your own domain. Priced in KES, billed via M-Pesa.

This is a spec-driven monorepo: every client wraps the same public API
(`spec/openapi.json`), so the libraries stay consistent across languages.

| Language | Package | Registry | Status |
|---|---|---|---|
| TypeScript / JavaScript | [`@axene/mailer`](packages/typescript) | npm | ✅ ready |
| .NET (C#) | [`Axene.Mailer`](packages/dotnet) | NuGet | ✅ ready |
| Python | `axene-mailer` | PyPI | 🚧 planned |
| Java | [`io.axene:mailer`](packages/java) | Maven Central | ✅ ready |
| Go | `github.com/Axene-Solutions/axene-mailer-go` | n/a | 🚧 separate repo |

> Go lives in its own repo because a Go module path is the repo path
> (`go get github.com/Axene-Solutions/axene-mailer-go`). Everything else is fine in this monorepo.

## Quickstart

**TypeScript**
```bash
npm install @axene/mailer
```
```ts
import { Axene } from '@axene/mailer';
const axene = new Axene({ apiKey: process.env.AXENE_API_KEY! });
await axene.emails.send({
  from: 'hello@yourdomain.com',
  to: 'customer@example.com',
  subject: 'Your receipt',
  html: '<p>Thanks for your order.</p>',
});
```

**.NET**
```bash
dotnet add package Axene.Mailer
```
```csharp
using Axene.Mailer;
var axene = new AxeneMailerClient("axm_k_your_api_key");
await axene.SendAsync(new SendEmail {
    From = "hello@yourdomain.com",
    To = { "customer@example.com" },
    Subject = "Your receipt",
    Html = "<p>Thanks for your order.</p>",
});
```

Get an API key from your dashboard at [mail.axene.io](https://mail.axene.io).
Full docs: <https://axene.io/docs/mailer/getting-started/welcome>.

## Releasing

Each package releases independently via a tag prefix (see `.github/workflows/release.yml`):

```bash
git tag ts-v0.1.0     && git push --tags   # -> npm
git tag dotnet-v0.1.0 && git push --tags   # -> NuGet
```

- **npm** uses `NPM_TOKEN` (repo secret) + provenance.
- **NuGet** uses Trusted Publishing (OIDC), so no API key is stored.

## Keeping the spec in sync

`spec/openapi.json` is the public surface of the Axene Mailer API (admin/internal
routes are intentionally excluded). Regenerate it from the backend with
`scripts/export-spec.sh` whenever the API changes.

## License

MIT © Axene Solutions
