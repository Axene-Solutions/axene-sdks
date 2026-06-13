package axene

import (
	"context"
	"net/url"
)

// Domains is the domains resource, reached as client.Domains.
type Domains struct {
	http *transport
}

// List returns your sending domains and their verification status.
func (d *Domains) List(ctx context.Context) ([]DomainListItem, error) {
	var out []DomainListItem
	if err := d.http.doRequest(ctx, "GET", "/v1/domains/", nil, &out); err != nil {
		return nil, err
	}
	return out, nil
}

// Create registers a new sending domain and returns the DNS records to publish.
func (d *Domains) Create(ctx context.Context, name string) (*Domain, error) {
	var out Domain
	if err := d.http.doRequest(ctx, "POST", "/v1/domains/", map[string]string{"name": name}, &out); err != nil {
		return nil, err
	}
	return &out, nil
}

// Get fetches a domain with its DKIM selector and DNS records.
func (d *Domains) Get(ctx context.Context, id string) (*Domain, error) {
	var out Domain
	if err := d.http.doRequest(ctx, "GET", "/v1/domains/"+url.PathEscape(id), nil, &out); err != nil {
		return nil, err
	}
	return &out, nil
}

// Delete deletes a domain.
func (d *Domains) Delete(ctx context.Context, id string) error {
	return d.http.doRequest(ctx, "DELETE", "/v1/domains/"+url.PathEscape(id), nil, nil)
}

// Verify re-checks DNS and verifies the domain.
func (d *Domains) Verify(ctx context.Context, id string) (*Domain, error) {
	var out Domain
	if err := d.http.doRequest(ctx, "POST", "/v1/domains/"+url.PathEscape(id)+"/verify", nil, &out); err != nil {
		return nil, err
	}
	return &out, nil
}

// Health runs live DNS health checks (DKIM, SPF, DMARC, return-path, MX).
func (d *Domains) Health(ctx context.Context, id string) (*DomainHealth, error) {
	var out DomainHealth
	if err := d.http.doRequest(ctx, "GET", "/v1/domains/"+url.PathEscape(id)+"/health", nil, &out); err != nil {
		return nil, err
	}
	return &out, nil
}

// Diagnose diagnoses configuration issues and returns a health score.
func (d *Domains) Diagnose(ctx context.Context, id string) (*DomainDiagnosis, error) {
	var out DomainDiagnosis
	if err := d.http.doRequest(ctx, "GET", "/v1/domains/"+url.PathEscape(id)+"/diagnose", nil, &out); err != nil {
		return nil, err
	}
	return &out, nil
}

// MxStatus returns the current MX status (shape varies by provider).
func (d *Domains) MxStatus(ctx context.Context, id string) (map[string]any, error) {
	var out map[string]any
	if err := d.http.doRequest(ctx, "GET", "/v1/domains/"+url.PathEscape(id)+"/mx-status", nil, &out); err != nil {
		return nil, err
	}
	return out, nil
}

// PublishedRecords returns the values currently published in DNS for each of
// the domain's records (an open map).
func (d *Domains) PublishedRecords(ctx context.Context, id string) (map[string]any, error) {
	var out map[string]any
	if err := d.http.doRequest(ctx, "GET", "/v1/domains/"+url.PathEscape(id)+"/published-records", nil, &out); err != nil {
		return nil, err
	}
	return out, nil
}

// RotateDkim rotates the domain's DKIM key, returning the new record to publish.
func (d *Domains) RotateDkim(ctx context.Context, id string) (*DkimRotation, error) {
	var out DkimRotation
	if err := d.http.doRequest(ctx, "POST", "/v1/domains/"+url.PathEscape(id)+"/rotate-dkim", nil, &out); err != nil {
		return nil, err
	}
	return &out, nil
}

// Transfer initiates a transfer of this domain to another Axene account.
func (d *Domains) Transfer(ctx context.Context, id string, params TransferParams) (*DomainTransfer, error) {
	var out DomainTransfer
	if err := d.http.doRequest(ctx, "POST", "/v1/domains/"+url.PathEscape(id)+"/transfer", params, &out); err != nil {
		return nil, err
	}
	return &out, nil
}

// CheckAvailability checks whether a domain name is available to add.
func (d *Domains) CheckAvailability(ctx context.Context, name string) (*DomainAvailability, error) {
	q := url.Values{}
	q.Set("name", name)
	var out DomainAvailability
	if err := d.http.doRequest(ctx, "GET", "/v1/domains/check-availability?"+q.Encode(), nil, &out); err != nil {
		return nil, err
	}
	return &out, nil
}

// Check checks whether a domain name already exists in your account.
func (d *Domains) Check(ctx context.Context, name string) (*DomainCheck, error) {
	var out DomainCheck
	if err := d.http.doRequest(ctx, "GET", "/v1/domains/check/"+url.PathEscape(name), nil, &out); err != nil {
		return nil, err
	}
	return &out, nil
}
