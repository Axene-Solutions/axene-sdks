package io.axene.mailer;

import java.time.Instant;
import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

/** A message to send. Build with {@link #builder()}. {@code from}, {@code to}, and {@code subject} are required. */
public final class SendEmail {

    private final Address from;
    private final List<Address> to;
    private final String subject;
    private final String html;
    private final String text;
    private final List<Address> cc;
    private final List<Address> bcc;
    private final Address replyTo;
    private final Map<String, String> headers;
    private final List<String> tags;
    private final Instant sendAt;
    private final List<Attachment> attachments;

    private SendEmail(Builder b) {
        this.from = b.from;
        this.to = b.to;
        this.subject = b.subject;
        this.html = b.html;
        this.text = b.text;
        this.cc = b.cc;
        this.bcc = b.bcc;
        this.replyTo = b.replyTo;
        this.headers = b.headers;
        this.tags = b.tags;
        this.sendAt = b.sendAt;
        this.attachments = b.attachments;
    }

    public static Builder builder() {
        return new Builder();
    }

    /** Build the JSON wire form. The sender field is wire-named {@code from_}. */
    Map<String, Object> toWire() {
        if (from == null) throw new IllegalStateException("from is required");
        if (to == null || to.isEmpty()) throw new IllegalStateException("at least one `to` recipient is required");
        if (subject == null) throw new IllegalStateException("subject is required");
        Map<String, Object> m = new LinkedHashMap<>();
        m.put("from_", from.toMap());
        m.put("to", mapAddresses(to));
        m.put("subject", subject);
        if (html != null) m.put("html", html);
        if (text != null) m.put("text", text);
        if (cc != null) m.put("cc", mapAddresses(cc));
        if (bcc != null) m.put("bcc", mapAddresses(bcc));
        if (replyTo != null) m.put("reply_to", replyTo.toMap());
        if (headers != null) m.put("headers", headers);
        if (tags != null) m.put("tags", tags);
        if (sendAt != null) m.put("send_at", sendAt.toString());
        if (attachments != null) {
            List<Map<String, Object>> a = new ArrayList<>();
            for (Attachment at : attachments) a.add(at.toMap());
            m.put("attachments", a);
        }
        return m;
    }

    private static List<Map<String, Object>> mapAddresses(List<Address> list) {
        List<Map<String, Object>> out = new ArrayList<>();
        for (Address a : list) out.add(a.toMap());
        return out;
    }

    /** Fluent builder for {@link SendEmail}. */
    public static final class Builder {
        private Address from;
        private final List<Address> to = new ArrayList<>();
        private String subject;
        private String html;
        private String text;
        private List<Address> cc;
        private List<Address> bcc;
        private Address replyTo;
        private Map<String, String> headers;
        private List<String> tags;
        private Instant sendAt;
        private List<Attachment> attachments;

        public Builder from(String email) { this.from = new Address(email); return this; }
        public Builder from(String email, String name) { this.from = new Address(email, name); return this; }
        public Builder from(Address address) { this.from = address; return this; }

        public Builder to(String email) { this.to.add(new Address(email)); return this; }
        public Builder to(String email, String name) { this.to.add(new Address(email, name)); return this; }
        public Builder to(Address address) { this.to.add(address); return this; }

        public Builder cc(String email) { (cc = cc == null ? new ArrayList<>() : cc).add(new Address(email)); return this; }
        public Builder bcc(String email) { (bcc = bcc == null ? new ArrayList<>() : bcc).add(new Address(email)); return this; }

        public Builder subject(String subject) { this.subject = subject; return this; }
        public Builder html(String html) { this.html = html; return this; }
        public Builder text(String text) { this.text = text; return this; }
        public Builder replyTo(String email) { this.replyTo = new Address(email); return this; }
        public Builder header(String name, String value) {
            (headers = headers == null ? new LinkedHashMap<>() : headers).put(name, value);
            return this;
        }
        public Builder tag(String tag) { (tags = tags == null ? new ArrayList<>() : tags).add(tag); return this; }
        /** Schedule for later (Starter plan and up). */
        public Builder sendAt(Instant when) { this.sendAt = when; return this; }
        public Builder attachment(Attachment a) {
            (attachments = attachments == null ? new ArrayList<>() : attachments).add(a);
            return this;
        }

        public SendEmail build() { return new SendEmail(this); }
    }
}
