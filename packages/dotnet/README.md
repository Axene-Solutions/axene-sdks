# Axene.Mailer

Official .NET client for [Axene Mailer](https://axene.io). Send receipts, confirmations,
and campaigns from your own domain, priced in KES, billed via M-Pesa.

Targets `netstandard2.0` and `net8.0`, so it works from .NET Framework, .NET Core,
.NET 8, and Unity.

## Install

```bash
dotnet add package Axene.Mailer
```

## Usage

```csharp
using Axene.Mailer;

var axene = new AxeneMailerClient("axm_k_your_api_key");

var result = await axene.SendAsync(new SendEmail
{
    From = new Address("hello@yourdomain.com", "Your Shop"),
    To = { "customer@example.com" },
    Subject = "Your receipt",
    Html = "<p>Thanks for your order.</p>",
    Text = "Thanks for your order.",
});

Console.WriteLine($"queued {result.Id}");
```

`Address` converts implicitly from a string, so `To = { "a@example.com" }` works.

### More

```csharp
await axene.SendBatchAsync(new[] { /* … */ });
var email = await axene.GetAsync(result.Id);          // status
await axene.ValidateAsync("someone@example.com");      // address check

// Scheduling (Starter plan and up)
await axene.SendAsync(new SendEmail { /* … */, SendAt = DateTimeOffset.UtcNow.AddHours(1) });
```

### Dependency injection

Pass your own `HttpClient` (e.g. from `IHttpClientFactory`):

```csharp
services.AddHttpClient();
services.AddSingleton(sp =>
    new AxeneMailerClient(
        apiKey: config["Axene:ApiKey"]!,
        httpClient: sp.GetRequiredService<IHttpClientFactory>().CreateClient()));
```

### Errors & retries

Non-2xx responses throw `AxeneException` (`.Status`, `.Code`, `.Message`). The client
retries `429` and `5xx` with exponential backoff (configurable via `maxRetries`).

Get an API key at [mail.axene.io](https://mail.axene.io). Docs: <https://axene.io/docs/mailer/getting-started/welcome>.

MIT © Axene Solutions
