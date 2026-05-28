using InsuranceAIPlatform.BuildingBlocks;
using InsuranceAIPlatform.Services.AiAnalysis.Contracts;

namespace InsuranceAIPlatform.Services.AiAnalysis.Providers;

/// <summary>
/// Deterministic mock AI provider. Returns the golden CLM-1006 shape for that claim,
/// and a safe generic stub for all other claims.
/// No HTTP, no external service, no key read, no network call of any kind.
/// </summary>
public sealed class MockAiProvider : IAiProvider
{
    public AiProviderMode Mode => AiProviderMode.Mock;

    public Task<AiProviderRawOutput> AnalyzeAsync(AiAnalysisRequest request, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        AiProviderRawOutput output = request.ClaimId == SeedConstants.GoldenClaimId
            ? BuildGoldenOutput()
            : BuildGenericStub(request.ClaimId);

        return Task.FromResult(output);
    }

    // -----------------------------------------------------------------------
    // CLM-1006 deterministic golden output — matches AiAnalysisSeeder exactly.
    // -----------------------------------------------------------------------

    private static AiProviderRawOutput BuildGoldenOutput() => new(
        ModelName: "local-mock-v0.1",
        SummaryText:
            "AI аналіз виявив 2 попереджувальні та 1 нейтральну знахідку. " +
            "Оцінка збитку перевищує бенчмарк, відсутні деякі фото. " +
            "Покриття підтверджено. Рекомендується перевірка людиною.",
        Findings:
        [
            new AiFindingDraft("f1", "Документи",     "Відсутнє фото заднього бампера. 6 з 7 документів надано.",                        "warn"),
            new AiFindingDraft("f2", "Оцінка збитку", "Оцінка $2720 перевищує бенчмарк $1970 на 38%.",                                    "warn"),
            new AiFindingDraft("f3", "Покриття",      "Подія ДТП підпадає під Auto Comprehensive. Франшиза $500 застосовна.", "ok"),
        ],
        Evidence:
        [
            new AiEvidenceDraft("e1", "Поліцейський звіт", "Підтверджено факт ДТП 18.05.2026, Бориспіль.",                                        95),
            new AiEvidenceDraft("e2", "Рахунок СТО",       "Загальна сума $2720. Деталізація: бампер $980, лак $740, кузов $1000.",                87),
        ],
        Risks:
        [
            new AiRiskDraft("rs1", "Сума ремонту вище очікуваного діапазону",   25),
            new AiRiskDraft("rs2", "Відсутнє фото пошкодження",                 22),
            new AiRiskDraft("rs3", "Розбіжності у поясненнях водіїв",           18),
            new AiRiskDraft("rs4", "Confidence нижче порогу 85%",                9),
        ],
        RecommendedActionText: "Запросіть відсутнє фото бампера. Перевірте рахунок СТО. Рішення лише за людиною-ад'ютантом.",
        PolicyExplanationText: "Поліс Auto Comprehensive POL-2025-AC-4421 покриває збитки від ДТП після застосування франшизи $500.",
        ConfidenceScore: 78,
        Tokens: 4261,
        Cost: 0.0187m);

    // -----------------------------------------------------------------------
    // Generic stub for all other claims.
    // -----------------------------------------------------------------------

    private static AiProviderRawOutput BuildGenericStub(string claimId) => new(
        ModelName: "local-mock-v0.1",
        SummaryText: $"AI аналіз для {claimId} — в очікуванні даних.",
        Findings:
        [
            new AiFindingDraft("f1", "Загальне", "AI analysis pending — insufficient claim data for detailed analysis.", "ok"),
        ],
        Evidence: Array.Empty<AiEvidenceDraft>(),
        Risks:
        [
            new AiRiskDraft("rs1", "Insufficient data", 15),
        ],
        RecommendedActionText: "Gather all required documents before proceeding. Human adjuster review required.",
        PolicyExplanationText: "Policy coverage analysis requires complete claim data.",
        ConfidenceScore: 60,
        Tokens: 1500,
        Cost: 0.0070m);
}
