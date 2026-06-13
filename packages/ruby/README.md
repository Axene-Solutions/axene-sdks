# axene-mailer (Ruby)

Ruby SDK for the [Axene Mailer](https://axene.io) API. Send email, manage
domains, contacts, suppressions, templates, and webhooks. Zero runtime
dependencies (standard library only). Ruby 3.0+.

## Install

```ruby
# Gemfile
gem "axene-mailer"
```

```sh
bundle install
# or
gem install axene-mailer
```

## Quickstart

```ruby
require "axene/mailer"

client = Axene::Mailer::Client.new(api_key: ENV.fetch("AXENE_API_KEY"))

# Send a single email. A bare string is sugar for { email: ... }.
result = client.emails.send(
  from: "hello@yourdomain.com",
  to: "customer@example.com",
  subject: "Your receipt",
  html: "<p>Thanks for your order.</p>"
)
puts result[:id]
```

The client exposes six resources: `emails`, `domains`, `contacts`,
`suppressions`, `templates`, and `webhooks`. Methods return parsed Ruby
`Hash`/`Array` values with symbol keys.

### Addresses

Anywhere an address is accepted you may pass a plain string or a hash:

```ruby
client.emails.send(
  from: { email: "hello@yourdomain.com", name: "Acme" },
  to: ["a@example.com", { email: "b@example.com", name: "B" }],
  subject: "Hi",
  text: "Plain text body"
)
```

### Batch and validation

```ruby
client.emails.send_batch([
  { from: "hi@you.io", to: "a@example.com", subject: "1" },
  { from: "hi@you.io", to: "b@example.com", subject: "2" }
])

check = client.emails.validate(from: "hi@you.io", to: "a@example.com", subject: "Test")
puts check[:can_send]
```

### Contacts and CSV upload

```ruby
list = client.contacts.create_list(name: "Newsletter")

# Upload accepts raw bytes or a file path.
client.contacts.upload_csv(list[:id], "contacts.csv", filename: "contacts.csv")

client.contacts.bulk_send(
  list[:id],
  sender_address_id: "sa_123",
  subject: "Hello {{name}}",
  html: "<p>Hi {{name}}</p>"
)
```

### Suppressions, templates, webhooks

```ruby
client.suppressions.add(email: "bounce@example.com")
page = client.suppressions.list(page: 0, limit: 50)  # envelope: items/total/page/limit

client.templates.create(name: "Welcome", html: "<p>Hi</p>", text: "Hi")

hook = client.webhooks.create(url: "https://you.io/hooks", events: ["email.delivered"])
client.webhooks.update(hook[:id], is_active: false)
```

### Errors

Non-2xx responses raise `Axene::Mailer::Error`:

```ruby
begin
  client.emails.get("nope")
rescue Axene::Mailer::Error => e
  warn "#{e.status} #{e.code}: #{e.message}"
end
```

A `status` of `0` indicates a transport failure (no HTTP response). Requests
that fail with `429` or `5xx` are retried automatically with backoff, honoring
the `Retry-After` header.

### Configuration

```ruby
Axene::Mailer::Client.new(
  api_key: "axm_k_...",
  base_url: "https://mail.axene.io",  # default
  max_retries: 3,                     # default
  timeout: 30                         # seconds, default
)
```

Pagination is zero-based (`page: 0` is the first page).

## License

MIT
