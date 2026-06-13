package axene

import (
	"context"
	"net/url"
	"strconv"
)

// Webhooks is the webhooks resource, reached as client.Webhooks.
type Webhooks struct {
	http *transport
}

// List returns your active webhooks.
func (w *Webhooks) List(ctx context.Context) ([]Webhook, error) {
	var out []Webhook
	if err := w.http.doRequest(ctx, "GET", "/v1/webhooks/", nil, &out); err != nil {
		return nil, err
	}
	return out, nil
}

// Create creates a webhook. The signing secret is generated and returned.
func (w *Webhooks) Create(ctx context.Context, params CreateWebhookParams) (*Webhook, error) {
	var out Webhook
	if err := w.http.doRequest(ctx, "POST", "/v1/webhooks/", params, &out); err != nil {
		return nil, err
	}
	return &out, nil
}

// Update updates a webhook's url, events, or active state (partial).
func (w *Webhooks) Update(ctx context.Context, id string, params UpdateWebhookParams) (*Webhook, error) {
	var out Webhook
	if err := w.http.doRequest(ctx, "PATCH", "/v1/webhooks/"+url.PathEscape(id), params, &out); err != nil {
		return nil, err
	}
	return &out, nil
}

// Delete deletes a webhook.
func (w *Webhooks) Delete(ctx context.Context, id string) error {
	return w.http.doRequest(ctx, "DELETE", "/v1/webhooks/"+url.PathEscape(id), nil, nil)
}

// Test queues a sample email.delivered delivery to test the endpoint.
func (w *Webhooks) Test(ctx context.Context, id string) (*WebhookTestResult, error) {
	var out WebhookTestResult
	if err := w.http.doRequest(ctx, "POST", "/v1/webhooks/"+url.PathEscape(id)+"/test", nil, &out); err != nil {
		return nil, err
	}
	return &out, nil
}

// ListDeliveries lists delivery attempts for a webhook as a paginated envelope.
func (w *Webhooks) ListDeliveries(ctx context.Context, id string, params ListDeliveriesParams) (*Page[WebhookDelivery], error) {
	q := url.Values{}
	q.Set("page", strconv.Itoa(params.Page))
	if params.Limit > 0 {
		q.Set("limit", strconv.Itoa(params.Limit))
	}
	if params.Status != "" {
		q.Set("status", params.Status)
	}
	var out Page[WebhookDelivery]
	if err := w.http.doRequest(ctx, "GET", "/v1/webhooks/"+url.PathEscape(id)+"/deliveries?"+q.Encode(), nil, &out); err != nil {
		return nil, err
	}
	return &out, nil
}

// GetDelivery fetches one delivery with its full payload and endpoint response.
func (w *Webhooks) GetDelivery(ctx context.Context, id, deliveryID string) (*WebhookDeliveryDetail, error) {
	var out WebhookDeliveryDetail
	if err := w.http.doRequest(ctx, "GET", "/v1/webhooks/"+url.PathEscape(id)+"/deliveries/"+url.PathEscape(deliveryID), nil, &out); err != nil {
		return nil, err
	}
	return &out, nil
}
