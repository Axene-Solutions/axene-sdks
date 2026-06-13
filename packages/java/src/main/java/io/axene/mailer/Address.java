package io.axene.mailer;

import java.util.LinkedHashMap;
import java.util.Map;

/** A sender or recipient. */
public final class Address {
    public final String email;
    public final String name;

    public Address(String email) { this(email, null); }
    public Address(String email, String name) { this.email = email; this.name = name; }

    Map<String, Object> toMap() {
        Map<String, Object> m = new LinkedHashMap<>();
        m.put("email", email);
        if (name != null) m.put("name", name);
        return m;
    }
}
