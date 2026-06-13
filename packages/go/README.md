# Axene Mailer Go SDK

Official Go client for the [Axene Mailer](https://mail.axene.io) API: email
sending, domains, contacts, suppressions, templates, and webhooks.

Stdlib only. Go 1.21+. MIT licensed.

## Install

```sh
go get github.com/Axene-Solutions/axene-mailer-go
```

```go
import axene "github.com/Axene-Solutions/axene-mailer-go"
```

## Quickstart

```go
package main

import (
	"context"
	"log"

	axene "github.com/Axene-Solutions/axene-mailer-go"
)

func main() {
	client := axene.New("axm_k_...")

	resp, err := client.Emails.Send(context.Background(), axene.SendEmail{
		From:    axene.Addr("hello@yourdomain.com"),
		To:      []axene.Address{axene.Addr("customer@example.com")},
		Subject: "Your receipt",
		HTML:    "<p>Thanks for your order.</p>",
	})
	if err != nil {
		log.Fatal(err)
	}
	log.Printf("queued %s (%s)", resp.ID, resp.Status)
}
```

A sender or recipient is an `Address`. Use `axene.Addr("a@b.io")` for the
common case, or build one with a display name:

```go
axene.Address{Email: "a@b.io", Name: "Support"}
```

## Configuration

`New` takes the API key and optional functional options:

```go
client := axene.New(
	"axm_k_...",
	axene.WithBaseURL("https://mail.axene.io"), // default
	axene.WithMaxRetries(3),                    // default
	axene.WithTimeout(30*time.Second),          // default
	axene.WithHTTPClient(myHTTPClient),         // optional
)
```

The client retries `429` and `5xx` responses with exponential backoff,
honoring the `Retry-After` header. It never retries other `4xx` responses.

## Errors

Every method returns an `error` that is a `*axene.Error` on a non-2xx response
or a transport failure:

```go
_, err := client.Emails.Send(ctx, msg)
if err != nil {
	var ae *axene.Error
	if errors.As(err, &ae) {
		log.Printf("status=%d code=%s message=%s", ae.Status, ae.Code, ae.Message)
	}
}
```

`Status` is `0` for a network/transport failure with no HTTP response.

## Resources

- `client.Emails` - Send, SendBatch, Validate, List, Get, Events, Retry,
  Search, ListScheduled, CancelScheduled, SendScheduledNow, Updates,
  GetSavedSearches, SetSavedSearches.
- `client.Domains` - List, Create, Get, Delete, Verify, Health, Diagnose,
  MxStatus, PublishedRecords, RotateDkim, Transfer, CheckAvailability, Check.
- `client.Contacts` - ListLists, CreateList, GetList, UpdateList, DeleteList,
  AddContact, RemoveContact, UploadCSV, BulkSend.
- `client.Suppressions` - List, Add, BulkUpload, Remove.
- `client.Templates` - List, Create, Get, Update, Delete, Duplicate.
- `client.Webhooks` - List, Create, Update, Delete, Test, ListDeliveries,
  GetDelivery.

### Pagination

Pagination is zero-based: `Page: 0` is the first page. Most list endpoints
return a bare slice. Suppressions list and webhook deliveries return a
`Page[T]` envelope:

```go
page, _ := client.Suppressions.List(ctx, axene.ListSuppressionsParams{Page: 0, Limit: 50})
for _, s := range page.Items {
	log.Println(s.EmailAddress)
}
log.Printf("%d total", page.Total)
```

### CSV uploads

`Contacts.UploadCSV` and `Suppressions.BulkUpload` send the file as
`multipart/form-data` under the field name `file`:

```go
data, _ := os.ReadFile("contacts.csv")
res, _ := client.Contacts.UploadCSV(ctx, listID, data, "contacts.csv")
log.Printf("imported %d, skipped %d", res.Imported, res.Skipped)
```

## Not yet covered

The advanced domain endpoints (ns-provider, BIMI, domain-connect) are not
wrapped in this version.

## License

MIT
