package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;

import java.util.List;

/**
 * A paginated envelope {@code {items, total, page, limit}} returned by the
 * suppressions list and webhook-delivery list endpoints.
 *
 * @param <T> the element type.
 */
@JsonIgnoreProperties(ignoreUnknown = true)
public final class Page<T> {
    /** The items on this page. */
    public List<T> items;
    /** Total number of items across all pages. */
    public int total;
    /** Zero-based page index. */
    public int page;
    /** Page size. */
    public int limit;
}
