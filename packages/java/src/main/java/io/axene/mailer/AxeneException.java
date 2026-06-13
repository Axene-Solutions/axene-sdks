package io.axene.mailer;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;

/** Thrown for any non-2xx API response. */
public final class AxeneException extends RuntimeException {
    private final int status;
    private final String code;

    public AxeneException(int status, String message, String code) {
        super(message);
        this.status = status;
        this.code = code;
    }

    /** HTTP status code (0 for transport failures). */
    public int getStatus() { return status; }

    /** Machine-readable error code from the API, if any. */
    public String getCode() { return code; }

    static AxeneException fromResponse(int status, String body) {
        String message = "Axene request failed (" + status + ")";
        String code = null;
        try {
            JsonNode root = new ObjectMapper().readTree(body);
            JsonNode detail = root.get("detail");
            if (detail != null) {
                if (detail.isTextual()) {
                    message = detail.asText();
                } else if (detail.isObject()) {
                    if (detail.hasNonNull("message")) message = detail.get("message").asText();
                    if (detail.hasNonNull("code")) code = detail.get("code").asText();
                }
            }
        } catch (Exception ignored) {
            // non-JSON body: keep the generic message
        }
        return new AxeneException(status, message, code);
    }
}
