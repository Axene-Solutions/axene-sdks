package axene

// Address is a recipient or sender. Email is required; Name is optional.
// Use Addr to build one from a bare email string.
type Address struct {
	Email string `json:"email"`
	Name  string `json:"name,omitempty"`
}

// Addr is a helper that builds an Address from a bare email string.
func Addr(email string) Address {
	return Address{Email: email}
}

// Attachment is a file attached to an email. ContentBase64 is the raw base64
// content with no "data:" prefix.
type Attachment struct {
	Filename      string `json:"filename"`
	ContentBase64 string `json:"content_base64"`
	ContentType   string `json:"content_type,omitempty"`
}

// SendEmail is the body for Emails.Send, Emails.SendBatch, and Emails.Validate.
// The From field serializes to the wire key "from_" (trailing underscore).
type SendEmail struct {
	// From is the sender address. It must be on a verified domain.
	From Address `json:"from_"`
	// To is one or more recipients.
	To []Address `json:"to"`
	// Subject is the email subject line.
	Subject string `json:"subject"`
	// HTML is the HTML body. Provide HTML, Text, or both.
	HTML string `json:"html,omitempty"`
	// Text is the plain-text body. Provide HTML, Text, or both.
	Text string `json:"text,omitempty"`
	// CC is an optional list of carbon-copy recipients.
	CC []Address `json:"cc,omitempty"`
	// BCC is an optional list of blind-carbon-copy recipients.
	BCC []Address `json:"bcc,omitempty"`
	// ReplyTo overrides the reply-to address.
	ReplyTo *Address `json:"reply_to,omitempty"`
	// Headers are custom headers to attach to the message.
	Headers map[string]string `json:"headers,omitempty"`
	// Tags label the message for filtering and analytics.
	Tags []string `json:"tags,omitempty"`
	// SendAt schedules delivery for later (ISO 8601). Starter plan and up.
	SendAt string `json:"send_at,omitempty"`
	// Attachments are files to attach to the message.
	Attachments []Attachment `json:"attachments,omitempty"`
}

// SendResult is the result of a send: the queued message id and its status.
type SendResult struct {
	ID              string `json:"id"`
	Status          string `json:"status"`
	MessageID       string `json:"message_id,omitempty"`
	RejectionReason string `json:"rejection_reason,omitempty"`
}

// BatchResult is the result of Emails.SendBatch.
type BatchResult struct {
	Total   int          `json:"total"`
	Sent    int          `json:"sent"`
	Failed  int          `json:"failed"`
	Results []SendResult `json:"results"`
}

// EmailEvent is a delivery, open, click, or bounce event for a message.
type EmailEvent struct {
	ID        string         `json:"id"`
	EventType string         `json:"event_type"`
	Metadata  map[string]any `json:"metadata,omitempty"`
	CreatedAt string         `json:"created_at"`
}

// Email is a stored email and its current status.
type Email struct {
	ID           string   `json:"id"`
	FromAddress  string   `json:"from_address"`
	ToAddresses  []string `json:"to_addresses"`
	Subject      string   `json:"subject,omitempty"`
	Status       string   `json:"status"`
	Source       string   `json:"source,omitempty"`
	OpenedCount  int      `json:"opened_count"`
	ClickedCount int      `json:"clicked_count"`
	Tags         []string `json:"tags,omitempty"`
	ScheduledAt  string   `json:"scheduled_at,omitempty"`
	CreatedAt    string   `json:"created_at,omitempty"`
	SentAt       string   `json:"sent_at,omitempty"`
	DeliveredAt  string   `json:"delivered_at,omitempty"`
	RetryOfID    string   `json:"retry_of_id,omitempty"`
}

// EmailDetail is a stored email with its bodies and events, from Emails.Get.
type EmailDetail struct {
	Email
	CCAddresses  []string       `json:"cc_addresses,omitempty"`
	BCCAddresses []string       `json:"bcc_addresses,omitempty"`
	TextBody     string         `json:"text_body,omitempty"`
	HTMLBody     string         `json:"html_body,omitempty"`
	Headers      map[string]any `json:"headers,omitempty"`
	MessageID    string         `json:"message_id,omitempty"`
	Events       []EmailEvent   `json:"events"`
}

// EmailSearchHit is a search result row from Emails.Search.
type EmailSearchHit struct {
	ID          string   `json:"id"`
	FromAddress string   `json:"from_address"`
	ToAddresses []string `json:"to_addresses"`
	Subject     string   `json:"subject,omitempty"`
	Status      string   `json:"status"`
	Tags        []string `json:"tags,omitempty"`
	Source      string   `json:"source,omitempty"`
	CreatedAt   string   `json:"created_at,omitempty"`
	DeliveredAt string   `json:"delivered_at,omitempty"`
}

