/**
 * The `templates` resource: reusable email templates. Starter plan and up.
 * @module
 */
import type { HttpTransport } from '../http';
import { prune } from '../internal/serialize';
import type { CreateTemplateParams, Template, UpdateTemplateParams } from '../types';

/** Accessed as `axene.templates`. */
export class Templates {
  /** @internal */
  constructor(private readonly http: HttpTransport) {}

  /** List all templates, most recently updated first. */
  list(): Promise<Template[]> {
    return this.http.request<Template[]>('GET', '/v1/templates/');
  }

  /**
   * Create a template. `variables` are derived server-side from `{{name}}`
   * placeholders in the bodies, so you do not pass them.
   */
  create(params: CreateTemplateParams): Promise<Template> {
    return this.http.request<Template>(
      'POST',
      '/v1/templates/',
      prune({
        name: params.name,
        subject: params.subject,
        html_body: params.html,
        text_body: params.text,
        blocks_json: params.blocksJson,
      }),
    );
  }

  /** Fetch a single template. */
  get(id: string): Promise<Template> {
    return this.http.request<Template>('GET', `/v1/templates/${encodeURIComponent(id)}`);
  }

  /** Update a template (partial). */
  update(id: string, params: UpdateTemplateParams): Promise<Template> {
    return this.http.request<Template>(
      'PATCH',
      `/v1/templates/${encodeURIComponent(id)}`,
      prune({
        name: params.name,
        subject: params.subject,
        html_body: params.html,
        text_body: params.text,
        blocks_json: params.blocksJson,
      }),
    );
  }

  /** Delete a template. */
  delete(id: string): Promise<void> {
    return this.http.request<void>('DELETE', `/v1/templates/${encodeURIComponent(id)}`);
  }

  /** Duplicate a template (the copy's `blocks_json` is not carried over). */
  duplicate(id: string): Promise<Template> {
    return this.http.request<Template>('POST', `/v1/templates/${encodeURIComponent(id)}/duplicate`);
  }
}
