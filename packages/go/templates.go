package axene

import (
	"context"
	"net/url"
)

// Templates is the templates resource, reached as client.Templates. Templates
// are available on the Starter plan and up.
type Templates struct {
	http *transport
}

// List returns all templates, most recently updated first.
func (t *Templates) List(ctx context.Context) ([]Template, error) {
	var out []Template
	if err := t.http.doRequest(ctx, "GET", "/v1/templates/", nil, &out); err != nil {
		return nil, err
	}
	return out, nil
}

// Create creates a template. Variables are derived server-side from {{name}}
// placeholders, so you do not pass them.
func (t *Templates) Create(ctx context.Context, params CreateTemplateParams) (*Template, error) {
	var out Template
	if err := t.http.doRequest(ctx, "POST", "/v1/templates/", params, &out); err != nil {
		return nil, err
	}
	return &out, nil
}

// Get fetches a single template.
func (t *Templates) Get(ctx context.Context, id string) (*Template, error) {
	var out Template
	if err := t.http.doRequest(ctx, "GET", "/v1/templates/"+url.PathEscape(id), nil, &out); err != nil {
		return nil, err
	}
	return &out, nil
}

// Update updates a template (partial).
func (t *Templates) Update(ctx context.Context, id string, params UpdateTemplateParams) (*Template, error) {
	var out Template
	if err := t.http.doRequest(ctx, "PATCH", "/v1/templates/"+url.PathEscape(id), params, &out); err != nil {
		return nil, err
	}
	return &out, nil
}

// Delete deletes a template.
func (t *Templates) Delete(ctx context.Context, id string) error {
	return t.http.doRequest(ctx, "DELETE", "/v1/templates/"+url.PathEscape(id), nil, nil)
}

// Duplicate duplicates a template. The copy's blocks_json is not carried over.
func (t *Templates) Duplicate(ctx context.Context, id string) (*Template, error) {
	var out Template
	if err := t.http.doRequest(ctx, "POST", "/v1/templates/"+url.PathEscape(id)+"/duplicate", nil, &out); err != nil {
		return nil, err
	}
	return &out, nil
}
