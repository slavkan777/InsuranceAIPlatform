using InsuranceAIPlatform.Services.AiAnalysis.Rag;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Contracts;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Embedding;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Generation;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Persistence;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Retrieval;

namespace InsuranceAIPlatform.Tests;

/// <summary>
/// Pure-logic RAG tests (no DB): deterministic embeddings, cosine retrieval ranking, and
/// grounded-answer generation. Covers SEMANTIC (relevance + grounding) and NEGATIVE
/// (no fraud accusation) without any external dependency.
/// </summary>
public class RagFoundationTests
{
    private static readonly DeterministicEmbeddingProvider Embed = new(256);

    // ---- Embedding (MECHANICAL + SEMANTIC) ----

    [Fact]
    public void Embedding_is_deterministic_and_correct_dimension()
    {
        var a = Embed.Embed("поліцейський звіт про ДТП");
        var b = Embed.Embed("поліцейський звіт про ДТП");

        Assert.Equal(256, a.Length);
        Assert.Equal(a, b); // identical input → identical vector
    }

    [Fact]
    public void Embedding_is_l2_normalized_for_nonempty_text()
    {
        var v = Embed.Embed("рахунок СТО на ремонт бампера");
        double norm = Math.Sqrt(v.Sum(x => (double)x * x));
        Assert.InRange(norm, 0.99, 1.01);
    }

    [Fact]
    public void Related_text_is_more_similar_than_unrelated_text()
    {
        var query = Embed.Embed("поліцейський звіт підтвердив дорожньо-транспортну пригоду");
        var related = Embed.Embed("звіт поліції про дорожньо-транспортну пригоду");
        var unrelated = Embed.Embed("рахунок сто лакування кузова сума оплати");

        double simRelated = VectorMath.Cosine(query, related);
        double simUnrelated = VectorMath.Cosine(query, unrelated);

        Assert.True(simRelated > simUnrelated,
            $"Expected related ({simRelated:F3}) > unrelated ({simUnrelated:F3})");
    }

    // ---- Retrieval (SEMANTIC) ----

    [Fact]
    public void Retrieval_ranks_matching_chunk_first_and_respects_topk()
    {
        var chunks = new List<EvidenceChunk>
        {
            MakeChunk("c-police", "Поліцейський звіт підтвердив зіткнення транспортних засобів"),
            MakeChunk("c-invoice", "Рахунок СТО: бампер, лакування, кузовні роботи, сума ремонту"),
            MakeChunk("c-policy", "Поліс покриває збитки від ДТП, франшиза застосовується"),
        };
        var svc = new RagRetrievalService(Embed);

        var top = svc.Rank("яка сума ремонту за рахунком СТО", chunks, 2);

        Assert.Equal(2, top.Count);                       // topK respected
        Assert.Equal("c-invoice", top[0].Chunk.ChunkId);  // best lexical/semantic match first
    }

    [Fact]
    public void Retrieval_on_empty_candidates_returns_empty()
    {
        var svc = new RagRetrievalService(Embed);
        var top = svc.Rank("будь-що", new List<EvidenceChunk>(), 4);
        Assert.Empty(top);
    }

    // ---- Grounded generation (SEMANTIC grounding + NEGATIVE) ----

    [Fact]
    public void Generated_answer_cites_only_retrieved_chunks_and_is_advisory()
    {
        var retrieved = new List<ScoredChunk>
        {
            new(MakeChunk("CLM-1006-invoice#0", "Рахунок СТО на суму 2720 доларів"), 0.8),
            new(MakeChunk("CLM-1006-policy-terms#0", "Поліс покриває ДТП, франшиза 500"), 0.6),
        };
        var gen = new MockGroundedAnswerGenerator();

        var draft = gen.Generate(new GroundedRequest("CLM-1006", RagUseCases.Summary, "Підсумок?", retrieved));

        Assert.Contains(MockGroundedAnswerGenerator.AdvisoryFooter, draft.AnswerText);
        Assert.Equal(2, draft.Citations.Count);
        var retrievedIds = retrieved.Select(r => r.Chunk.ChunkId).ToHashSet();
        Assert.All(draft.Citations, c => Assert.Contains(c.ChunkId, retrievedIds)); // grounding invariant
        Assert.True(draft.Confidence > 0);
    }

    [Fact]
    public void Risk_explanation_never_accuses_fraud()
    {
        var retrieved = new List<ScoredChunk>
        {
            new(MakeChunk("CLM-1006-invoice#1", "Оцінка 2720 перевищує бенчмарк 1970 на 38 відсотків, потребує перевірки людиною"), 0.9),
        };
        var gen = new MockGroundedAnswerGenerator();

        var draft = gen.Generate(new GroundedRequest("CLM-1006", RagUseCases.Risk, "Чому ризик?", retrieved));

        var lower = draft.AnswerText.ToLowerInvariant();
        Assert.DoesNotContain("шахрай", lower);     // no "шахрай"/"шахрайство"
        Assert.DoesNotContain("fraud", lower);
        Assert.DoesNotContain("підробк", lower);    // no "підробка"
    }

    [Fact]
    public void No_retrieved_evidence_yields_low_confidence_honest_answer()
    {
        var gen = new MockGroundedAnswerGenerator();
        var draft = gen.Generate(new GroundedRequest("CLM-1006", RagUseCases.Custom, "Питання?", new List<ScoredChunk>()));

        Assert.Equal(0, draft.Confidence);
        Assert.Contains("Недостатньо", draft.AnswerText);
        Assert.Empty(draft.Citations);
    }

    private static EvidenceChunk MakeChunk(string id, string text)
    {
        var v = Embed.Embed(text);
        return new EvidenceChunk
        {
            ChunkId = id,
            ClaimId = "CLM-1006",
            DocumentId = id,
            Kind = "test",
            Text = text,
            EmbeddingModel = Embed.ModelName,
            EmbeddingDim = Embed.Dimensions,
            EmbeddingJson = EmbeddingCodec.ToJson(v)
        };
    }
}
