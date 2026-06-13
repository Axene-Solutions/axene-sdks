package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;

/** A subscriber list. */
@JsonIgnoreProperties(ignoreUnknown = true)
public class ContactList {
    public String id;
    public String name;
    public String description;
    @JsonProperty("icon_seed") public String iconSeed;
    @JsonProperty("contact_count") public int contactCount;
    @JsonProperty("created_at") public String createdAt;
}
