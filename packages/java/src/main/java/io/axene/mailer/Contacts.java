package io.axene.mailer;

import java.util.List;

/** Manage subscriber lists, their contacts, CSV imports, and templated bulk sends. Accessed as {@code client.contacts()}. */
public final class Contacts {

    private final ApiTransport transport;

    Contacts(ApiTransport transport) {
        this.transport = transport;
    }

    /**
     * List all subscriber lists in the active workspace.
     *
     * @return the lists.
     */
    public List<ContactList> listLists() {
        return transport.request("GET", "/v1/contacts/", null, transport.listType(ContactList.class));
    }

    /**
     * Create a subscriber list.
     *
     * @param params the list fields ({@code name} required).
     * @return the created list.
     */
    public ContactList createList(ContactListParams params) {
        return transport.request("POST", "/v1/contacts/", params.toWire(), transport.type(ContactList.class));
    }

    /**
     * Get a list with a page of its contacts (zero-based {@code page}).
     *
     * @param id    the list id.
     * @param page  zero-based page index.
     * @param limit page size (1-200).
     * @return the list with a page of contacts.
     */
    public ContactListDetail getList(String id, int page, int limit) {
        String qs = Query.of().add("page", page).add("limit", limit).build();
        return transport.request("GET", "/v1/contacts/" + Query.enc(id) + qs, null, transport.type(ContactListDetail.class));
    }

    /**
     * Update a list's name, description, or icon (partial).
     *
     * @param id     the list id.
     * @param params the fields to update.
     * @return the updated list.
     */
    public ContactList updateList(String id, ContactListParams params) {
        return transport.request("PATCH", "/v1/contacts/" + Query.enc(id), params.toWire(), transport.type(ContactList.class));
    }

    /**
     * Delete a list and all of its contacts.
     *
     * @param id the list id.
     */
    public void deleteList(String id) {
        transport.request("DELETE", "/v1/contacts/" + Query.enc(id), null, transport.type(Void.class));
    }

    /**
     * Add a single contact to a list.
     *
     * @param listId the list id.
     * @param params the contact fields ({@code email} required).
     * @return the created contact.
     */
    public Contact addContact(String listId, ContactParams params) {
        return transport.request("POST", "/v1/contacts/" + Query.enc(listId) + "/contacts", params.toWire(), transport.type(Contact.class));
    }

    /**
     * Remove a contact from a list.
     *
     * @param listId    the list id.
     * @param contactId the contact id.
     */
    public void removeContact(String listId, String contactId) {
        transport.request("DELETE", "/v1/contacts/" + Query.enc(listId) + "/contacts/" + Query.enc(contactId), null, transport.type(Void.class));
    }

    /**
     * Import contacts from a CSV file (header row required). The email column is
     * auto-detected; other columns become contact metadata.
     *
     * @param listId   the list id.
     * @param file     the raw CSV bytes.
     * @param filename the filename to advertise.
     * @return the import result.
     */
    public CsvImportResult uploadCsv(String listId, byte[] file, String filename) {
        return transport.upload("/v1/contacts/" + Query.enc(listId) + "/upload", file, filename, transport.type(CsvImportResult.class));
    }

    /**
     * Send a templated email to every contact in a list. The {@code contact_list_id}
     * field is set automatically from {@code listId}.
     *
     * @param listId the list id.
     * @param params the bulk-send fields.
     * @return the bulk-send result.
     */
    public BulkSendResult bulkSend(String listId, BulkSendParams params) {
        return transport.request("POST", "/v1/contacts/" + Query.enc(listId) + "/send", params.toWire(listId), transport.type(BulkSendResult.class));
    }
}
