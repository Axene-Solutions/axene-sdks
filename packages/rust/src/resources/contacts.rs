//! The `contacts` resource: subscriber lists, contacts, CSV imports, bulk sends.

use reqwest::Method;

use super::{query, urlencode};
use crate::error::Result;
use crate::http::HttpTransport;
use crate::models::{
    AddContact, BulkSend, BulkSendResult, Contact, ContactList, ContactListDetail, CreateList,
    CsvImportResult, UpdateList,
};

/// Accessed as `client.contacts()`.
#[derive(Debug, Clone)]
pub struct Contacts {
    http: HttpTransport,
}

impl Contacts {
    pub(crate) fn new(http: HttpTransport) -> Self {
        Self { http }
    }

    /// List all subscriber lists in the active workspace.
    pub async fn list_lists(&self) -> Result<Vec<ContactList>> {
        self.http.request(Method::GET, "/v1/contacts/").await
    }

    /// Create a subscriber list.
    pub async fn create_list(&self, params: &CreateList) -> Result<ContactList> {
        self.http
            .request_json(Method::POST, "/v1/contacts/", params)
            .await
    }

    /// Get a list with a page of its contacts (zero-based `page`).
    pub async fn get_list(
        &self,
        id: &str,
        page: Option<u64>,
        limit: Option<u64>,
    ) -> Result<ContactListDetail> {
        let q = query(&[
            ("page", page.map(|p| p.to_string())),
            ("limit", limit.map(|l| l.to_string())),
        ]);
        self.http
            .request(Method::GET, &format!("/v1/contacts/{}{q}", urlencode(id)))
            .await
    }

    /// Update a list's name, description, or icon (partial).
    pub async fn update_list(&self, id: &str, params: &UpdateList) -> Result<ContactList> {
        self.http
            .request_json(
                Method::PATCH,
                &format!("/v1/contacts/{}", urlencode(id)),
                params,
            )
            .await
    }

    /// Delete a list and all of its contacts.
    pub async fn delete_list(&self, id: &str) -> Result<()> {
        self.http
            .request_empty(Method::DELETE, &format!("/v1/contacts/{}", urlencode(id)))
            .await
    }

    /// Add a single contact to a list.
    pub async fn add_contact(&self, list_id: &str, params: &AddContact) -> Result<Contact> {
        self.http
            .request_json(
                Method::POST,
                &format!("/v1/contacts/{}/contacts", urlencode(list_id)),
                params,
            )
            .await
    }

    /// Remove a contact from a list.
    pub async fn remove_contact(&self, list_id: &str, contact_id: &str) -> Result<()> {
        self.http
            .request_empty(
                Method::DELETE,
                &format!(
                    "/v1/contacts/{}/contacts/{}",
                    urlencode(list_id),
                    urlencode(contact_id)
                ),
            )
            .await
    }

    /// Import contacts from a CSV file (header row required). Sent as
    /// `multipart/form-data` under the field `file`.
    pub async fn upload_csv(
        &self,
        list_id: &str,
        file: Vec<u8>,
        filename: &str,
    ) -> Result<CsvImportResult> {
        self.http
            .upload(
                &format!("/v1/contacts/{}/upload", urlencode(list_id)),
                file,
                filename,
            )
            .await
    }

    /// Send a templated email to every contact in a list. The `contact_list_id`
    /// field is set automatically to `list_id`.
    pub async fn bulk_send(&self, list_id: &str, params: &BulkSend) -> Result<BulkSendResult> {
        let mut body = params.clone();
        body.contact_list_id = list_id.to_string();
        self.http
            .request_json(
                Method::POST,
                &format!("/v1/contacts/{}/send", urlencode(list_id)),
                &body,
            )
            .await
    }
}
