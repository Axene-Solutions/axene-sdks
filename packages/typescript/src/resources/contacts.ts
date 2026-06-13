/**
 * The `contacts` resource: manage subscriber lists, their contacts, CSV
 * imports, and templated bulk sends.
 * @module
 */
import type { HttpTransport } from '../http';
import { query } from '../internal/query';
import { prune } from '../internal/serialize';
import type {
  AddContactParams,
  BulkSendParams,
  BulkSendResult,
  Contact,
  ContactList,
  ContactListDetail,
  CreateListParams,
  CsvImportResult,
  UpdateListParams,
} from '../types';

/** Accessed as `axene.contacts`. */
export class Contacts {
  /** @internal */
  constructor(private readonly http: HttpTransport) {}

  /** List all subscriber lists in the active workspace. */
  listLists(): Promise<ContactList[]> {
    return this.http.request<ContactList[]>('GET', '/v1/contacts/');
  }

  /** Create a subscriber list. */
  createList(params: CreateListParams): Promise<ContactList> {
    return this.http.request<ContactList>(
      'POST',
      '/v1/contacts/',
      prune({ name: params.name, description: params.description, icon_seed: params.iconSeed }),
    );
  }

  /** Get a list with a page of its contacts (zero-based `page`). */
  getList(id: string, params: { page?: number; limit?: number } = {}): Promise<ContactListDetail> {
    return this.http.request<ContactListDetail>('GET', `/v1/contacts/${encodeURIComponent(id)}${query(params)}`);
  }

  /** Update a list's name, description, or icon (partial). */
  updateList(id: string, params: UpdateListParams): Promise<ContactList> {
    return this.http.request<ContactList>(
      'PATCH',
      `/v1/contacts/${encodeURIComponent(id)}`,
      prune({ name: params.name, description: params.description, icon_seed: params.iconSeed }),
    );
  }

  /** Delete a list and all of its contacts. */
  deleteList(id: string): Promise<void> {
    return this.http.request<void>('DELETE', `/v1/contacts/${encodeURIComponent(id)}`);
  }

  /** Add a single contact to a list. */
  addContact(listId: string, params: AddContactParams): Promise<Contact> {
    return this.http.request<Contact>(
      'POST',
      `/v1/contacts/${encodeURIComponent(listId)}/contacts`,
      prune({ email: params.email, name: params.name, metadata: params.metadata }),
    );
  }

  /** Remove a contact from a list. */
  removeContact(listId: string, contactId: string): Promise<void> {
    return this.http.request<void>(
      'DELETE',
      `/v1/contacts/${encodeURIComponent(listId)}/contacts/${encodeURIComponent(contactId)}`,
    );
  }

  /**
   * Import contacts from a CSV file (header row required). The email column is
   * auto-detected; other columns become contact metadata.
   */
  uploadCsv(listId: string, file: Uint8Array, filename = 'contacts.csv'): Promise<CsvImportResult> {
    return this.http.upload<CsvImportResult>(`/v1/contacts/${encodeURIComponent(listId)}/upload`, file, filename);
  }

  /**
   * Send a templated email to every contact in a list. Subject/html/text may
   * use `{{email}}`, `{{name}}`, and `{{metadata_key}}` placeholders.
   */
  bulkSend(listId: string, params: BulkSendParams): Promise<BulkSendResult> {
    return this.http.request<BulkSendResult>(
      'POST',
      `/v1/contacts/${encodeURIComponent(listId)}/send`,
      prune({
        contact_list_id: listId,
        sender_address_id: params.senderAddressId,
        subject: params.subject,
        html: params.html,
        text: params.text,
        tags: params.tags,
      }),
    );
  }
}
