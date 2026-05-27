namespace InsuranceAIPlatform.Services.Documents.Persistence;

/// <summary>
/// Internal audit record for a missing-document request. No customer message is sent.
/// RequestedByActor is synthetic — no real PII.
/// </summary>
public sealed class MissingDocumentRequest
{
    public int Id { get; set; }
    public string ClaimId { get; set; } = string.Empty;       // cross-context ref
    public string DocumentTitle { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTimeOffset RequestedAtUtc { get; set; }
    public string RequestedByActor { get; set; } = string.Empty;
}
