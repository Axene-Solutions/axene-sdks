package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;

import java.util.Map;

/** A webhook delivery with the full payload and the endpoint's response. */
@JsonIgnoreProperties(ignoreUnknown = true)
public final class WebhookDeliveryDetail extends WebhookDelivery {
    public Map<String, Object> payload;
    @JsonProperty("response_body") public String responseBody;
    @JsonProperty("endpoint_url") public String endpointUrl;
}
