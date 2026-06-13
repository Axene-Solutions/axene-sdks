# Axene Mailer SDKs

[![License: MIT](https://img.shields.io/badge/License-MIT-1F8A5B?style=flat-square&labelColor=101010)](LICENSE)
[![npm](https://img.shields.io/badge/npm-%40axene%2Fmailer-CB3837?style=flat-square&labelColor=101010)](https://www.npmjs.com/package/@axene/mailer)
[![NuGet](https://img.shields.io/badge/NuGet-Axene.Mailer-004880?style=flat-square&labelColor=101010)](https://www.nuget.org/packages/Axene.Mailer)
[![Maven Central](https://img.shields.io/badge/Maven-io.axene%3Amailer-C71A36?style=flat-square&labelColor=101010)](https://central.sonatype.com/)
[![Built in Kenya](https://img.shields.io/badge/Built%20in-Nairobi%2C%20Kenya-FFD100?style=flat-square&labelColor=101010)](https://axene.io)

Official client libraries for [Axene Mailer](https://axene.io): professional email for Africa.
Send receipts, confirmations, and campaigns from your own domain. Priced in KES, billed via M-Pesa.

This is a spec-driven monorepo: every client wraps the same public API
(`spec/openapi.json`), so the libraries stay consistent across languages.

| Language | Package | Registry | Status |
|---|---|---|---|
| TypeScript / JavaScript | [`@axene/mailer`](packages/typescript) | npm | ✅ ready |
| .NET (C#) | [`Axene.Mailer`](packages/dotnet) | NuGet | ✅ ready |
| Python | [`axene-mailer`](packages/python) | PyPI | ✅ ready |
| Java | [`io.axene:mailer`](packages/java) | Maven Central | ✅ ready |
| Rust | [`axene-mailer`](packages/rust) | crates.io | ✅ ready |
| Ruby | [`axene-mailer`](packages/ruby) | RubyGems | ✅ ready |
| PHP | [`axene/mailer`](packages/php) | Packagist | ✅ ready |
| Swift | [`AxeneMailer`](packages/swift) | SwiftPM (git tag) | ✅ ready |
| Go | [`axene`](packages/go) | git tag | 🚧 moves to its own repo |

Every client covers the same Core surface (emails, domains, contacts,
suppressions, templates, webhooks), defined once in [`spec/SURFACE.md`](spec/SURFACE.md)
and [`spec/DESIGN.md`](spec/DESIGN.md) and extracted from the live backend.

> Go lives in its own repo at release time because a Go module path is the repo
> path (`go get github.com/Axene-Solutions/axene-mailer-go`). The code is
> developed here under `packages/go` and extracted on release.

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
git tag py-v0.1.0     && git push --tags   # -> PyPI
git tag java-v0.1.0   && git push --tags   # -> Maven Central
git tag rust-v0.1.0   && git push --tags   # -> crates.io
git tag ruby-v0.1.0   && git push --tags   # -> RubyGems
git tag php-v0.1.0    && git push --tags   # -> Packagist (auto-sync on tag)
git tag swift-v0.1.0  && git push --tags   # -> SwiftPM resolves the tag
```

- **npm** uses `NPM_TOKEN` (repo secret) + provenance.
- **NuGet**, **PyPI**, **crates.io**, and **RubyGems** use Trusted Publishing
  (OIDC), so no API keys are stored.
- **Maven Central** uses the Sonatype Central Portal (GPG-signed, `io.axene`).
- **Packagist** auto-syncs from git tags via its webhook; **SwiftPM** resolves
  the git tag directly. Both release jobs only run tests as a gate.
- Every release runs the package's tests before publishing.

## Keeping the spec in sync

`spec/openapi.json` is the public surface of the Axene Mailer API (admin/internal
routes are intentionally excluded). Regenerate it from the backend with
`scripts/export-spec.sh` whenever the API changes.

## Contributing

Contributions are welcome. The SDKs are spec-driven, so most changes start from
`spec/openapi.json`. Open an issue to discuss larger changes, then send a pull
request. Run the per-package test suites before submitting.

## License

MIT © Axene Solutions
