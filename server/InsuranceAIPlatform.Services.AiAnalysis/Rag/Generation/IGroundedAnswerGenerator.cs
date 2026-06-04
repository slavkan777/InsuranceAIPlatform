using InsuranceAIPlatform.Services.AiAnalysis.Rag.Contracts;

namespace InsuranceAIPlatform.Services.AiAnalysis.Rag.Generation;

/// <summary>
/// Produces a grounded answer from ALREADY-retrieved chunks (retrieval-before-generation).
/// Implementations must cite only chunks present in the request, stay advisory-only, and never
/// make a definitive fraud accusation.
/// </summary>
public interface IGroundedAnswerGenerator
{
    /// <summary>Provider mode recorded in the audit trace (e.g. "Mock", "LocalLlama"). Never a real cloud provider.</summary>
    string ProviderMode { get; }

    GroundedDraft Generate(GroundedRequest request);
}
