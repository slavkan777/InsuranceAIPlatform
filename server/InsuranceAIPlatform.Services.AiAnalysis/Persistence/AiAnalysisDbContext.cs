using InsuranceAIPlatform.Services.AiAnalysis.Rag.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAIPlatform.Services.AiAnalysis.Persistence;

/// <summary>
/// EF Core DbContext for the AI Analysis bounded context.
/// Schema: ai_analysis. No cross-context DbSets. ProviderMode is enforced at seeder level (never real provider).
/// </summary>
public sealed class AiAnalysisDbContext : DbContext
{
    public AiAnalysisDbContext(DbContextOptions<AiAnalysisDbContext> options) : base(options) { }

    public DbSet<AiAnalysisRun> AiAnalysisRuns => Set<AiAnalysisRun>();
    public DbSet<AiFinding> AiFindings => Set<AiFinding>();
    public DbSet<AiEvidenceReference> AiEvidenceReferences => Set<AiEvidenceReference>();
    public DbSet<AiRiskSignal> AiRiskSignals => Set<AiRiskSignal>();

    // ---- Local RAG foundation (RAG_LOCAL_FOUNDATION_MEGA_V0.1) — same ai_analysis schema ----
    public DbSet<PolicyClause> PolicyClauses => Set<PolicyClause>();
    public DbSet<EvidenceChunk> EvidenceChunks => Set<EvidenceChunk>();
    public DbSet<RagEvaluationQuestion> RagEvaluationQuestions => Set<RagEvaluationQuestion>();
    public DbSet<RagAuditTrace> RagAuditTraces => Set<RagAuditTrace>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ai_analysis");

        modelBuilder.Entity<AiAnalysisRun>(e =>
        {
            e.HasKey(x => x.RunId);
            e.Property(x => x.RunId).HasMaxLength(64);
            e.Property(x => x.ClaimId).HasMaxLength(32).IsRequired();
            e.Property(x => x.ProviderMode).HasMaxLength(50);
            e.Property(x => x.Cost).HasColumnType("decimal(18,4)");

            // Structured fields added by AddAiAnalysisRunStructuredFields migration — all nullable.
            e.Property(x => x.ModelName).HasMaxLength(100);
            e.Property(x => x.Status).HasMaxLength(100);
            e.Property(x => x.SummaryText).HasMaxLength(500);
            e.Property(x => x.RecommendedActionJson).HasMaxLength(4000);
            e.Property(x => x.PolicyExplanationText).HasMaxLength(500);
            e.Property(x => x.GuardrailFlagsJson).HasMaxLength(4000);
            e.Property(x => x.RiskLevel).HasMaxLength(100);
            e.Property(x => x.CorrelationId).HasMaxLength(100);

            e.HasMany(x => x.Findings).WithOne(f => f.Run)
                .HasForeignKey(f => f.RunId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.EvidenceReferences).WithOne(r => r.Run)
                .HasForeignKey(r => r.RunId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.RiskSignals).WithOne(s => s.Run)
                .HasForeignKey(s => s.RunId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AiFinding>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(32);
            e.Property(x => x.RunId).HasMaxLength(64).IsRequired();
            e.Property(x => x.Category).HasMaxLength(100);
            e.Property(x => x.Text).HasMaxLength(500);
            e.Property(x => x.Severity).HasMaxLength(20);
        });

        modelBuilder.Entity<AiEvidenceReference>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(32);
            e.Property(x => x.RunId).HasMaxLength(64).IsRequired();
            e.Property(x => x.Source).HasMaxLength(200);
            e.Property(x => x.Note).HasMaxLength(500);
        });

        modelBuilder.Entity<AiRiskSignal>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(32);
            e.Property(x => x.RunId).HasMaxLength(64).IsRequired();
            e.Property(x => x.Label).HasMaxLength(200);
        });

        // ---- Local RAG foundation entities (additive; same ai_analysis schema) ----

        modelBuilder.Entity<PolicyClause>(e =>
        {
            e.HasKey(x => x.ClauseId);
            e.Property(x => x.ClauseId).HasMaxLength(64);
            e.Property(x => x.ProductCode).HasMaxLength(64).IsRequired();
            e.Property(x => x.PolicyId).HasMaxLength(64);
            e.Property(x => x.ClauseType).HasMaxLength(32);
            e.Property(x => x.Title).HasMaxLength(200);
            // Text → nvarchar(max) (no length cap)
            e.HasIndex(x => x.ProductCode);
        });

        modelBuilder.Entity<EvidenceChunk>(e =>
        {
            e.HasKey(x => x.ChunkId);
            e.Property(x => x.ChunkId).HasMaxLength(128);
            e.Property(x => x.ClaimId).HasMaxLength(32).IsRequired();
            e.Property(x => x.DocumentId).HasMaxLength(128);
            e.Property(x => x.Kind).HasMaxLength(40);
            e.Property(x => x.ChunkHash).HasMaxLength(64);
            e.Property(x => x.Language).HasMaxLength(8);
            e.Property(x => x.SourceVersion).HasMaxLength(16);
            e.Property(x => x.EmbeddingModel).HasMaxLength(64);
            // Text, EmbeddingJson → nvarchar(max)
            e.HasIndex(x => x.ClaimId);
        });

        modelBuilder.Entity<RagEvaluationQuestion>(e =>
        {
            e.HasKey(x => x.QuestionId);
            e.Property(x => x.QuestionId).HasMaxLength(64);
            e.Property(x => x.ClaimId).HasMaxLength(32).IsRequired();
            e.Property(x => x.UseCase).HasMaxLength(32);
            e.Property(x => x.Language).HasMaxLength(8);
            e.HasIndex(x => x.ClaimId);
        });

        modelBuilder.Entity<RagAuditTrace>(e =>
        {
            e.HasKey(x => x.TraceId);
            e.Property(x => x.TraceId).HasMaxLength(64);
            e.Property(x => x.ClaimId).HasMaxLength(32).IsRequired();
            e.Property(x => x.UseCase).HasMaxLength(32);
            e.Property(x => x.ProviderMode).HasMaxLength(50);
            e.HasIndex(x => x.ClaimId);
        });
    }
}
