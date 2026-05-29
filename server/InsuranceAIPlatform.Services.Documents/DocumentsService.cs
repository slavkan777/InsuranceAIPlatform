using InsuranceAIPlatform.BuildingBlocks;

namespace InsuranceAIPlatform.Services.Documents;

/// <summary>
/// Skeleton implementation of <see cref="IDocumentsService"/>. Reports readiness only; stores no
/// metadata and accepts no uploads yet. Write methods are no-ops — the DB-backed
/// <see cref="Persistence.PersistenceDocumentsService"/> handles real writes.
/// </summary>
public sealed class DocumentsService : IDocumentsService
{
    public string ServiceName => ServiceNames.Documents;

    public ServiceHealthSnapshot GetHealth() => new(
        ServiceNames.Documents,
        ServiceReadinessStatus.Stub,
        "skeleton-v0.1",
        new[] { "document-metadata", "photo-metadata", "missing-evidence" });

    public Task<int> RequestMissingDocumentAsync(
        string claimId, string documentTitle, string? reason,
        ActorContext actor, CancellationToken ct = default)
        => Task.FromResult(-1); // no-op in skeleton

    public Task<string> CreateMetadataPlaceholderAsync(
        string claimId, string kind, string title, string? docType,
        ActorContext actor, CancellationToken ct = default)
        => Task.FromResult(string.Empty); // no-op in skeleton

    public Task<string> UploadDocumentContentAsync(
        string claimId, string kind, string title, string? docType,
        string content, ActorContext actor, CancellationToken ct = default)
        => Task.FromResult(string.Empty); // no-op in skeleton
}