// ScheduledEmail is an email awaiting future delivery.
type ScheduledEmail struct {
	ID               string   `json:"id"`
	FromAddress      string   `json:"from_address"`
	ToAddresses      []string `json:"to_addresses"`
	Subject          string   `json:"subject,omitempty"`
	Status           string   `json:"status"`
	Tags             []string `json:"tags,omitempty"`
	ScheduledAt      string   `json:"scheduled_at,omitempty"`
	SecondsUntilSend int      `json:"seconds_until_send"`
	CreatedAt        string   `json:"created_at,omitempty"`
}

// ValidationIssue is a single reason a message would not send.
type ValidationIssue struct {
	Field string `json:"field"`
	Error string `json:"error"`
}

// ValidationUsage is the sending-quota usage returned alongside a validation.
type ValidationUsage struct {
	Daily        int `json:"daily"`
	DailyLimit   int `json:"daily_limit"`
	Monthly      int `json:"monthly"`
	MonthlyLimit int `json:"monthly_limit"`
}

// ValidationResult is the result of Emails.Validate, a dry-run that never sends.
type ValidationResult struct {
	Valid   bool              `json:"valid"`
	CanSend bool              `json:"can_send"`
	Issues  []ValidationIssue `json:"issues"`
	Plan    string            `json:"plan"`
	Usage   ValidationUsage   `json:"usage"`
}

// IDStatus is a minimal {id, status} response (cancel/send-now scheduled).
type IDStatus struct {
	ID     string `json:"id"`
	Status string `json:"status"`
}

// SavedSearch is one stored email search. The server normalizes its fields.
type SavedSearch struct {
	ID     string `json:"id,omitempty"`
	Name   string `json:"name,omitempty"`
	Query  string `json:"query,omitempty"`
	Range  string `json:"range,omitempty"`
	Status string `json:"status,omitempty"`
	Domain string `json:"domain,omitempty"`
	Source string `json:"source,omitempty"`
}

// -- domains ----------------------------------------------------------------

// DomainListItem is a row from Domains.List.
type DomainListItem struct {
	ID              string `json:"id"`
	Name            string `json:"name"`
	Status          string `json:"status"`
	CreatedAt       string `json:"created_at,omitempty"`
	PlatformWarning string `json:"platform_warning,omitempty"`
}

// DnsRecord is a DNS record the API expects you to publish for a domain.
type DnsRecord struct {
	ID            string `json:"id"`
	RecordType    string `json:"record_type"`
	Purpose       string `json:"purpose"`
	Host          string `json:"host"`
	Value         string `json:"value"`
	IsVerified    bool   `json:"is_verified"`
	LastCheckedAt string `json:"last_checked_at,omitempty"`
}

// Domain is a sending domain with its DKIM selector and DNS records.
type Domain struct {
	ID              string      `json:"id"`
	Name            string      `json:"name"`
	Status          string      `json:"status"`
	DkimSelector    string      `json:"dkim_selector"`
	VerifiedAt      string      `json:"verified_at,omitempty"`
	CreatedAt       string      `json:"created_at,omitempty"`
	DnsRecords      []DnsRecord `json:"dns_records"`
	PlatformWarning string      `json:"platform_warning,omitempty"`
}

// DomainHealthRecord is the optional DNS record on a health check row.
type DomainHealthRecord struct {
	Type  string `json:"type"`
	Host  string `json:"host"`
	Value string `json:"value"`
}

// DomainHealthCheck is one row of a domain health report.
type DomainHealthCheck struct {
	Key            string              `json:"key"`
	Label          string              `json:"label"`
	Status         string              `json:"status"`
	Detail         string              `json:"detail"`
	Recommendation string              `json:"recommendation,omitempty"`
	Record         *DomainHealthRecord `json:"record,omitempty"`
}

// DomainHealthSummary is the tally of check statuses in a health report.
type DomainHealthSummary struct {
	OK    int `json:"ok"`
	Warn  int `json:"warn"`
	Error int `json:"error"`
	Info  int `json:"info"`
}

// DomainHealth is the result of Domains.Health.
type DomainHealth struct {
	Domain  string              `json:"domain"`
	Checks  []DomainHealthCheck `json:"checks"`
	Summary DomainHealthSummary `json:"summary"`
}

// DomainDiagnosis is the result of Domains.Diagnose. Issue shapes vary.
type DomainDiagnosis struct {
	Domain      string           `json:"domain"`
	Issues      []map[string]any `json:"issues"`
	HealthScore int              `json:"health_score"`
}

// DkimRotation is the result of Domains.RotateDkim: the new record and domain.
type DkimRotation struct {
	DkimRecordHost  string `json:"dkim_record_host"`
	DkimRecordValue string `json:"dkim_record_value"`
	Domain          Domain `json:"domain"`
}

