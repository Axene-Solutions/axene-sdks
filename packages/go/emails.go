package axene

import (
	"context"
	"net/url"
	"strconv"
)

// Emails is the emails resource, reached as client.Emails.
type Emails struct {
	http *transport
}

// Send sends a single email.
func (e *Emails) Send(ctx context.Context, msg SendEmail) (*SendResult, error) {
	var out SendResult
	if err := e.http.doRequest(ctx, "POST", "/v1/emails/", msg, &out); err != nil {
		return nil, err
	}
	return &out, nil
}

// SendBatch sends up to the plan's batch limit in one call. The API accepts a
// bare array of messages and returns a per-message result set.
func (e *Emails) SendBatch(ctx context.Context, msgs []SendEmail) (*BatchResult, error) {
	var out BatchResult
	if err := e.http.doRequest(ctx, "POST", "/v1/emails/batch", msgs, &out); err != nil {
		return nil, err
	}
	return &out, nil
}

// Validate dry-runs a send: it checks whether msg would be accepted (sender
// registered, domain verified, plan limits) without actually sending it.
func (e *Emails) Validate(ctx context.Context, msg SendEmail) (*ValidationResult, error) {
	var out ValidationResult
	if err := e.http.doRequest(ctx, "POST", "/v1/emails/validate", msg, &out); err != nil {
		return nil, err
	}
	return &out, nil
}

// List returns recent emails, newest first.
func (e *Emails) List(ctx context.Context, params ListEmailsParams) ([]Email, error) {
	q := url.Values{}
	if params.Status != "" {
		q.Set("status", params.Status)
	}
	q.Set("page", strconv.Itoa(params.Page))
	if params.Limit > 0 {
		q.Set("limit", strconv.Itoa(params.Limit))
	}
	var out []Email
	if err := e.http.doRequest(ctx, "GET", "/v1/emails/?"+q.Encode(), nil, &out); err != nil {
		return nil, err
	}
	return out, nil
}

// Get fetches a single email with its bodies and events.
func (e *Emails) Get(ctx context.Context, id string) (*EmailDetail, error) {
	var out EmailDetail
	if err := e.http.doRequest(ctx, "GET", "/v1/emails/"+url.PathEscape(id), nil, &out); err != nil {
		return nil, err
	}
	return &out, nil
}

// Events lists delivery, open, click, and bounce events for an email.
func (e *Emails) Events(ctx context.Context, id string) ([]EmailEvent, error) {
	var out []EmailEvent
	if err := e.http.doRequest(ctx, "GET", "/v1/emails/"+url.PathEscape(id)+"/events", nil, &out); err != nil {
		return nil, err
	}
	return out, nil
}

// Retry re-sends a bounced, rejected, or failed email as a new message.
func (e *Emails) Retry(ctx context.Context, id string) (*SendResult, error) {
	var out SendResult
	if err := e.http.doRequest(ctx, "POST", "/v1/emails/"+url.PathEscape(id)+"/retry", nil, &out); err != nil {
		return nil, err
	}
	return &out, nil
}

// Search searches emails. q supports inline tokens (to:, from:, status:,
// domain:, tag:); leftover words are matched as free text.
func (e *Emails) Search(ctx context.Context, params SearchEmailsParams) ([]EmailSearchHit, error) {
	q := url.Values{}
	q.Set("q", params.Q)
	if params.Status != "" {
		q.Set("status", params.Status)
	}
	if params.Tag != "" {
		q.Set("tag", params.Tag)
	}
	q.Set("page", strconv.Itoa(params.Page))
	if params.Limit > 0 {
		q.Set("limit", strconv.Itoa(params.Limit))
	}
	var out []EmailSearchHit
	if err := e.http.doRequest(ctx, "GET", "/v1/emails/search?"+q.Encode(), nil, &out); err != nil {
		return nil, err
	}
	return out, nil
}

// ListScheduled lists emails scheduled for future delivery, soonest first.
func (e *Emails) ListScheduled(ctx context.Context) ([]ScheduledEmail, error) {
	var out []ScheduledEmail
	if err := e.http.doRequest(ctx, "GET", "/v1/emails/scheduled", nil, &out); err != nil {
		return nil, err
	}
	return out, nil
}

// CancelScheduled cancels a scheduled email.
func (e *Emails) CancelScheduled(ctx context.Context, id string) (*IDStatus, error) {
	var out IDStatus
	if err := e.http.doRequest(ctx, "DELETE", "/v1/emails/scheduled/"+url.PathEscape(id), nil, &out); err != nil {
		return nil, err
	}
	return &out, nil
}

// SendScheduledNow sends a scheduled email immediately instead of waiting.
func (e *Emails) SendScheduledNow(ctx context.Context, id string) (*IDStatus, error) {
	var out IDStatus
	if err := e.http.doRequest(ctx, "POST", "/v1/emails/scheduled/"+url.PathEscape(id)+"/send-now", nil, &out); err != nil {
		return nil, err
	}
	return &out, nil
}

// Updates polls for emails whose status changed at or after since (ISO 8601).
// The result is capped at 50 rows. since is required.
func (e *Emails) Updates(ctx context.Context, since string) ([]Email, error) {
	q := url.Values{}
	q.Set("since", since)
	var out []Email
	if err := e.http.doRequest(ctx, "GET", "/v1/emails/updates?"+q.Encode(), nil, &out); err != nil {
		return nil, err
	}
	return out, nil
}

// savedSearchEnvelope is the wire shape for the saved-searches endpoints.
type savedSearchEnvelope struct {
	Searches []SavedSearch `json:"searches"`
}

// GetSavedSearches returns the caller's saved email searches.
func (e *Emails) GetSavedSearches(ctx context.Context) ([]SavedSearch, error) {
	var out savedSearchEnvelope
	if err := e.http.doRequest(ctx, "GET", "/v1/emails/saved-searches", nil, &out); err != nil {
		return nil, err
	}
	return out.Searches, nil
}

// SetSavedSearches replaces the caller's saved email searches (max 50).
func (e *Emails) SetSavedSearches(ctx context.Context, searches []SavedSearch) ([]SavedSearch, error) {
	var out savedSearchEnvelope
	if err := e.http.doRequest(ctx, "PUT", "/v1/emails/saved-searches", savedSearchEnvelope{Searches: searches}, &out); err != nil {
		return nil, err
	}
	return out.Searches, nil
}
