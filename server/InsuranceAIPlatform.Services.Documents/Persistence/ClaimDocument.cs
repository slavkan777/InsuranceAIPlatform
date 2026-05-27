namespace InsuranceAIPlatform.Services.Documents.Persistence;

/// <summary>
/// Document associated with a claim. ClaimId is a cross-context string reference.
/// DocType: "document" | "photo". Status: "ok" | "warn" | "missing".
/// </summary>
public sealed class ClaimDocument
{
    public string Id { get; set; } = string.Empty;
    public string ClaimId { get; set; } = string.Empty;     // cross-context ref by id
    public string Kind { get; set; } = string.Empty;        // e.g. "application", "police", "photo-front"
    public string Title { get; set; } = string.Empty;
    public string Meta { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;      // "ok" | "warn" | "missing"
    public string DocType { get; set; } = string.Empty;     // "document" | "photo"
    public int? AiConfidence { get; set; }
}
