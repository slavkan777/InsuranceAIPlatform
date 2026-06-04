using InsuranceAIPlatform.Services.AiAnalysis.Persistence;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAIPlatform.Services.AiAnalysis.Rag.Retrieval;

/// <summary>
/// DB-backed chunk source. Uses the singleton <see cref="IDbContextFactory{AiAnalysisDbContext}"/>
/// (same pattern as PersistenceAiAnalysisOrchestrator) and filters strictly by claimId in SQL.
/// </summary>
public sealed class DbRagChunkSource : IRagChunkSource
{
    private readonly IDbContextFactory<AiAnalysisDbContext> _factory;

    public DbRagChunkSource(IDbContextFactory<AiAnalysisDbContext> factory) => _factory = factory;

    public async Task<IReadOnlyList<EvidenceChunk>> GetClaimChunksAsync(string claimId, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.EvidenceChunks
            .AsNoTracking()
            .Where(c => c.ClaimId == claimId)   // <-- leakage guard: strict claim scoping
            .OrderBy(c => c.ChunkId)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<EvidenceChunk>> GetAllChunksAsync(CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.EvidenceChunks
            .AsNoTracking()
            .OrderBy(c => c.ChunkId)
            .ToListAsync(ct);
    }
}
