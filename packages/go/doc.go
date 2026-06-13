// Package axene is the official Go client for the Axene Mailer API.
//
// Axene Mailer is an email marketing and transactional sending platform.
// This package wraps the REST API at https://mail.axene.io behind a small,
// typed client. Construct a client with an API key (it starts with axm_k_)
// and reach the API through resource fields:
//
//	client := axene.New("axm_k_...")
//	resp, err := client.Emails.Send(ctx, axene.SendEmail{
//		From:    axene.Addr("hello@yourdomain.com"),
//		To:      []axene.Address{axene.Addr("customer@example.com")},
//		Subject: "Your receipt",
//		HTML:    "<p>Thanks for your order.</p>",
//	})
//
// The client owns one transport layer (transport.go) that handles bearer
// authentication, JSON encoding, the from -> from_ wire mapping, retries on
// 429 and 5xx with backoff (honoring Retry-After), and error mapping to
// *Error. Resource types (Emails, Domains, Contacts, Suppressions, Templates,
// Webhooks) are thin and delegate to that transport. Every method takes a
// context.Context as its first argument.
//
// Pagination is zero-based: page 0 is the first page. Most list endpoints
// return a bare slice; suppressions list and webhook deliveries return a
// Page[T] envelope.
package axene
