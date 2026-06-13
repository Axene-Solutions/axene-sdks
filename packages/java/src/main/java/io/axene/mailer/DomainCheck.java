package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;

/** Result of {@code domains.check}: whether a domain name already exists in your account. */
@JsonIgnoreProperties(ignoreUnknown = true)
public final class DomainCheck {
    public boolean exists;
    public boolean verified;
    public String status;
    public String domain;
    public String id;
}
