using InsuranceAIPlatform.Services.AiAnalysis.Persistence;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Embedding;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Ingestion;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InsuranceAIPlatform.Tests;

/// <summary>
/// Unit tests for EvidenceIngestionService — the seam that turns an uploaded synthetic claim
/// document's text into claim-scoped EvidenceChunks (closing the create-from-zero RAG gap).
/// All run against EF InMemory — no SQL Server required.
/// Asserts: chunks are created for the target claim, claim-prefixed + scoped; re-ingest is idempotent;
/// other claims' evidence is never touched (leakage guard); empty text creates nothing.
/// </summary>
public class EvidenceIngestionServiceTests
{
    private sealed class InMemoryFactory : IDbContextFactory<AiAnalysisDbContext>
    {
        private readonly DbContextOptions<AiAnalysisDbContext> _options;
        public InMemoryFactory(string dbName) =>
            _options = new DbContextOptionsBuilder<AiAnalysisDbContext>().UseInMemoryDatabase(dbName).Options;
        public AiAnalysisDbContext CreateDbContext() => new(_options);
    }

    private static (EvidenceIngestionService svc, InMemoryFactory factory) Build(string dbName)
    {
        var factory = new InMemoryFactory(dbName);
        var svc = new EvidenceIngestionService(factory, new DeterministicEmbeddingProvider(256));
        return (svc, factory);
    }

    [Fact]
    public async Task Ingest_creates_claim_scoped_chunks_with_embeddings()
    {
        var (svc, factory) = Build(nameof(Ingest_creates_claim_scoped_chunks_with_embeddings));

        var created = await svc.IngestDocumentTextAsync(
            "CLM-NEW-1", "DOC-1", "statement", "Заява водія",
            "Перший абзац: водій повідомив про зіткнення.\n\nДругий абзац: пошкоджено передній бампер.");

        Assert.True(created >= 1);
        await using var db = factory.CreateDbContext();
        var chunks = db.EvidenceChunks.Where(c => c.ClaimId == "CLM-NEW-1").ToList();
        Assert.Equal(created, chunks.Count);
        Assert.All(chunks, c => Assert.Equal("CLM-NEW-1", c.ClaimId));
        Assert.All(chunks, c => Assert.StartsWith("CLM-NEW-1-uploaded-", c.ChunkId));
        Assert.All(chunks, c => Assert.False(string.IsNullOrWhiteSpace(c.EmbeddingJson)));
        Assert.All(chunks, c => Assert.Equal(256, c.EmbeddingDim));
        Assert.All(chunks, c => Assert.False(string.IsNullOrWhiteSpace(c.ChunkHash)));
    }

    [Fact]
    public async Task Ingest_is_idempotent_on_reingest()
    {
        var (svc, factory) = Build(nameof(Ingest_is_idempotent_on_reingest));

        var first = await svc.IngestDocumentTextAsync("CLM-NEW-2", "DOC-2", "note", "Документ", "Текст доказу.");
        var second = await svc.IngestDocumentTextAsync("CLM-NEW-2", "DOC-2", "note", "Документ", "Текст доказу.");

        Assert.True(first >= 1);
        Assert.Equal(0, second); // same document + text => no duplicate chunks
        await using var db = factory.CreateDbContext();
        Assert.Equal(first, db.EvidenceChunks.Count(c => c.ClaimId == "CLM-NEW-2"));
    }

    [Fact]
    public async Task Ingest_never_touches_or_references_other_claims()
    {
        var (svc, factory) = Build(nameof(Ingest_never_touches_or_references_other_claims));

        // Pre-existing seeded-style evidence for a different claim.
        await using (var seed = factory.CreateDbContext())
        {
            seed.EvidenceChunks.Add(new EvidenceChunk
            {
                ChunkId = "CLM-1006-police#0", ClaimId = "CLM-1006", DocumentId = "d", Kind = "police",
                Ordinal = 0, Text = "seeded police report", ChunkHash = "h", EmbeddingDim = 256,
            });
            await seed.SaveChangesAsync();
        }

        await svc.IngestDocumentTextAsync("CLM-NEW-3", "DOC-3", "statement", "Документ", "Доказ лише для нового клейму.");

        await using var db = factory.CreateDbContext();
        // Seeded CLM-1006 evidence is untouched.
        var seededForCustomer = db.EvidenceChunks.Where(c => c.ClaimId == "CLM-1006").ToList();
        Assert.Single(seededForCustomer);
        Assert.Equal("CLM-1006-police#0", seededForCustomer[0].ChunkId);
        Assert.Equal("seeded police report", seededForCustomer[0].Text);
        // New claim chunks exist and none reference another claim id.
        var newChunks = db.EvidenceChunks.Where(c => c.ClaimId == "CLM-NEW-3").ToList();
        Assert.NotEmpty(newChunks);
        Assert.All(newChunks, c => Assert.DoesNotContain("CLM-1006", c.ChunkId));
    }

    [Fact]
    public async Task Ingest_empty_or_whitespace_text_creates_nothing()
    {
        var (svc, factory) = Build(nameof(Ingest_empty_or_whitespace_text_creates_nothing));

        Assert.Equal(0, await svc.IngestDocumentTextAsync("CLM-NEW-4", "DOC-4", "note", "Doc", "   "));
        Assert.Equal(0, await svc.IngestDocumentTextAsync("CLM-NEW-4", "DOC-4", "note", "Doc", ""));
        await using var db = factory.CreateDbContext();
        Assert.Equal(0, db.EvidenceChunks.Count(c => c.ClaimId == "CLM-NEW-4"));
    }
}
