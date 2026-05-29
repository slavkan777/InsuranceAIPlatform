namespace InsuranceAIPlatform.Services.Documents.Persistence;

/// <summary>
/// Document associated with a claim. ClaimId is a cross-context string reference.
/// DocType: "document" | "photo". Status: "ok" | "warn" | "missing" | "placeholder" | "uploaded".
///
/// Content stores synthetic local-sandbox text content for the test/local-demo
/// scenario (police report text, customer statement text, claim-description,
/// etc.). It is intentionally a nvarchar(max) text column — no binary, no blob,
/// no OCR, no real file. Content is OPTIONAL (NULL allowed) so legacy seed rows
/// and metadata-only rows continue to work.
/// </summary>
public sealed class ClaimDocument
{
    public string Id { get; set; } = string.Empty;
    public string ClaimId { get; set; } = string.Empty;     // cross-context ref by id
    public string Kind { get; set; } = string.Empty;        // e.g. "application", "police", "photo-front"
    public string Title { get; set; } = string.Empty;
    public string Meta { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;      // "ok" | "warn" | "missing" | "placeholder" | "uploaded"
    public string DocType { get; set; } = string.Empty;     // "document" | "photo"
    public int? AiConfidence { get; set; }

    /// <summary>
    /// Optional synthetic text content (police report text, customer statement,
    /// etc.). Local sandbox only — no binary, no PII, no external file. NULL for
    /// legacy metadata-only rows.
    /// </summary>
    public string? Content { get; set; }

    /// <summary>UTC timestamp when uploaded. NULL for legacy seed/placeholder rows.</summary>
    public DateTimeOffset? UploadedAtUtc { get; set; }

    /// <summary>Synthetic actor who uploaded the content. NULL for legacy rows.</summary>
    public string? UploadedByActor { get; set; }
}
