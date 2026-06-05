using System.Text;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Contracts;

namespace InsuranceAIPlatform.Services.AiAnalysis.Rag.Generation;

/// <summary>
/// Local Ollama / LLaMA grounded-answer generator (OLLAMA_LOCAL_FULL_EXECUTION gate).
///
/// Wired into DI ONLY when <c>Rag:LocalLlamaEnabled = true</c>. Behaviour:
///  - If the seam is disabled, no client is registered, or there are NO retrieved chunks
///    (insufficient evidence), it delegates to the deterministic <see cref="MockGroundedAnswerGenerator"/>
///    — it never calls a model when there is nothing grounded to answer from.
///  - Otherwise it builds a STRICTLY grounded prompt from the already-retrieved chunks and asks the
///    local Ollama endpoint. On ANY failure/timeout/empty response it falls back to the mock.
///
/// Honesty + grounding invariants (mirrors the gate's hard requirements):
///  - The returned <see cref="GroundedDraft.ProviderMode"/> is <c>"LocalLlama"</c> ONLY when the local
///    model actually produced the prose; a fallback carries the mock's <c>"Mock"</c> mode — a slow or
///    absent Ollama can never be mislabelled as a live local answer.
///  - Citations are built from the retrieved chunks via <see cref="MockGroundedAnswerGenerator.BuildCitations"/>,
///    NOT from anything the model writes — the cited evidence is always the real claim-scoped chunks.
///  - Confidence is derived from the top retrieval score, never invented by the model.
///  - The advisory footer is always appended (human makes the final decision).
/// No API key / secret is involved (local model only).
/// </summary>
public sealed class LocalLlamaGroundedAnswerGenerator : IGroundedAnswerGenerator
{
    public const string LocalProviderMode = "LocalLlama";

    private readonly RagOptions _options;
    private readonly MockGroundedAnswerGenerator _fallback;
    private readonly ILocalLlamaClient? _client;

    public LocalLlamaGroundedAnswerGenerator(
        RagOptions options, MockGroundedAnswerGenerator fallback, ILocalLlamaClient? client = null)
    {
        _options = options;
        _fallback = fallback;
        _client = client;
    }

    // Nominal mode of this generator. The EFFECTIVE per-answer provider is carried on each GroundedDraft
    // (it is "Mock" whenever a call falls back), so the audit trace never overstates the live model.
    public string ProviderMode => LocalProviderMode;

    public GroundedDraft Generate(GroundedRequest request)
    {
        var retrieved = request.Retrieved ?? Array.Empty<ScoredChunk>();

        // No model call when the seam is off, no client is wired, or there is no grounded evidence —
        // the mock emits the honest "insufficient evidence, human review" answer with 0 citations.
        if (!_options.LocalLlamaEnabled || _client is null || retrieved.Count == 0)
            return _fallback.Generate(request);

        string system = BuildSystemPrompt();
        string user = BuildUserPrompt(request, retrieved);

        LocalLlamaCompletion? completion = _client.TryComplete(system, user);

        // Any failure/timeout/empty body => honest fallback (ProviderMode stays "Mock").
        if (completion is null || string.IsNullOrWhiteSpace(completion.Text))
            return _fallback.Generate(request);

        string answer = $"{completion.Text.Trim()} {MockGroundedAnswerGenerator.AdvisoryFooter}";

        // Grounding invariant: citations + confidence come from the retrieval, not the model.
        var citations = MockGroundedAnswerGenerator.BuildCitations(retrieved);
        int confidence = MockGroundedAnswerGenerator.ConfidenceFromScore(retrieved[0].Score);

        int promptTokens = completion.PromptTokens > 0
            ? completion.PromptTokens
            : MockGroundedAnswerGenerator.EstTokens(user);
        int completionTokens = completion.CompletionTokens > 0
            ? completion.CompletionTokens
            : MockGroundedAnswerGenerator.EstTokens(answer);

        return new GroundedDraft(answer, confidence, citations, promptTokens, completionTokens, LocalProviderMode);
    }

    private static string BuildSystemPrompt() =>
        "Ти — досвідчений аналітик відділу врегулювання страхових справ. " +
        "Твоя задача — стисло відповісти на запитання, спираючись ВИКЛЮЧНО на надані фрагменти доказів. " +
        "Правила: " +
        "(1) відповідай 2–4 реченнями, СВОЇМИ СЛОВАМИ — НЕ копіюй і не переписуй фрагменти дослівно; " +
        "(2) не вигадуй фактів, яких немає у фрагментах; " +
        "(3) якщо доказів недостатньо — прямо скажи про це і порекомендуй перегляд людиною; " +
        "(4) не приймай остаточних рішень про виплату чи відмову і не звинувачуй у шахрайстві — " +
        "лише вказуй ризик-сигнали для людини-ад'юстера; " +
        "(5) відповідай мовою запитання.";

    private static string BuildUserPrompt(GroundedRequest request, IReadOnlyList<ScoredChunk> retrieved)
    {
        // Context FIRST, question LAST, then an explicit answer cue — this framing stops a small
        // instruct model from continuing/echoing the prompt verbatim.
        var sb = new StringBuilder();
        sb.Append("Контекст — фрагменти доказів справи ").Append(request.ClaimId)
          .Append(" (єдине джерело для відповіді):\n");
        for (int i = 0; i < retrieved.Count; i++)
            sb.Append('[').Append(i + 1).Append("] ").Append(retrieved[i].Chunk.Text).Append('\n');
        sb.Append("\nЗапитання: ").Append(request.Question).Append('\n');
        sb.Append("\nСтисла відповідь-аналіз (2–4 речення, своїми словами):");
        return sb.ToString();
    }
}
