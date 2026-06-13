package io.axene.mailer;

import java.util.List;

/** Manage reusable email templates (Starter plan and up). Accessed as {@code client.templates()}. */
public final class Templates {

    private final ApiTransport transport;

    Templates(ApiTransport transport) {
        this.transport = transport;
    }

    /**
     * List all templates, most recently updated first.
     *
     * @return the templates.
     */
    public List<Template> list() {
        return transport.request("GET", "/v1/templates/", null, transport.listType(Template.class));
    }

    /**
     * Create a template. {@code variables} are derived server-side from
     * {@code {{name}}} placeholders, so you do not pass them.
     *
     * @param params the template fields ({@code name} required).
     * @return the created template.
     */
    public Template create(TemplateParams params) {
        return transport.request("POST", "/v1/templates/", params.toWire(), transport.type(Template.class));
    }

    /**
     * Fetch a single template.
     *
     * @param id the template id.
     * @return the template.
     */
    public Template get(String id) {
        return transport.request("GET", "/v1/templates/" + Query.enc(id), null, transport.type(Template.class));
    }

    /**
     * Update a template (partial).
     *
     * @param id     the template id.
     * @param params the fields to update.
     * @return the updated template.
     */
    public Template update(String id, TemplateParams params) {
        return transport.request("PATCH", "/v1/templates/" + Query.enc(id), params.toWire(), transport.type(Template.class));
    }

    /**
     * Delete a template.
     *
     * @param id the template id.
     */
    public void delete(String id) {
        transport.request("DELETE", "/v1/templates/" + Query.enc(id), null, transport.type(Void.class));
    }

    /**
     * Duplicate a template (the copy's {@code blocks_json} is not carried over).
     *
     * @param id the template id.
     * @return the duplicated template.
     */
    public Template duplicate(String id) {
        return transport.request("POST", "/v1/templates/" + Query.enc(id) + "/duplicate", null, transport.type(Template.class));
    }
}
