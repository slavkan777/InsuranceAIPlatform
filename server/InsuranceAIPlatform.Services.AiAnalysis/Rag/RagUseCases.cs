namespace InsuranceAIPlatform.Services.AiAnalysis.Rag;

/// <summary>Canonical RAG use-case identifiers (map 1:1 to the Claim Evidence Intelligence buttons).</summary>
public static class RagUseCases
{
    public const string Coverage = "coverage";        // Check policy coverage
    public const string MissingDocs = "missing_docs"; // Find missing documents
    public const string Risk = "risk";                // Explain risk
    public const string Similar = "similar";          // Find similar claims
    public const string Summary = "summary";          // Prepare approval summary
    public const string Custom = "custom";            // Ask custom question

    public static readonly IReadOnlyList<string> All =
        new[] { Coverage, MissingDocs, Risk, Similar, Summary, Custom };

    /// <summary>Normalizes any input to a known use-case; unknown → <see cref="Custom"/>.</summary>
    public static string Normalize(string? useCase) =>
        (useCase?.Trim().ToLowerInvariant()) switch
        {
            Coverage => Coverage,
            MissingDocs => MissingDocs,
            Risk => Risk,
            Similar => Similar,
            Summary => Summary,
            _ => Custom
        };
}