// DomainTransfer is a domain transfer record returned by Domains.Transfer.
type DomainTransfer struct {
	ID           string `json:"id"`
	DomainID     string `json:"domain_id"`
	DomainName   string `json:"domain_name,omitempty"`
	SourceUserID string `json:"source_user_id,omitempty"`
	SourceOrgID  string `json:"source_org_id,omitempty"`
	SourceLabel  string `json:"source_label,omitempty"`
	TargetEmail  string `json:"target_email"`
	TargetUserID string `json:"target_user_id,omitempty"`
	TargetOrgID  string `json:"target_org_id,omitempty"`
	Status       string `json:"status"`
	Note         string `json:"note,omitempty"`
	CooloffUntil string `json:"cooloff_until,omitempty"`
	InitiatedAt  string `json:"initiated_at"`
	AcceptedAt   string `json:"accepted_at,omitempty"`
	CompletedAt  string `json:"completed_at,omitempty"`
	ExpiresAt    string `json:"expires_at"`
}

// TransferParams is the body for Domains.Transfer.
type TransferParams struct {
	TargetEmail string `json:"target_email"`
	Note        string `json:"note,omitempty"`
}

// DomainAvailability is the result of Domains.CheckAvailability.
type DomainAvailability struct {
	Available   bool   `json:"available"`
	Reason      string `json:"reason,omitempty"`
	Detail      string `json:"detail,omitempty"`
	StaleTokens *int   `json:"stale_tokens,omitempty"`
}

// DomainCheck is the result of Domains.Check.
type DomainCheck struct {
	Exists   bool   `json:"exists"`
	Verified bool   `json:"verified"`
	Status   string `json:"status,omitempty"`
	Domain   string `json:"domain"`
	ID       string `json:"id,omitempty"`
}

// -- contacts ---------------------------------------------------------------

// ContactList is a subscriber list.
type ContactList struct {
	ID           string `json:"id"`
	Name         string `json:"name"`
	Description  string `json:"description,omitempty"`
	IconSeed     string `json:"icon_seed,omitempty"`
	ContactCount int    `json:"contact_count"`
	CreatedAt    string `json:"created_at"`
}

// Contact is a single contact in a list.
type Contact struct {
	ID        string         `json:"id"`
	Email     string         `json:"email"`
	Name      string         `json:"name,omitempty"`
	Metadata  map[string]any `json:"metadata,omitempty"`
	CreatedAt string         `json:"created_at"`
}

// ContactListDetail is a contact list with a page of its contacts.
type ContactListDetail struct {
	ContactList
	Contacts []Contact `json:"contacts"`
}

// CreateListParams is the body for Contacts.CreateList. IconSeed maps to
// the wire field icon_seed. Use pointers so unset fields are omitted.
type CreateListParams struct {
	Name        string  `json:"name"`
	Description *string `json:"description,omitempty"`
	IconSeed    *string `json:"icon_seed,omitempty"`
}

// UpdateListParams is the partial body for Contacts.UpdateList.
type UpdateListParams struct {
	Name        *string `json:"name,omitempty"`
	Description *string `json:"description,omitempty"`
	IconSeed    *string `json:"icon_seed,omitempty"`
}

// AddContactParams is the body for Contacts.AddContact.
type AddContactParams struct {
	Email    string         `json:"email"`
	Name     *string        `json:"name,omitempty"`
	Metadata map[string]any `json:"metadata,omitempty"`
}

// CsvImportResult is the result of Contacts.UploadCSV.
type CsvImportResult struct {
	Imported int      `json:"imported"`
	Skipped  int      `json:"skipped"`
	Errors   []string `json:"errors"`
}

// BulkSendParams is the body for Contacts.BulkSend. ContactListID is injected
// automatically from the list id by BulkSend, so callers leave it unset.
type BulkSendParams struct {
	ContactListID   string   `json:"contact_list_id,omitempty"`
	SenderAddressID string   `json:"sender_address_id"`
	Subject         string   `json:"subject"`
	HTML            string   `json:"html,omitempty"`
	Text            string   `json:"text,omitempty"`
	Tags            []string `json:"tags,omitempty"`
}

// BulkSendResult is the result of Contacts.BulkSend.
type BulkSendResult struct {
	Queued  int      `json:"queued"`
	Skipped int      `json:"skipped"`
	Errors  []string `json:"errors"`
}

// -- suppressions -----------------------------------------------------------

// Suppression is a suppressed recipient address.
type Suppression struct {
	ID           string `json:"id"`
	EmailAddress string `json:"email_address"`
	Reason       string `json:"reason"`
	CreatedAt    string `json:"created_at,omitempty"`
}

// AddSuppressionParams is the body for Suppressions.Add. Email maps to the
// wire field email_address.
type AddSuppressionParams struct {
	Email  string `json:"email_address"`
	Reason string `json:"reason,omitempty"`
}

