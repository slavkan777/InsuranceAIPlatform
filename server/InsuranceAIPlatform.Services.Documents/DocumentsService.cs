using InsuranceAIPlatform.BuildingBlocks;

namespace InsuranceAIPlatform.Services.Documents;

/// <summary>
/// Skeleton implementation of <see cref="IDocumentsService"/>. Reports readiness only;
/// stores no metadata and accepts no uploads yet. Blob storage stays an Azure-later concern.
/// </summary>
public sealed class DocumentsService : IDocumentsService
{
    public string ServiceName => ServiceNames.Documents;

    public ServiceHealthSnapshot GetHealth() => new(
        ServiceNames.Documents,
        ServiceReadinessStatus.Stub,
        "skeleton-v0.1",
        new[] { "document-metadata", "photo-metadata", "missing-evidence" });
}
