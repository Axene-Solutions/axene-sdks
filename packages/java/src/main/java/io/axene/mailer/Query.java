package io.axene.mailer;

import java.net.URLEncoder;
import java.nio.charset.StandardCharsets;
import java.util.LinkedHashMap;
import java.util.Map;

/**
 * Internal helper for building URL query strings, skipping null values. Returns
 * an empty string when nothing is set, or {@code "?a=1&b=2"} otherwise. Not part
 * of the public API.
 */
final class Query {

    private final Map<String, String> params = new LinkedHashMap<>();

    private Query() {
    }

    static Query of() {
        return new Query();
    }

    /** Add a parameter when {@code value} is non-null. */
    Query add(String key, Object value) {
        if (value != null) {
            params.put(key, String.valueOf(value));
        }
        return this;
    }

    /** Render the query string (with leading {@code ?}), or empty when no params are set. */
    String build() {
        if (params.isEmpty()) {
            return "";
        }
        StringBuilder sb = new StringBuilder("?");
        boolean first = true;
        for (Map.Entry<String, String> e : params.entrySet()) {
            if (!first) sb.append('&');
            first = false;
            sb.append(enc(e.getKey())).append('=').append(enc(e.getValue()));
        }
        return sb.toString();
    }

    static String enc(String s) {
        return URLEncoder.encode(s, StandardCharsets.UTF_8);
    }
}
