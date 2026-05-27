using InsuranceAIPlatform.BuildingBlocks;

namespace InsuranceAIPlatform.Services.Documents;

/// <summary>
/// Documents service boundary. Owner of document/photo metadata and missing-evidence detection.
/// No upload, no blob bytes, no OCR, no external calls, no customer messaging in any implementation.
/// </summary>
public interface IDocumentsService : IServiceHealthContributor
{
    /// <summary>Canonical service name (see <see cref="ServiceNames.Documents"/>).</summary>
    string ServiceName { get; }

    /// <summary>
    /// Persists an internal missing-document request record.
    /// No customer message is sent.
    /// Returns the new request's Id.
    /// </summary>
    Task<int> RequestMissingDocumentAsync(
        string claimId,
        string documentTitle,
        string? reason,
        ActorContext actor,
        CancellationToken ct = default);

    /// <summary>
    /// Persists a ClaimDocument metadata placeholder row.
    /// No binary upload, no blob, no OCR.
    /// Returns the new document's Id string.
    /// </summary>
    Task<string> CreateMetadataPlaceholderAsync(
        string claimId,
        string kind,
        string title,
        string? docType,
        ActorContext actor,
        CancellationToken ct = default);
}
