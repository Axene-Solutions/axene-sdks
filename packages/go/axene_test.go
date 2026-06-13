package axene

import (
	"context"
	"encoding/json"
	"io"
	"mime"
	"mime/multipart"
	"net/http"
	"net/http/httptest"
	"strings"
	"sync/atomic"
	"testing"
)

// newTestClient points a Client at a test server.
func newTestClient(t *testing.T, srv *httptest.Server) *Client {
	t.Helper()
	return New("axm_k_test", WithBaseURL(srv.URL), WithHTTPClient(srv.Client()))
}

func TestBearerHeader(t *testing.T) {
	var gotAuth string
	srv := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		gotAuth = r.Header.Get("Authorization")
		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusAccepted)
		_, _ = w.Write([]byte(`{"id":"e1","status":"queued"}`))
	}))
	defer srv.Close()

	client := newTestClient(t, srv)
	_, err := client.Emails.Send(context.Background(), SendEmail{
		From:    Addr("a@x.io"),
		To:      []Address{Addr("b@y.io")},
		Subject: "Hi",
	})
	if err != nil {
		t.Fatalf("send: %v", err)
	}
	if gotAuth != "Bearer axm_k_test" {
		t.Fatalf("auth header = %q, want %q", gotAuth, "Bearer axm_k_test")
	}
}

func TestFromUnderscoreMapping(t *testing.T) {
	var body map[string]any
	srv := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		_ = json.NewDecoder(r.Body).Decode(&body)
		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusAccepted)
		_, _ = w.Write([]byte(`{"id":"e1","status":"queued"}`))
	}))
	defer srv.Close()

	client := newTestClient(t, srv)
	_, err := client.Emails.Send(context.Background(), SendEmail{
		From:    Address{Email: "a@x.io", Name: "Sender"},
		To:      []Address{Addr("b@y.io")},
		Subject: "Hi",
		HTML:    "<p>hi</p>",
	})
	if err != nil {
		t.Fatalf("send: %v", err)
	}
	if _, ok := body["from_"]; !ok {
		t.Fatalf("expected wire key from_, body keys = %v", keysOf(body))
	}
	if _, ok := body["from"]; ok {
		t.Fatalf("unexpected wire key from (should be from_)")
	}
	from := body["from_"].(map[string]any)
	if from["email"] != "a@x.io" || from["name"] != "Sender" {
		t.Fatalf("from_ = %v, want {email:a@x.io, name:Sender}", from)
	}
	// emails keep html/text (not html_body/text_body).
	if _, ok := body["html"]; !ok {
		t.Fatalf("expected email wire key html")
	}
}

func TestSendBatchBareArray(t *testing.T) {
	var raw json.RawMessage
	srv := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		raw, _ = io.ReadAll(r.Body)
		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusAccepted)
		_, _ = w.Write([]byte(`{"total":2,"sent":2,"failed":0,"results":[{"id":"a","status":"queued"},{"id":"b","status":"queued"}]}`))
	}))
	defer srv.Close()

	client := newTestClient(t, srv)
	res, err := client.Emails.SendBatch(context.Background(), []SendEmail{
		{From: Addr("a@x.io"), To: []Address{Addr("b@y.io")}, Subject: "1"},
		{From: Addr("a@x.io"), To: []Address{Addr("c@y.io")}, Subject: "2"},
	})
	if err != nil {
		t.Fatalf("batch: %v", err)
	}
	trimmed := strings.TrimSpace(string(raw))
	if !strings.HasPrefix(trimmed, "[") {
		t.Fatalf("batch body should be a bare array, got %s", trimmed)
	}
	if res.Total != 2 || res.Sent != 2 || len(res.Results) != 2 {
		t.Fatalf("unexpected batch result: %+v", res)
	}
}

func TestValidateFullBody(t *testing.T) {
	var path string
	var body map[string]any
	srv := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		path = r.URL.Path
		_ = json.NewDecoder(r.Body).Decode(&body)
		w.Header().Set("Content-Type", "application/json")
		_, _ = w.Write([]byte(`{"valid":true,"can_send":true,"issues":[],"plan":"starter","usage":{"daily":1,"daily_limit":100,"monthly":1,"monthly_limit":1000}}`))
	}))
	defer srv.Close()

	client := newTestClient(t, srv)
	res, err := client.Emails.Validate(context.Background(), SendEmail{
		From:    Addr("a@x.io"),
		To:      []Address{Addr("b@y.io")},
		Subject: "Check",
		Text:    "body",
		Tags:    []string{"t1"},
	})
	if err != nil {
		t.Fatalf("validate: %v", err)
	}
	if path != "/v1/emails/validate" {
		t.Fatalf("path = %q", path)
	}
	if _, ok := body["from_"]; !ok {
		t.Fatalf("validate should send the full send body with from_")
	}
	if !res.Valid || !res.CanSend || res.Plan != "starter" || res.Usage.MonthlyLimit != 1000 {
		t.Fatalf("unexpected validation result: %+v", res)
	}
}

