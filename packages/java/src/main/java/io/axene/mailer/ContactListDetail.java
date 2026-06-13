package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;

import java.util.List;

/** A contact list with a page of its contacts, from {@code contacts.getList}. */
@JsonIgnoreProperties(ignoreUnknown = true)
public final class ContactListDetail extends ContactList {
    public List<Contact> contacts;
}
