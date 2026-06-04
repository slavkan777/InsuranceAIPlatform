using InsuranceAIPlatform.Services.AiAnalysis.Rag.Persistence;

namespace InsuranceAIPlatform.Services.AiAnalysis.Rag.Contracts;

/// <summary>A single cited evidence reference shown to the human reviewer.</summary>
public sealed record RagCitation(string ChunkId, string DocumentId, string Kind, string Snippet, double Score);

/// <summary>A retrieved chunk plus its similarity score (internal to the pipeline).</summary>
public sealed record ScoredChunk(EvidenceChunk Chunk, double Score);

/// <summary>A hit returned by evidence-search (no generated answer).</summary>
public sealed record RagEvidenceHit(string ChunkId, string DocumentId, string Kind, string Snippet, double Score);

/// <summary>
/// A cross-claim similarity result. Deliberately claim-level only: NO raw evidence text of the
/// other claim is exposed — the user must open that claim to see its evidence.
/// </summary>
public sealed record SimilarClaim(string ClaimId, double Score, string Reason, IReadOnlyList<string> MatchingCategories);

/// <summary>
/// A grounded RAG answer. Advisory only — the human adjuster makes the final decision.
/// Citations reference ONLY retrieved chunks (grounding invariant).
/// </summary>
public sealed record RagAnswer(
    string TraceId,
    string ClaimId,
    string UseCase,
    string Question,
    string AnswerText,
    int Confidence,
    IReadOnlyList<RagCitation> Citations,
    IReadOnlyList<string> RetrievedChunkIds,
    string ProviderMode,
    int PromptTokens,
    int CompletionTokens,
    long CostMicros,
    long RetrievalMs,
    bool AdvisoryOnly,
    DateTime CreatedAtUtc);

/// <summary>A gold evaluation question projected for the API/UI (no grader internals leaked).</summary>
public sealed record RagEvalQuestionView(string QuestionId, string ClaimId, string UseCase, string Text, string Language);

/// <summary>A persisted audit trace projected for the API/UI.</summary>
public sealed record RagAuditView(
    string TraceId,
    string ClaimId,
    string UseCase,
    string QueryText,
    string AnswerText,
    int Confidence,
    IReadOnlyList<string> RetrievedChunkIds,
    long CostMicros,
    DateTime CreatedAtUtc);

/// <summary>Input to a grounded-answer generator: retrieval already happened (retrieval-before-generation).</summary>
public sealed record GroundedRequest(
    string ClaimId,
    string UseCase,
    string Question,
    IReadOnlyList<ScoredChunk> Retrieved);

/// <summary>Output of a grounded-answer generator before persistence.</summary>
public sealed record GroundedDraft(
    string AnswerText,
    int Confidence,
    IReadOnlyList<RagCitation> Citations,
    int PromptTokens,
    int CompletionTokens,
    string ProviderMode);

// ---- Infrastructure status contracts (RAG_LOCAL_FOUNDATION_MEGA_V0.1) ----

/// <summary>SQL source-of-truth health: entity counts for the ai_analysis schema.</summary>
public sealed record RagSqlStatus(string Status, int PolicyClauses, int EvidenceChunks, int EvaluationQuestions, int AuditTraces);

/// <summary>In-process vector-cache health: how many chunks have a cached embedding vs total.</summary>
public sealed record RagIndexStatus(string Status, int EmbeddedChunks, int TotalChunks, string EmbeddingModel, int Dimensions);

/// <summary>
/// LocalLlama/Ollama runtime status (advisory — never blocks the pipeline).
/// <paramref name="Reachable"/> is set by a mechanical HTTP probe, NOT guessed from the enabled flag.
/// Status: <c>disabled</c> (seam off) | <c>live_local</c> (enabled + reachable) | <c>skipped_not_available</c> (enabled + unreachable).
/// </summary>
public sealed record RagRuntimeStatus(string Status, bool Enabled, string Model, bool EndpointConfigured, bool Reachable);

/// <summary>
/// Vector runtime (Qdrant) status. Disabled/fallback-safe: when Qdrant is not enabled or not reachable,
/// the in-process deterministic index (EvidenceMemoryIndex) serves vectors. <paramref name="Reachable"/>
/// is mechanically probed. <paramref name="Backend"/> is the backend actually serving vectors
/// (<c>in-memory-hash</c> fallback or <c>qdrant</c> when live).
/// </summary>
public sealed record RagVectorRuntimeStatus(string Status, bool Enabled, string Backend, bool EndpointConfigured, bool Reachable);

/// <summary>Aggregate infrastructure status for a single claim, returned by GetInfrastructureStatusAsync.</summary>
public sealed record RagInfrastructureStatus(
    string ClaimId,
    RagSqlStatus SqlSourceOfTruth,
    RagIndexStatus EvidenceMemoryIndex,
    RagVectorRuntimeStatus VectorRuntime,
    RagRuntimeStatus LocalReasoningRuntime,
    DateTime GeneratedAtUtc,
    string CorrelationId);
