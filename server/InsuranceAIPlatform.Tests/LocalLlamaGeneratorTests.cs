using InsuranceAIPlatform.Services.AiAnalysis.Rag;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Contracts;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Embedding;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Generation;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Persistence;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Retrieval;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Runtime;

namespace InsuranceAIPlatform.Tests;

/// <summary>
/// Tests for the local Ollama grounded-answer generator (OLLAMA_LOCAL_FULL_EXECUTION gate). Proves:
///  - the seam stays OFF unless enabled AND a client is wired (else deterministic mock);
///  - a live completion is labelled "LocalLlama" and carries the model's prose;
///  - citations + confidence ALWAYS come from the retrieval, never from the model (grounding invariant);
///  - an Ollama outage/timeout falls back to the mock and is NEVER mislabelled as a live local answer;
///  - empty retrieval short-circuits to the "insufficient evidence" answer WITHOUT calling the model.
/// The HTTP boundary is faked; the REAL <see cref="LocalLlamaGroundedAnswerGenerator"/> logic is under test.
/// </summary>
public class LocalLlamaGeneratorTests
{
    private static readonly DeterministicEmbeddingProvider Embed = new(256);

    private static EvidenceChunk Chunk(string chunkId, string claimId, string text) =>
        new() { ChunkId = chunkId, ClaimId = claimId, DocumentId = "DOC", Kind = "statement", Text = text };

    private static IReadOnlyList<ScoredChunk> SampleRetrieved() => new[]
    {
        new ScoredChunk(Chunk("CLM-1006-invoice#1", "CLM-1006", "Оцінка ремонту 2720 перевищує бенчмарк на 38 відсотків."), 0.81),
        new ScoredChunk(Chunk("CLM-1006-police#0",  "CLM-1006", "Поліцейський звіт підтвердив зіткнення."), 0.55),
    };

    private static GroundedRequest Request(IReadOnlyList<ScoredChunk> retrieved, string question = "Чому ризик?") =>
        new("CLM-1006", RagUseCases.Risk, question, retrieved);

    // ── seam off ─────────────────────────────────────────────────────────────

    [Fact]
    public void Disabled_seam_delegates_to_mock_and_never_calls_the_client()
    {
        var fake = new FakeLocalLlamaClient();
        var options = new RagOptions { LocalLlamaEnabled = false };
        var gen = new LocalLlamaGroundedAnswerGenerator(options, new MockGroundedAnswerGenerator(), fake);

        var draft = gen.Generate(Request(SampleRetrieved()));

        Assert.Equal("Mock", draft.ProviderMode);   // honest: no live model was used
        Assert.Equal(0, fake.Calls);                 // disabled seam must not contact Ollama at all
    }

    [Fact]
    public void Enabled_but_no_client_wired_delegates_to_mock()
    {
        var options = new RagOptions { LocalLlamaEnabled = true };
        var gen = new LocalLlamaGroundedAnswerGenerator(options, new MockGroundedAnswerGenerator(), client: null);

        var draft = gen.Generate(Request(SampleRetrieved()));

        Assert.Equal("Mock", draft.ProviderMode);
    }

    // ── live local path ────────────────────────────────────────────────────────

    [Fact]
    public void Enabled_with_serving_client_produces_a_LocalLlama_answer()
    {
        var fake = new FakeLocalLlamaClient(reply: "Ризик підвищений: рахунок перевищує бенчмарк, потрібна перевірка людиною.");
        var options = new RagOptions { LocalLlamaEnabled = true };
        var gen = new LocalLlamaGroundedAnswerGenerator(options, new MockGroundedAnswerGenerator(), fake);

        var draft = gen.Generate(Request(SampleRetrieved()));

        Assert.Equal(LocalLlamaGroundedAnswerGenerator.LocalProviderMode, draft.ProviderMode); // "LocalLlama"
        Assert.Equal(1, fake.Calls);
        Assert.Contains("перевищує бенчмарк", draft.AnswerText);                                // the model's prose
        Assert.Contains(MockGroundedAnswerGenerator.AdvisoryFooter, draft.AnswerText);          // advisory framing always present
        // The grounded context (retrieved chunk text) was actually sent to the model.
        Assert.Contains("38 відсотків", fake.LastUserPrompt);
    }

    [Fact]
    public void Citations_and_confidence_come_from_retrieval_not_from_the_model()
    {
        // The "model" tries to cite a foreign chunk in its prose — it must NOT leak into the citations.
        var fake = new FakeLocalLlamaClient(reply: "Беру до уваги CLM-9999-secret#0 з іншої справи.");
        var options = new RagOptions { LocalLlamaEnabled = true };
        var retrieved = SampleRetrieved();
        var gen = new LocalLlamaGroundedAnswerGenerator(options, new MockGroundedAnswerGenerator(), fake);

        var draft = gen.Generate(Request(retrieved));

        // Citations are exactly the retrieved (claim-scoped) chunks — the model cannot author them.
        Assert.Equal(retrieved.Select(r => r.Chunk.ChunkId).OrderBy(x => x),
                     draft.Citations.Select(c => c.ChunkId).OrderBy(x => x));
        Assert.DoesNotContain(draft.Citations, c => c.ChunkId.Contains("9999"));
        // Confidence is derived from the top retrieval score, NOT the model: it equals exactly what the
        // deterministic mock produces for the same retrieval set.
        var mockDraft = new MockGroundedAnswerGenerator().Generate(Request(retrieved));
        Assert.Equal(mockDraft.Confidence, draft.Confidence);
    }

