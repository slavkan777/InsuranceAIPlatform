using InsuranceAIPlatform.BuildingBlocks;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAIPlatform.Services.Documents.Persistence;

/// <summary>
/// DB-backed implementation of <see cref="IDocumentsService"/> (singleton-safe via IDbContextFactory).
/// Writes document metadata and missing-document requests to <see cref="DocumentsDbContext"/>.
/// No binary, no blob, no OCR, no customer messaging.
/// </summary>
public sealed class PersistenceDocumentsService : IDocumentsService
{
    private readonly IDbContextFactory<DocumentsDbContext> _factory;
    private readonly IClock _clock;

    public PersistenceDocumentsService(IDbContextFactory<DocumentsDbContext> factory, IClock clock)
    {
        _factory = factory;
        _clock   = clock;
    }

    public string ServiceName => ServiceNames.Documents;

    public ServiceHealthSnapshot GetHealth() => new(
        ServiceNames.Documents,
        ServiceReadinessStatus.Ready,
        "persistence-v0.1",
        new[] { "document-metadata", "document-metadata-write", "missing-document-request", "photo-metadata", "missing-evidence" });

    // -----------------------------------------------------------------------
    // RequestMissingDocumentAsync
    // -----------------------------------------------------------------------

    public async Task<int> RequestMissingDocumentAsync(
        string claimId,
        string documentTitle,
        string? reason,
        ActorContext actor,
        CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var request = new MissingDocumentRequest
        {
            ClaimId          = claimId,
            DocumentTitle    = documentTitle,
            Reason           = reason,
            RequestedAtUtc   = _clock.UtcNow,
            RequestedByActor = $"{actor.ActorName} ({actor.ActorType})",
        };
        db.MissingDocumentRequests.Add(request);
        await db.SaveChangesAsync(ct);
        return request.Id;
    }

    // -----------------------------------------------------------------------
    // CreateMetadataPlaceholderAsync
    // -----------------------------------------------------------------------

    public async Task<string> CreateMetadataPlaceholderAsync(
        string claimId,
        string kind,
        string title,
        string? docType,
        ActorContext actor,
        CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var id = $"doc-{Guid.NewGuid():N}";
        var doc = new ClaimDocument
        {
            Id      = id,
            ClaimId = claimId,
            Kind    = kind,
            Title   = title,
            Meta    = string.Empty,
            Status  = "placeholder",
            DocType = docType ?? "document",
        };
        db.ClaimDocuments.Add(doc);
        await db.SaveChangesAsync(ct);
        return id;
    }
}
