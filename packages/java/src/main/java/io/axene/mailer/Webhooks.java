package io.axene.mailer;

import com.fasterxml.jackson.databind.JavaType;

import java.util.List;

/** Manage event webhooks and inspect deliveries. Accessed as {@code client.webhooks()}. */
public final class Webhooks {

    private final ApiTransport transport;

    Webhooks(ApiTransport transport) {
        this.transport = transport;
    }

    /**
     * List your active webhooks.
     *
     * @return the webhooks.
     */
    public List<Webhook> list() {
        return transport.request("GET", "/v1/webhooks/", null, transport.listType(Webhook.class));
    }

    /**
     * Create a webhook. The signing {@code secret} is generated and returned.
     *
     * @param params the webhook fields ({@code url} and {@code events} required).
     * @return the created webhook.
     */
    public Webhook create(WebhookParams params) {
        return transport.request("POST", "/v1/webhooks/", params.toWire(), transport.type(Webhook.class));
    }

    /**
     * Update a webhook's url, events, or active state (partial). The
     * {@code isActive} field is sent as the wire field {@code is_active}.
     *
     * @param id     the webhook id.
     * @param params the fields to update.
     * @return the updated webhook.
     */
    public Webhook update(String id, WebhookParams params) {
        return transport.request("PATCH", "/v1/webhooks/" + Query.enc(id), params.toWire(), transport.type(Webhook.class));
    }

    /**
     * Delete a webhook.
     *
     * @param id the webhook id.
     */
    public void delete(String id) {
        transport.request("DELETE", "/v1/webhooks/" + Query.enc(id), null, transport.type(Void.class));
    }

    /**
     * Queue a sample {@code email.delivered} delivery to test the endpoint.
     *
     * @param id the webhook id.
     * @return confirmation that a delivery was queued and the target url.
     */
    public WebhookTestResult test(String id) {
        return transport.request("POST", "/v1/webhooks/" + Query.enc(id) + "/test", null, transport.type(WebhookTestResult.class));
    }

    /**
     * List delivery attempts for a webhook (paginated envelope).
     *
     * @param id     the webhook id.
     * @param page   zero-based page index.
     * @param limit  page size (1-100).
     * @param status optional status filter, or null.
     * @return a page of deliveries.
     */
    public Page<WebhookDelivery> listDeliveries(String id, int page, int limit, String status) {
        String qs = Query.of().add("page", page).add("limit", limit).add("status", status).build();
        JavaType type = transport.mapper().getTypeFactory()
                .constructParametricType(Page.class, WebhookDelivery.class);
        return transport.request("GET", "/v1/webhooks/" + Query.enc(id) + "/deliveries" + qs, null, type);
    }

    /**
     * Fetch one delivery with its full payload and the endpoint's response.
     *
     * @param id         the webhook id.
     * @param deliveryId the delivery id.
     * @return the delivery detail.
     */
    public WebhookDeliveryDetail getDelivery(String id, String deliveryId) {
        return transport.request("GET",
                "/v1/webhooks/" + Query.enc(id) + "/deliveries/" + Query.enc(deliveryId),
                null, transport.type(WebhookDeliveryDetail.class));
    }
}
