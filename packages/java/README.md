# io.axene:mailer (Java)

Official Java client for [Axene Mailer](https://axene.io). Send receipts, confirmations,
and campaigns from your own domain, priced in KES, billed via M-Pesa.

Targets Java 11+. One dependency (Jackson) for JSON.

## Install

Gradle:
```kotlin
implementation("io.axene:mailer:0.1.0")
```

Maven:
```xml
<dependency>
  <groupId>io.axene</groupId>
  <artifactId>mailer</artifactId>
  <version>0.1.0</version>
</dependency>
```

## Usage

```java
import io.axene.mailer.*;

AxeneMailerClient axene = new AxeneMailerClient("axm_k_your_api_key");

SendEmailResult res = axene.send(SendEmail.builder()
    .from("hello@yourdomain.com", "Your Shop")
    .to("customer@example.com")
    .subject("Your receipt")
    .html("<p>Thanks for your order.</p>")
    .text("Thanks for your order.")
    .build());

System.out.println("queued " + res.id);
```

### More

```java
axene.sendBatch(List.of(/* ... */));
EmailRecord email = axene.get(res.id);              // status
axene.validate("someone@example.com");               // address check
List<DomainRecord> domains = axene.listDomains();     // your sending domains

// Scheduling (Starter plan and up)
axene.send(SendEmail.builder()
    .from("hello@yourdomain.com").to("a@example.com").subject("s")
    .sendAt(java.time.Instant.now().plusSeconds(3600))
    .build());
```

### Errors and retries

Non-2xx responses throw `AxeneException` (`getStatus()`, `getCode()`, `getMessage()`).
The client retries 429 and 5xx with exponential backoff (configurable via the
`maxRetries` constructor argument).

Get an API key at [mail.axene.io](https://mail.axene.io). Docs: <https://axene.io/docs/mailer/getting-started/welcome>.

MIT (c) Axene Solutions
