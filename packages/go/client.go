package axene

import (
	"net/http"
	"strings"
	"time"
)

// Client is the Axene Mailer API client. Construct it with New and reach the
// API through its resource fields. Client is safe for concurrent use.
type Client struct {
	// Emails sends, searches, schedules, and inspects messages.
	Emails *Emails
	// Domains registers, verifies, and transfers sending domains.
	Domains *Domains
	// Contacts manages subscriber lists and bulk sends.
	Contacts *Contacts
	// Suppressions manages the do-not-send list.
	Suppressions *Suppressions
	// Templates manages reusable email templates.
	Templates *Templates
	// Webhooks manages event webhooks and inspects deliveries.
	Webhooks *Webhooks

	transport *transport
}

// Option configures a Client. Pass options to New.
type Option func(*transport)

// WithBaseURL overrides the API base URL (default https://mail.axene.io).
func WithBaseURL(baseURL string) Option {
	return func(t *transport) {
		t.baseURL = strings.TrimRight(baseURL, "/")
	}
}

// WithMaxRetries sets the total number of attempts on 429/5xx, including the
// first (default 3).
func WithMaxRetries(maxRetries int) Option {
	return func(t *transport) {
		if maxRetries >= 1 {
			t.maxRetries = maxRetries
		}
	}
}

// WithTimeout sets the per-request timeout (default 30s).
func WithTimeout(timeout time.Duration) Option {
	return func(t *transport) {
		t.httpClient.Timeout = timeout
	}
}

// WithHTTPClient injects a custom *http.Client (for testing or proxies).
func WithHTTPClient(hc *http.Client) Option {
	return func(t *transport) {
		if hc != nil {
			t.httpClient = hc
		}
	}
}

// New constructs a Client. apiKey is required and starts with axm_k_.
func New(apiKey string, opts ...Option) *Client {
	t := &transport{
		apiKey:     apiKey,
		baseURL:    defaultBaseURL,
		maxRetries: defaultMaxRetries,
		httpClient: &http.Client{Timeout: defaultTimeout},
	}
	for _, opt := range opts {
		opt(t)
	}

	c := &Client{transport: t}
	c.Emails = &Emails{http: t}
	c.Domains = &Domains{http: t}
	c.Contacts = &Contacts{http: t}
	c.Suppressions = &Suppressions{http: t}
	c.Templates = &Templates{http: t}
	c.Webhooks = &Webhooks{http: t}
	return c
}
