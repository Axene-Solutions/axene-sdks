package axene

import (
	"bytes"
	"context"
	"encoding/json"
	"fmt"
	"io"
	"mime/multipart"
	"net/http"
	"strconv"
	"time"
)

const (
	defaultBaseURL    = "https://mail.axene.io"
	defaultMaxRetries = 3
	defaultTimeout    = 30 * time.Second
	userAgent         = "axene-mailer-go"
)

// transport is the single place that talks to the network. It owns bearer
// authentication, JSON encode/decode, retries on 429 and 5xx with backoff
// (honoring Retry-After), multipart uploads, and error mapping to *Error.
type transport struct {
	apiKey     string
	baseURL    string
	maxRetries int
	httpClient *http.Client
}

// doRequest performs a JSON request and decodes the response into out (which
// may be nil for empty bodies, such as 204 responses). The body, if non-nil,
// is JSON-encoded. Retries 429 and 5xx with exponential backoff, honoring
// Retry-After. Never retries other 4xx responses.
func (t *transport) doRequest(ctx context.Context, method, path string, body, out any) error {
	var encoded []byte
	if body != nil {
		var err error
		encoded, err = json.Marshal(body)
		if err != nil {
			return &Error{Status: 0, Message: fmt.Sprintf("failed to encode request body: %v", err)}
		}
	}

	var lastErr error
	for attempt := 1; attempt <= t.maxRetries; attempt++ {
		var reader io.Reader
		if encoded != nil {
			reader = bytes.NewReader(encoded)
		}
		req, err := http.NewRequestWithContext(ctx, method, t.baseURL+path, reader)
		if err != nil {
			return &Error{Status: 0, Message: fmt.Sprintf("failed to build request: %v", err)}
		}
		req.Header.Set("Authorization", "Bearer "+t.apiKey)
		req.Header.Set("User-Agent", userAgent)
		if encoded != nil {
			req.Header.Set("Content-Type", "application/json")
		}

		resp, err := t.httpClient.Do(req)
		if err != nil {
			// Transport or network failure: retry if attempts remain.
			lastErr = err
			if attempt < t.maxRetries {
				if werr := sleepCtx(ctx, backoff(nil, attempt)); werr != nil {
					return werr
				}
				continue
			}
			break
		}

		if isRetryable(resp.StatusCode) && attempt < t.maxRetries {
			wait := backoff(resp, attempt)
			resp.Body.Close()
			if werr := sleepCtx(ctx, wait); werr != nil {
				return werr
			}
			continue
		}

		return t.finish(resp, out)
	}

	return &Error{Status: 0, Message: fmt.Sprintf("request failed: %v", lastErr)}
}

// upload posts a single file as multipart/form-data under the field name
// "file". Uploads are not retried because they are not idempotent.
func (t *transport) upload(ctx context.Context, path string, file []byte, filename string, out any) error {
	var buf bytes.Buffer
	mw := multipart.NewWriter(&buf)
	part, err := mw.CreateFormFile("file", filename)
	if err != nil {
		return &Error{Status: 0, Message: fmt.Sprintf("failed to build multipart form: %v", err)}
	}
	if _, err := part.Write(file); err != nil {
		return &Error{Status: 0, Message: fmt.Sprintf("failed to write file: %v", err)}
	}
	if err := mw.Close(); err != nil {
		return &Error{Status: 0, Message: fmt.Sprintf("failed to finalize multipart form: %v", err)}
	}

	req, err := http.NewRequestWithContext(ctx, http.MethodPost, t.baseURL+path, &buf)
	if err != nil {
		return &Error{Status: 0, Message: fmt.Sprintf("failed to build request: %v", err)}
	}
	req.Header.Set("Authorization", "Bearer "+t.apiKey)
	req.Header.Set("User-Agent", userAgent)
	req.Header.Set("Content-Type", mw.FormDataContentType())

	resp, err := t.httpClient.Do(req)
	if err != nil {
		return &Error{Status: 0, Message: fmt.Sprintf("upload failed: %v", err)}
	}
	return t.finish(resp, out)
}

// finish reads the response, mapping non-2xx into *Error and decoding 2xx
// bodies into out when out is non-nil.
func (t *transport) finish(resp *http.Response, out any) error {
	defer resp.Body.Close()
	data, _ := io.ReadAll(resp.Body)

	if resp.StatusCode < 200 || resp.StatusCode >= 300 {
		return mapError(resp.StatusCode, data)
	}
	if out == nil || len(data) == 0 {
		return nil
	}
	if err := json.Unmarshal(data, out); err != nil {
		return &Error{Status: resp.StatusCode, Message: fmt.Sprintf("failed to decode response: %v", err)}
	}
	return nil
}

// isRetryable reports whether a status code should be retried.
func isRetryable(status int) bool {
	return status == http.StatusTooManyRequests || status >= 500
}

// backoff returns how long to wait before the next attempt. It honors a
// Retry-After header (in seconds) when present, otherwise uses exponential
// backoff starting at 250ms.
func backoff(resp *http.Response, attempt int) time.Duration {
	if resp != nil {
		if ra := resp.Header.Get("Retry-After"); ra != "" {
			if secs, err := strconv.Atoi(ra); err == nil && secs > 0 {
				return time.Duration(secs) * time.Second
			}
		}
	}
	return time.Duration(250*(1<<(attempt-1))) * time.Millisecond
}

// sleepCtx waits for d, returning early if the context is cancelled.
func sleepCtx(ctx context.Context, d time.Duration) error {
	timer := time.NewTimer(d)
	defer timer.Stop()
	select {
	case <-ctx.Done():
		return &Error{Status: 0, Message: fmt.Sprintf("request cancelled: %v", ctx.Err())}
	case <-timer.C:
		return nil
	}
}

// mapError turns the API's { "detail": { code, message } } or { "detail":
// string } body into an *Error.
func mapError(status int, data []byte) *Error {
	out := &Error{Status: status, Message: fmt.Sprintf("request failed (status %d)", status)}

	var envelope struct {
		Detail json.RawMessage `json:"detail"`
	}
	if err := json.Unmarshal(data, &envelope); err != nil || len(envelope.Detail) == 0 {
		return out
	}

	var structured struct {
		Code    string `json:"code"`
		Message string `json:"message"`
	}
	if err := json.Unmarshal(envelope.Detail, &structured); err == nil && structured.Message != "" {
		out.Code = structured.Code
		out.Message = structured.Message
		return out
	}

	var plain string
	if err := json.Unmarshal(envelope.Detail, &plain); err == nil && plain != "" {
		out.Message = plain
	}
	return out
}
