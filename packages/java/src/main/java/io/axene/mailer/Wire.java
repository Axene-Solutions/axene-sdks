package io.axene.mailer;

import java.util.LinkedHashMap;
import java.util.Map;

/** Internal helpers for building JSON wire bodies. Not part of the public API. */
final class Wire {

    private Wire() {
    }

    /** A new ordered map for accumulating wire fields. */
    static Map<String, Object> map() {
        return new LinkedHashMap<>();
    }

    /** Put {@code value} under {@code key} only when it is non-null (omits the field otherwise). */
    static void putIfNotNull(Map<String, Object> m, String key, Object value) {
        if (value != null) {
            m.put(key, value);
        }
    }
}
