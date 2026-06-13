package axene

import (
	"context"
	"net/url"
	"strconv"
)

// Suppressions is the suppressions resource, reached as client.Suppressions.
type Suppressions struct {
	http *transport
}

// List returns suppressed addresses as a paginated envelope (zero-based page).
func (s *Suppressions) List(ctx context.Context, params ListSuppressionsParams) (*Page[Suppression], error) {
	q := url.Values{}
	q.Set("page", strconv.Itoa(params.Page))
	if params.Limit > 0 {
		q.Set("limit", strconv.Itoa(params.Limit))
	}
	if params.Search != "" {
		q.Set("search", params.Search)
	}
	var out Page[Suppression]
	if err := s.http.doRequest(ctx, "GET", "/v1/suppressions?"+q.Encode(), nil, &out); err != nil {
		return nil, err
	}
	return &out, nil
}

// Add suppresses a single address. Reason defaults to "manual" when empty.
func (s *Suppressions) Add(ctx context.Context, params AddSuppressionParams) (*Suppression, error) {
	if params.Reason == "" {
		params.Reason = "manual"
	}
	var out Suppression
	if err := s.http.doRequest(ctx, "POST", "/v1/suppressions", params, &out); err != nil {
		return nil, err
	}
	return &out, nil
}

// BulkUpload imports suppressions from a file (one email per line). The upload
// is sent as multipart/form-data under the field name "file".
func (s *Suppressions) BulkUpload(ctx context.Context, file []byte, filename string) (*BulkSuppressionResult, error) {
	if filename == "" {
		filename = "suppressions.txt"
	}
	var out BulkSuppressionResult
	if err := s.http.upload(ctx, "/v1/suppressions/bulk", file, filename, &out); err != nil {
		return nil, err
	}
	return &out, nil
}

// Remove removes an address from the suppression list.
func (s *Suppressions) Remove(ctx context.Context, id string) error {
	return s.http.doRequest(ctx, "DELETE", "/v1/suppressions/"+url.PathEscape(id), nil, nil)
}