// BulkSuppressionResult is the result of Suppressions.BulkUpload.
type BulkSuppressionResult struct {
	Added          int `json:"added"`
	Skipped        int `json:"skipped"`
	TotalProcessed int `json:"total_processed"`
}

// -- templates --------------------------------------------------------------

// Template is a reusable email template. Variables is derived server-side and
// is read-only.
type Template struct {
	ID         string         `json:"id"`
	Name       string         `json:"name"`
	Subject    string         `json:"subject,omitempty"`
	HTMLBody   string         `json:"html_body,omitempty"`
	TextBody   string         `json:"text_body,omitempty"`
	Variables  []string       `json:"variables,omitempty"`
	BlocksJSON map[string]any `json:"blocks_json,omitempty"`
	CreatedAt  string         `json:"created_at"`
	UpdatedAt  string         `json:"updated_at"`
}

// CreateTemplateParams is the body for Templates.Create. HTML maps to
// html_body and Text maps to text_body.
type CreateTemplateParams struct {
	Name       string         `json:"name"`
	Subject    *string        `json:"subject,omitempty"`
	HTML       *string        `json:"html_body,omitempty"`
	Text       *string        `json:"text_body,omitempty"`
	BlocksJSON map[string]any `json:"blocks_json,omitempty"`
}

// UpdateTemplateParams is the partial body for Templates.Update.
type UpdateTemplateParams struct {
	Name       *string        `json:"name,omitempty"`
	Subject    *string        `json:"subject,omitempty"`
	HTML       *string        `json:"html_body,omitempty"`
	Text       *string        `json:"text_body,omitempty"`
	BlocksJSON map[string]any `json:"blocks_json,omitempty"`
}

// -- webhooks ---------------------------------------------------------------

// Webhook is a configured webhook endpoint. Secret is returned in plaintext.
type Webhook struct {
	ID        string   `json:"id"`
	URL       string   `json:"url"`
	Events    []string `json:"events"`
	Secret    string   `json:"secret"`
	IsActive  bool     `json:"is_active"`
	CreatedAt string   `json:"created_at"`
}

// CreateWebhookParams is the body for Webhooks.Create.
type CreateWebhookParams struct {
	URL    string   `json:"url"`
	Events []string `json:"events"`
}

// UpdateWebhookParams is the partial body for Webhooks.Update. IsActive maps
// to the wire field is_active.
type UpdateWebhookParams struct {
	URL      *string  `json:"url,omitempty"`
	Events   []string `json:"events,omitempty"`
	IsActive *bool    `json:"is_active,omitempty"`
}

// WebhookTestResult is the result of Webhooks.Test.
type WebhookTestResult struct {
	Queued bool   `json:"queued"`
	URL    string `json:"url"`
}

// WebhookDelivery is a summary of one webhook delivery attempt.
type WebhookDelivery struct {
	ID             string `json:"id"`
	WebhookID      string `json:"webhook_id"`
	EventType      string `json:"event_type,omitempty"`
	Status         string `json:"status"`
	ResponseStatus *int   `json:"response_status,omitempty"`
	Attempt        int    `json:"attempt"`
	NextRetryAt    string `json:"next_retry_at,omitempty"`
	CreatedAt      string `json:"created_at,omitempty"`
}

// WebhookDeliveryDetail is a delivery with the full payload and endpoint response.
type WebhookDeliveryDetail struct {
	WebhookDelivery
	Payload      map[string]any `json:"payload"`
	ResponseBody string         `json:"response_body,omitempty"`
	EndpointURL  string         `json:"endpoint_url"`
}

// -- pagination -------------------------------------------------------------

// Page is the envelope returned by endpoints that paginate with a total count:
// suppressions list and webhook deliveries.
type Page[T any] struct {
	Items []T `json:"items"`
	Total int `json:"total"`
	Page  int `json:"page"`
	Limit int `json:"limit"`
}

// -- list/query parameter structs -------------------------------------------

// ListEmailsParams are the query parameters for Emails.List.
type ListEmailsParams struct {
	Status string
	Page   int
	Limit  int
}

// SearchEmailsParams are the query parameters for Emails.Search.
type SearchEmailsParams struct {
	Q      string
	Status string
	Tag    string
	Page   int
	Limit  int
}

// ListContactsParams are the query parameters for Contacts.GetList.
type ListContactsParams struct {
	Page  int
	Limit int
}

// ListSuppressionsParams are the query parameters for Suppressions.List.
type ListSuppressionsParams struct {
	Page   int
	Limit  int
	Search string
}

// ListDeliveriesParams are the query parameters for Webhooks.ListDeliveries.
type ListDeliveriesParams struct {
	Page   int
	Limit  int
	Status string
}
