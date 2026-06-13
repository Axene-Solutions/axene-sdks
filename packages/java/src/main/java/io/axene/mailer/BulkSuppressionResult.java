package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;

/** Result of {@code suppressions.bulkUpload}. */
@JsonIgnoreProperties(ignoreUnknown = true)
public final class BulkSuppressionResult {
    public int added;
    public int skipped;
    @JsonProperty("total_processed") public int totalProcessed;
}
