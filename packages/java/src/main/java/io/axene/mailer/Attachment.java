package io.axene.mailer;

import java.util.LinkedHashMap;
import java.util.Map;

/** A file attachment. {@code contentBase64} is the base64-encoded content. */
public final class Attachment {
    public final String filename;
    public final String contentBase64;
    public final String contentType;

    public Attachment(String filename, String contentBase64) { this(filename, contentBase64, null); }
    public Attachment(String filename, String contentBase64, String contentType) {
        this.filename = filename; this.contentBase64 = contentBase64; this.contentType = contentType;
    }

    Map<String, Object> toMap() {
        Map<String, Object> m = new LinkedHashMap<>();
        m.put("filename", filename);
        m.put("content_base64", contentBase64);
        if (contentType != null) m.put("content_type", contentType);
        return m;
    }
}
