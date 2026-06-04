namespace InsuranceAIPlatform.Services.AiAnalysis.Rag.Persistence;

/// <summary>
/// A gold evaluation question — the "ruler" for RAG quality. Synthetic only.
///
/// Merge note (§3): the proposed separate <c>ExpectedAnswer</c> entity is folded in here
/// (1:1 with a question). The grader checks that retrieval returns
/// <see cref="ExpectedSourceChunkIdsCsv"/>, that the answer contains
/// <see cref="ExpectedAnswerKeywordsCsv"/>, and — critically for the leakage guard — that the
/// answer NEVER cites any chunk in <see cref="MustNotCiteChunkIdsCsv"/> (e.g. another claim's evidence).
/// </summary>
public sealed class RagEvaluationQuestion
{
    public string QuestionId { get; set; } = string.Empty;            // e.g. "Q-COVER-1006-1"
    public string ClaimId { get; set; } = string.Empty;
    public string UseCase { get; set; } = string.Empty;              // coverage | missing_docs | risk | similar | summary | custom
    public string Text { get; set; } = string.Empty;
    public string Language { get; set; } = "uk";
    public string ExpectedSourceChunkIdsCsv { get; set; } = string.Empty;
    public string MustNotCiteChunkIdsCsv { get; set; } = string.Empty;   // negative guard (cross-claim leak)
    public string ExpectedAnswerKeywordsCsv { get; set; } = string.Empty; // grader keywords (merged ExpectedAnswer)
}
