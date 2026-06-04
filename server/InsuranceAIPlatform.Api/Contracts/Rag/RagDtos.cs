namespace InsuranceAIPlatform.Api.Contracts.Rag;

/// <summary>Request body for POST /api/claims/{claimId}/rag/ask.</summary>
public sealed record RagAskRequestDto(string? Question, string? UseCase);

public sealed record RagCitationDto(string ChunkId, string DocumentId, string Kind, string Snippet, double Score);

/// <summary>
/// A grounded RAG answer. AdvisoryOnly is always true — the human adjuster makes the final decision.
/// Citations reference only retrieved chunks.
/// </summary>
public sealed record RagAnswerDto(
    string TraceId,
    string ClaimId,
    string UseCase,
    string Question,
    string Answer,
    int Confidence,
    IReadOnlyList<RagCitationDto> Citations,
    IReadOnlyList<string> RetrievedChunkIds,
    string ProviderMode,
    int PromptTokens,
    int CompletionTokens,
    long CostMicros,
    long RetrievalMs,
    bool AdvisoryOnly,
    string CorrelationId,
    DateTime CreatedAtUtc);

public sealed record RagEvidenceHitDto(string ChunkId, string DocumentId, string Kind, string Snippet, double Score);

/// <summary>Cross-claim similar-claim card. Claim-level only — no other claim's evidence text.</summary>
public sealed record SimilarClaimDto(string ClaimId, double Score, string Reason, IReadOnlyList<string> MatchingCategories);

public sealed record SimilarClaimsResponseDto(string ClaimId, IReadOnlyList<SimilarClaimDto> SimilarClaims, string CorrelationId);

public sealed record RagEvidenceSearchResponseDto(
    string ClaimId,
    string Query,
    IReadOnlyList<RagEvidenceHitDto> Hits,
    string CorrelationId);

public sealed record RagEvaluationQuestionDto(string QuestionId, string ClaimId, string UseCase, string Text, string Language);

public sealed record RagAuditTraceDto(
    string TraceId,
    string ClaimId,
    string UseCase,
    string Query,
    string Answer,
    int Confidence,
    IReadOnlyList<string> RetrievedChunkIds,
    long CostMicros,
    DateTime CreatedAtUtc);

// ---- Infrastructure status DTOs ----

public sealed record RagSqlStatusDto(string Status, int PolicyClauses, int EvidenceChunks, int EvaluationQuestions, int AuditTraces);

public sealed record RagIndexStatusDto(string Status, int EmbeddedChunks, int TotalChunks, string EmbeddingModel, int Dimensions);

public sealed record RagRuntimeStatusDto(string Status, bool Enabled, string Model, bool EndpointConfigured, bool Reachable);

public sealed record RagVectorRuntimeStatusDto(string Status, bool Enabled, string Backend, bool EndpointConfigured, bool Reachable);

public sealed record RagInfrastructureStatusDto(
    string ClaimId,
    RagSqlStatusDto SqlSourceOfTruth,
    RagIndexStatusDto EvidenceMemoryIndex,
    RagVectorRuntimeStatusDto VectorRuntime,
    RagRuntimeStatusDto LocalReasoningRuntime,
    DateTime GeneratedAtUtc,
    string CorrelationId);