    // ── fallback honesty ─────────────────────────────────────────────────────────

    [Fact]
    public void Client_outage_falls_back_to_mock_and_does_not_claim_a_local_answer()
    {
        var fake = new FakeLocalLlamaClient(outage: true); // TryComplete returns null
        var options = new RagOptions { LocalLlamaEnabled = true };
        var gen = new LocalLlamaGroundedAnswerGenerator(options, new MockGroundedAnswerGenerator(), fake);

        var draft = gen.Generate(Request(SampleRetrieved()));

        Assert.Equal("Mock", draft.ProviderMode); // MUST NOT be "LocalLlama" — never mislabel a fallback
        Assert.Equal(1, fake.Calls);               // it tried the live model first…
        Assert.NotEmpty(draft.Citations);          // …and the mock still produced a grounded answer
    }

    [Fact]
    public void Empty_retrieval_returns_insufficient_evidence_and_skips_the_model()
    {
        var fake = new FakeLocalLlamaClient(reply: "should never be used");
        var options = new RagOptions { LocalLlamaEnabled = true };
        var gen = new LocalLlamaGroundedAnswerGenerator(options, new MockGroundedAnswerGenerator(), fake);

        var draft = gen.Generate(Request(Array.Empty<ScoredChunk>(), "що завгодно"));

        Assert.Equal(0, fake.Calls);               // no grounded evidence → never call the model
        Assert.Equal("Mock", draft.ProviderMode);
        Assert.Empty(draft.Citations);
        Assert.Equal(0, draft.Confidence);
        Assert.Contains("Недостатньо", draft.AnswerText); // honest insufficient-evidence answer
    }

    // ── RagService integration: a serving local model is persisted as "LocalLlama" ──

    [Fact]
    public async Task AskAsync_with_serving_local_model_persists_LocalLlama_provider_and_claim_scoped_citations()
    {
        var factory = RagInfrastructureTestHelper.MakeFactory(
            nameof(AskAsync_with_serving_local_model_persists_LocalLlama_provider_and_claim_scoped_citations));
        await using (var db = factory.CreateDbContext())
            await RagSeeder.SeedAsync(db, Embed);

        var fake = new FakeLocalLlamaClient(reply: "Грунтована відповідь на основі доказів справи.");
        var options = new RagOptions { EmbeddingDimensions = Embed.Dimensions, DefaultTopK = 4, LocalLlamaEnabled = true };
        var generator = new LocalLlamaGroundedAnswerGenerator(options, new MockGroundedAnswerGenerator(), fake);
        var svc = new RagService(new DbRagChunkSource(factory), new RagRetrievalService(Embed),
            generator, factory, options, Embed, new AlwaysReachableProbe(), router: null);

        var answer = await svc.AskAsync("CLM-1006", "Чому справа ризикова?", "risk", "corr-ollama");

        Assert.Equal(LocalLlamaGroundedAnswerGenerator.LocalProviderMode, answer.ProviderMode);
        Assert.True(fake.Calls >= 1);
        Assert.NotEmpty(answer.Citations);
        // Grounding/leakage: every cited + retrieved chunk belongs to the asked claim.
        Assert.All(answer.Citations, c => Assert.StartsWith("CLM-1006", c.ChunkId));
        Assert.All(answer.RetrievedChunkIds, id => Assert.StartsWith("CLM-1006", id));
        Assert.True(answer.AdvisoryOnly);
    }

    private sealed class AlwaysReachableProbe : IRagRuntimeProbe
    {
        public Task<bool> IsReachableAsync(string endpoint, string? healthPath, CancellationToken ct = default) =>
            Task.FromResult(true);
    }
}

/// <summary>
/// In-memory fake of <see cref="ILocalLlamaClient"/>. Models a serving local model (returns a fixed reply)
/// or an outage (returns null, like a timeout / unreachable endpoint). Captures the last prompts so a test
/// can assert the grounded context was actually sent.
/// </summary>
internal sealed class FakeLocalLlamaClient : ILocalLlamaClient
{
    private readonly bool _outage;
    private readonly string _reply;

    public int Calls { get; private set; }
    public string LastSystemPrompt { get; private set; } = string.Empty;
    public string LastUserPrompt { get; private set; } = string.Empty;

    public FakeLocalLlamaClient(string reply = "ok", bool outage = false)
    {
        _reply = reply;
        _outage = outage;
    }

    public LocalLlamaCompletion? TryComplete(string systemPrompt, string userPrompt, CancellationToken ct = default)
    {
        Calls++;
        LastSystemPrompt = systemPrompt;
        LastUserPrompt = userPrompt;
        if (_outage) return null; // simulate unreachable / timeout / non-2xx
        return new LocalLlamaCompletion(_reply, PromptTokens: 42, CompletionTokens: 17, Model: "llama3.2:1b");
    }
}
