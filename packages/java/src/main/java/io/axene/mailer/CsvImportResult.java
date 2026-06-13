package io.axene.mailer;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;

import java.util.List;

/** Result of {@code contacts.uploadCsv}. */
@JsonIgnoreProperties(ignoreUnknown = true)
public final class CsvImportResult {
    public int imported;
    public int skipped;
    public List<String> errors;
}
