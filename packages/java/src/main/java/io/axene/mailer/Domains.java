package io.axene.mailer;

import com.fasterxml.jackson.databind.JavaType;

import java.util.Map;

/** Register, verify, inspect, and transfer sending domains. Accessed as {@code client.domains()}. */
public final class Domains {

    private final ApiTransport transport;

    Domains(ApiTransport transport) {
        this.transport = transport;
    }

    private JavaType mapType() {
        return transport.mapper().getTypeFactory().constructMapType(
                java.util.LinkedHashMap.class, String.class, Object.class);
    }

    /**
     * List your sending domains and their verification status.
     *
     * @return the domains.
     */
    public java.util.List<DomainRecord> list() {
        return transport.request("GET", "/v1/domains/", null, transport.listType(DomainRecord.class));
    }

    /**
     * Register a new sending domain. Returns the DNS records to publish.
     *
     * @param name the domain name.
     * @return the created domain with its DNS records.
     */
    public DomainDetailRecord create(String name) {
        Map<String, Object> body = Wire.map();
        body.put("name", name);
        return transport.request("POST", "/v1/domains/", body, transport.type(DomainDetailRecord.class));
    }

    /**
     * Fetch a domain with its DKIM selector and DNS records.
     *
     * @param id the domain id.
     * @return the domain.
     */
    public DomainDetailRecord get(String id) {
        return transport.request("GET", "/v1/domains/" + Query.enc(id), null, transport.type(DomainDetailRecord.class));
    }

    /**
     * Delete a domain.
     *
     * @param id the domain id.
     */
    public void delete(String id) {
        transport.request("DELETE", "/v1/domains/" + Query.enc(id), null, transport.type(Void.class));
    }

    /**
     * Re-check DNS and verify the domain.
     *
     * @param id the domain id.
     * @return the updated domain.
     */
    public DomainDetailRecord verify(String id) {
        return transport.request("POST", "/v1/domains/" + Query.enc(id) + "/verify", null, transport.type(DomainDetailRecord.class));
    }

    /**
     * Run live DNS health checks (DKIM, SPF, DMARC, return-path, MX).
     *
     * @param id the domain id.
     * @return the health report.
     */
    public DomainHealth health(String id) {
        return transport.request("GET", "/v1/domains/" + Query.enc(id) + "/health", null, transport.type(DomainHealth.class));
    }

    /**
     * Diagnose configuration issues and get a health score.
     *
     * @param id the domain id.
     * @return the diagnosis.
     */
    public DomainDiagnosis diagnose(String id) {
        return transport.request("GET", "/v1/domains/" + Query.enc(id) + "/diagnose", null, transport.type(DomainDiagnosis.class));
    }

    /**
     * Current MX status for inbound/forwarding. The shape varies by provider, so
     * the result is returned as an open map.
     *
     * @param id the domain id.
     * @return the MX status map.
     */
    public Map<String, Object> mxStatus(String id) {
        return transport.request("GET", "/v1/domains/" + Query.enc(id) + "/mx-status", null, mapType());
    }

    /**
     * The values currently published in DNS for each of the domain's records.
     *
     * @param id the domain id.
     * @return a map of record id to published values.
     */
    public Map<String, Object> publishedRecords(String id) {
        return transport.request("GET", "/v1/domains/" + Query.enc(id) + "/published-records", null, mapType());
    }

    /**
     * Rotate the domain's DKIM key, returning the new record to publish.
     *
     * @param id the domain id.
     * @return the new DKIM record and the updated domain.
     */
    public DkimRotation rotateDkim(String id) {
        return transport.request("POST", "/v1/domains/" + Query.enc(id) + "/rotate-dkim", null, transport.type(DkimRotation.class));
    }

    /**
     * Initiate a transfer of this domain to another Axene account.
     *
     * @param id          the domain id.
     * @param targetEmail the recipient account's email.
     * @param note        an optional note, or null.
     * @return the transfer record.
     */
    public DomainTransfer transfer(String id, String targetEmail, String note) {
        Map<String, Object> body = Wire.map();
        body.put("target_email", targetEmail);
        body.put("note", note);
        return transport.request("POST", "/v1/domains/" + Query.enc(id) + "/transfer", body, transport.type(DomainTransfer.class));
    }

    /**
     * Check whether a domain name is available to add (checks public DNS).
     *
     * @param name the domain name.
     * @return the availability result.
     */
    public DomainAvailability checkAvailability(String name) {
        String qs = Query.of().add("name", name).build();
        return transport.request("GET", "/v1/domains/check-availability" + qs, null, transport.type(DomainAvailability.class));
    }

    /**
     * Check whether a domain name already exists in your account.
     *
     * @param name the domain name.
     * @return the check result.
     */
    public DomainCheck check(String name) {
        return transport.request("GET", "/v1/domains/check/" + Query.enc(name), null, transport.type(DomainCheck.class));
    }
}
