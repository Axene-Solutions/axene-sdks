package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;

/** Result of {@code webhooks.test}: confirmation that a sample delivery was queued. */
@JsonIgnoreProperties(ignoreUnknown = true)
public final class WebhookTestResult {
    public boolean queued;
    public String url;
}