func TestUploadCSVMultipart(t *testing.T) {
	var fieldName, fileContent string
	srv := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		ct := r.Header.Get("Content-Type")
		mt, params, _ := mime.ParseMediaType(ct)
		if mt != "multipart/form-data" {
			t.Errorf("content-type = %q, want multipart/form-data", ct)
		}
		mr := multipart.NewReader(r.Body, params["boundary"])
		part, err := mr.NextPart()
		if err != nil {
			t.Errorf("read part: %v", err)
		} else {
			fieldName = part.FormName()
			c, _ := io.ReadAll(part)
			fileContent = string(c)
		}
		w.Header().Set("Content-Type", "application/json")
		_, _ = w.Write([]byte(`{"imported":3,"skipped":1,"errors":[]}`))
	}))
	defer srv.Close()

	client := newTestClient(t, srv)
	res, err := client.Contacts.UploadCSV(context.Background(), "list1", []byte("email\nx@y.io\n"), "c.csv")
	if err != nil {
		t.Fatalf("upload: %v", err)
	}
	if fieldName != "file" {
		t.Fatalf("multipart field = %q, want file", fieldName)
	}
	if !strings.Contains(fileContent, "x@y.io") {
		t.Fatalf("file content not received: %q", fileContent)
	}
	if res.Imported != 3 || res.Skipped != 1 {
		t.Fatalf("unexpected import result: %+v", res)
	}
}

func TestSuppressionsEnvelopeParse(t *testing.T) {
	srv := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Content-Type", "application/json")
		_, _ = w.Write([]byte(`{"items":[{"id":"s1","email_address":"x@y.io","reason":"bounce"}],"total":1,"page":0,"limit":50}`))
	}))
	defer srv.Close()

	client := newTestClient(t, srv)
	page, err := client.Suppressions.List(context.Background(), ListSuppressionsParams{Page: 0, Limit: 50})
	if err != nil {
		t.Fatalf("list: %v", err)
	}
	if page.Total != 1 || page.Limit != 50 || len(page.Items) != 1 {
		t.Fatalf("unexpected envelope: %+v", page)
	}
	if page.Items[0].EmailAddress != "x@y.io" {
		t.Fatalf("email_address not parsed: %+v", page.Items[0])
	}
}

func TestSuppressionsAddWireField(t *testing.T) {
	var body map[string]any
	srv := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		_ = json.NewDecoder(r.Body).Decode(&body)
		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusCreated)
		_, _ = w.Write([]byte(`{"id":"s1","email_address":"x@y.io","reason":"manual"}`))
	}))
	defer srv.Close()

	client := newTestClient(t, srv)
	_, err := client.Suppressions.Add(context.Background(), AddSuppressionParams{Email: "x@y.io"})
	if err != nil {
		t.Fatalf("add: %v", err)
	}
	if body["email_address"] != "x@y.io" {
		t.Fatalf("expected wire field email_address, got %v", keysOf(body))
	}
	if body["reason"] != "manual" {
		t.Fatalf("reason default = %v, want manual", body["reason"])
	}
}

func TestWebhookIsActiveMapping(t *testing.T) {
	var body map[string]any
	srv := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		_ = json.NewDecoder(r.Body).Decode(&body)
		w.Header().Set("Content-Type", "application/json")
		_, _ = w.Write([]byte(`{"id":"w1","url":"https://h.io","events":["email.delivered"],"secret":"sec","is_active":false,"created_at":"2026-01-01T00:00:00Z"}`))
	}))
	defer srv.Close()

	client := newTestClient(t, srv)
	inactive := false
	wh, err := client.Webhooks.Update(context.Background(), "w1", UpdateWebhookParams{IsActive: &inactive})
	if err != nil {
		t.Fatalf("update: %v", err)
	}
	if _, ok := body["is_active"]; !ok {
		t.Fatalf("expected wire key is_active, got %v", keysOf(body))
	}
	if body["is_active"] != false {
		t.Fatalf("is_active = %v, want false", body["is_active"])
	}
	if wh.IsActive {
		t.Fatalf("parsed is_active should be false")
	}
}

func TestRetryThenSuccess(t *testing.T) {
	var calls int32
	srv := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		n := atomic.AddInt32(&calls, 1)
		if n == 1 {
			w.Header().Set("Retry-After", "0")
			w.WriteHeader(http.StatusTooManyRequests)
			return
		}
		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusAccepted)
		_, _ = w.Write([]byte(`{"id":"e1","status":"queued"}`))
	}))
	defer srv.Close()

	client := newTestClient(t, srv)
	res, err := client.Emails.Send(context.Background(), SendEmail{
		From:    Addr("a@x.io"),
		To:      []Address{Addr("b@y.io")},
		Subject: "Hi",
	})
	if err != nil {
		t.Fatalf("send after retry: %v", err)
	}
	if atomic.LoadInt32(&calls) != 2 {
		t.Fatalf("expected 2 calls (1 retry), got %d", calls)
	}
	if res.ID != "e1" {
		t.Fatalf("unexpected result: %+v", res)
	}
}

func TestErrorMapping(t *testing.T) {
	srv := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusUnprocessableEntity)
		_, _ = w.Write([]byte(`{"detail":{"code":"unverified_sender","message":"sender not verified"}}`))
	}))
	defer srv.Close()

	client := newTestClient(t, srv)
	_, err := client.Emails.Send(context.Background(), SendEmail{From: Addr("a@x.io"), To: []Address{Addr("b@y.io")}, Subject: "x"})
	if err == nil {
		t.Fatal("expected an error")
	}
	ae, ok := err.(*Error)
	if !ok {
		t.Fatalf("error type = %T, want *Error", err)
	}
	if ae.Status != 422 || ae.Code != "unverified_sender" || ae.Message != "sender not verified" {
		t.Fatalf("unexpected error: %+v", ae)
	}
}

func keysOf(m map[string]any) []string {
	out := make([]string, 0, len(m))
	for k := range m {
		out = append(out, k)
	}
	return out
}
