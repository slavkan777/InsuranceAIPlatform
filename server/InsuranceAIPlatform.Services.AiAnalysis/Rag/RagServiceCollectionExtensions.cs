using InsuranceAIPlatform.Services.AiAnalysis.Rag.Embedding;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Generation;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Retrieval;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceAIPlatform.Services.AiAnalysis.Rag;

/// <summary>
/// DI registration for the local RAG foundation. Requires the AiAnalysis DbContext factory to be
/// registered first (AddAiAnalysisPersistence). Default generator is the deterministic mock; the
/// LocalLlama seam is wired ONLY when <c>RagOptions.LocalLlamaEnabled</c> is true.
/// </summary>
public static class RagServiceCollectionExtensions
{
    public static IServiceCollection AddRagFoundation(this IServiceCollection services, RagOptions options)
    {
        services.AddSingleton(options);
        services.AddSingleton<IEmbeddingProvider>(_ => new DeterministicEmbeddingProvider(options.EmbeddingDimensions));
        services.AddSingleton<IRagRetrievalService, RagRetrievalService>();
        services.AddSingleton<IRagChunkSource, DbRagChunkSource>();

        // Vector retrieval router: prefers Qdrant when the seam is enabled AND an IQdrantVectorClient is
        // registered (the Api layer wires the HTTP client), otherwise the in-process index. The client is
        // resolved with GetService so it stays optional — without it, retrieval is in-memory-hash only.
        services.AddSingleton<IVectorRetrievalRouter>(sp => new VectorRetrievalRouter(
            sp.GetRequiredService<IRagRetrievalService>(),
            sp.GetRequiredService<IEmbeddingProvider>(),
            options,
            sp.GetService<IQdrantVectorClient>()));

        // Mock generator is always available as the safe default + fallback target.
        services.AddSingleton<MockGroundedAnswerGenerator>();
        if (options.LocalLlamaEnabled)
        {
            // ILocalLlamaClient is resolved with GetService so it stays optional: the Api layer wires the
            // HTTP client (HttpOllamaClient). Without it — e.g. Services-layer unit tests — the generator
            // falls back to the deterministic mock instead of attempting a live call.
            services.AddSingleton<IGroundedAnswerGenerator>(sp =>
                new LocalLlamaGroundedAnswerGenerator(
                    options,
                    sp.GetRequiredService<MockGroundedAnswerGenerator>(),
                    sp.GetService<ILocalLlamaClient>()));
        }
        else
        {
            services.AddSingleton<IGroundedAnswerGenerator>(sp => sp.GetRequiredService<MockGroundedAnswerGenerator>());
        }

        services.AddSingleton<IRagService, RagService>();
        return services;
    }
}
