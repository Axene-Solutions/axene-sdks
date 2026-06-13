package axene

import (
	"context"
	"net/url"
	"strconv"
)

// Contacts is the contacts resource, reached as client.Contacts.
type Contacts struct {
	http *transport
}

// ListLists returns all subscriber lists in the active workspace.
func (c *Contacts) ListLists(ctx context.Context) ([]ContactList, error) {
	var out []ContactList
	if err := c.http.doRequest(ctx, "GET", "/v1/contacts/", nil, &out); err != nil {
		return nil, err
	}
	return out, nil
}

// CreateList creates a subscriber list.
func (c *Contacts) CreateList(ctx context.Context, params CreateListParams) (*ContactList, error) {
	var out ContactList
	if err := c.http.doRequest(ctx, "POST", "/v1/contacts/", params, &out); err != nil {
		return nil, err
	}
	return &out, nil
}

// GetList gets a list with a page of its contacts (zero-based page).
func (c *Contacts) GetList(ctx context.Context, id string, params ListContactsParams) (*ContactListDetail, error) {
	q := url.Values{}
	q.Set("page", strconv.Itoa(params.Page))
	if params.Limit > 0 {
		q.Set("limit", strconv.Itoa(params.Limit))
	}
	var out ContactListDetail
	if err := c.http.doRequest(ctx, "GET", "/v1/contacts/"+url.PathEscape(id)+"?"+q.Encode(), nil, &out); err != nil {
		return nil, err
	}
	return &out, nil
}

// UpdateList updates a list's name, description, or icon (partial).
func (c *Contacts) UpdateList(ctx context.Context, id string, params UpdateListParams) (*ContactList, error) {
	var out ContactList
	if err := c.http.doRequest(ctx, "PATCH", "/v1/contacts/"+url.PathEscape(id), params, &out); err != nil {
		return nil, err
	}
	return &out, nil
}

// DeleteList deletes a list and all of its contacts.
func (c *Contacts) DeleteList(ctx context.Context, id string) error {
	return c.http.doRequest(ctx, "DELETE", "/v1/contacts/"+url.PathEscape(id), nil, nil)
}

// AddContact adds a single contact to a list.
func (c *Contacts) AddContact(ctx context.Context, listID string, params AddContactParams) (*Contact, error) {
	var out Contact
	if err := c.http.doRequest(ctx, "POST", "/v1/contacts/"+url.PathEscape(listID)+"/contacts", params, &out); err != nil {
		return nil, err
	}
	return &out, nil
}

// RemoveContact removes a contact from a list.
func (c *Contacts) RemoveContact(ctx context.Context, listID, contactID string) error {
	return c.http.doRequest(ctx, "DELETE", "/v1/contacts/"+url.PathEscape(listID)+"/contacts/"+url.PathEscape(contactID), nil, nil)
}

// UploadCSV imports contacts from a CSV file (header row required). The upload
// is sent as multipart/form-data under the field name "file".
func (c *Contacts) UploadCSV(ctx context.Context, listID string, file []byte, filename string) (*CsvImportResult, error) {
	if filename == "" {
		filename = "contacts.csv"
	}
	var out CsvImportResult
	if err := c.http.upload(ctx, "/v1/contacts/"+url.PathEscape(listID)+"/upload", file, filename, &out); err != nil {
		return nil, err
	}
	return &out, nil
}

// BulkSend sends a templated email to every contact in a list. The list id is
// injected as contact_list_id automatically. Subject/HTML/Text may use
// {{email}}, {{name}}, and {{metadata_key}} placeholders.
func (c *Contacts) BulkSend(ctx context.Context, listID string, params BulkSendParams) (*BulkSendResult, error) {
	params.ContactListID = listID
	var out BulkSendResult
	if err := c.http.doRequest(ctx, "POST", "/v1/contacts/"+url.PathEscape(listID)+"/send", params, &out); err != nil {
		return nil, err
	}
	return &out, nil
}
