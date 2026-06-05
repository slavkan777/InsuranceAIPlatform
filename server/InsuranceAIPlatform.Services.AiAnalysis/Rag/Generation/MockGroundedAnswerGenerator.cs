using InsuranceAIPlatform.Services.AiAnalysis.Rag.Contracts;

namespace InsuranceAIPlatform.Services.AiAnalysis.Rag.Generation;

/// <summary>
/// Deterministic, local grounded-answer generator (default). Composes an advisory answer strictly
/// from the retrieved chunks and cites only those chunks — the grounding invariant. No external call.
///
/// Guardrails baked in:
///  - advisory-only framing + footer (human makes the final decision);
///  - the "risk" use-case is explicitly framed as advisory and NEVER asserts fraud;
///  - when nothing relevant is retrieved, it says so instead of inventing an answer.
/// </summary>
public sealed class MockGroundedAnswerGenerator : IGroundedAnswerGenerator
{
    public const string AdvisoryFooter =
        "AI-аналіз має лише рекомендаційний характер — фінальне рішення приймає людина-ад'юстер.";

    public string ProviderMode => "Mock";

    public GroundedDraft Generate(GroundedRequest request)
    {
        var retrieved = request.Retrieved ?? Array.Empty<ScoredChunk>();

        var citations = BuildCitations(retrieved);

        if (retrieved.Count == 0)
        {
            const string none = "Недостатньо релевантних доказів у матеріалах справи для відповіді. " +
                                 "Рекомендується перегляд людиною.";
            return new GroundedDraft($"{none} {AdvisoryFooter}", 0, citations, EstTokens(request.Question), 24, ProviderMode);
        }

        string lead = Lead(request.UseCase);
        string body = string.Join(" ", retrieved.Select((s, i) => $"[{i + 1}] {Snippet(s.Chunk.Text)}"));
        string answer = $"{lead} {body} {AdvisoryFooter}";

        int confidence = ConfidenceFromScore(retrieved[0].Score);
        int promptTokens = EstTokens(request.Question) + retrieved.Sum(s => EstTokens(s.Chunk.Text));
        int completionTokens = EstTokens(answer);

        return new GroundedDraft(answer, confidence, citations, promptTokens, completionTokens, ProviderMode);
    }

    /// <summary>
    /// Build citations from the ALREADY-retrieved chunks — the grounding invariant. Shared with the
    /// LocalLlama generator so a live model NEVER authors citations: the cited evidence is always the
    /// real retrieved (claim-scoped) chunks, regardless of what the model writes in its prose.
    /// </summary>
    internal static IReadOnlyList<RagCitation> BuildCitations(IReadOnlyList<ScoredChunk> retrieved) =>
        retrieved
            .Select(s => new RagCitation(s.Chunk.ChunkId, s.Chunk.DocumentId, s.Chunk.Kind, Snippet(s.Chunk.Text), s.Score))
            .ToList();

    private static string Lead(string useCase) => useCase switch
    {
        RagUseCases.Coverage    => "За умовами полісу та матеріалами справи:",
        RagUseCases.MissingDocs => "Перевірка повноти документів у справі:",
        // Explicitly advisory; never accuses fraud.
        RagUseCases.Risk        => "Пояснення ризик-сигналів (рекомендаційно, без звинувачень):",
        RagUseCases.Similar     => "Справи зі схожими ознаками за наявними доказами:",
        RagUseCases.Summary     => "Зведення доказів для рішення людини:",
        _                       => "На основі знайдених доказів:"
    };

    /// <summary>
    /// Maps cosine similarity (~0..1) to a 0..99 confidence band. Shared with the LocalLlama generator:
    /// confidence is ALWAYS derived from the top retrieval score, never invented by a live model.
    /// </summary>
    internal static int ConfidenceFromScore(double topScore)
    {
        int c = (int)Math.Round(Math.Clamp(topScore, 0, 1) * 100);
        return Math.Clamp(c, 0, 99);
    }

    internal static string Snippet(string text)
    {
        text = (text ?? string.Empty).Trim();
        return text.Length <= 200 ? text : text[..200] + "…";
    }

    /// <summary>Rough deterministic token estimate (~4 chars/token).</summary>
    internal static int EstTokens(string? s) => string.IsNullOrEmpty(s) ? 0 : Math.Max(1, s.Length / 4);
}
