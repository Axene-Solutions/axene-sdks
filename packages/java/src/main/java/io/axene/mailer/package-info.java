/**
 * Official Java client for Axene Mailer.
 *
 * <p>Professional email for Africa: send receipts, confirmations, and campaigns
 * from your own domain. Priced in KES, billed via M-Pesa.
 *
 * <p>The entry point is {@link io.axene.mailer.AxeneMailerClient}. Messages are
 * built with the fluent {@link io.axene.mailer.SendEmail#builder()}; failures
 * surface as {@link io.axene.mailer.AxeneException}.
 *
 * <pre>{@code
 * AxeneMailerClient axene = new AxeneMailerClient("axm_k_your_api_key");
 * SendEmailResult res = axene.emails().send(SendEmail.builder()
 *     .from("hello@yourdomain.com", "Your Shop")
 *     .to("customer@example.com")
 *     .subject("Your receipt")
 *     .html("<p>Thanks for your order.</p>")
 *     .build());
 * }</pre>
 */
package io.axene.mailer;
