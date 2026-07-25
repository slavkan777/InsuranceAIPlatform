using InsuranceAIPlatform.BuildingBlocks;

namespace InsuranceAIPlatform.Tests;

/// <summary>
/// Contract-code normalization: legacy Ukrainian persisted values must map to stable English
/// codes at the API boundary, canonical codes must be idempotent, and unrecognised values must
/// stay explicit (returned verbatim, reported as not-known) instead of being silently classified.
/// </summary>
public class ClaimContractCodesTests
{
    // ---- Legacy -> code -------------------------------------------------------------------

    [Theory]
    [InlineData("Новий", ClaimContractCodes.Status.New)]
    [InlineData("В роботі", ClaimContractCodes.Status.InProgress)]
    [InlineData("Збір документів", ClaimContractCodes.Status.CollectingDocuments)]
    [InlineData("AI-обробка", ClaimContractCodes.Status.AiProcessing)]
    [InlineData("Високий ризик", ClaimContractCodes.Status.HighRisk)]
    [InlineData("Готова", ClaimContractCodes.Status.Ready)]
    [InlineData("Завершено", ClaimContractCodes.Status.Completed)]
    public void NormalizeStatus_maps_legacy_ukrainian_to_code(string legacy, string expected)
    {
        Assert.Equal(expected, ClaimContractCodes.NormalizeStatus(legacy));
        Assert.True(ClaimContractCodes.IsKnownStatus(legacy));
    }

    [Theory]
    [InlineData("Невизначений", ClaimContractCodes.Risk.Undetermined)]
    [InlineData("Низький", ClaimContractCodes.Risk.Low)]
    [InlineData("Середній", ClaimContractCodes.Risk.Medium)]
    [InlineData("Високий", ClaimContractCodes.Risk.High)]
    public void NormalizeRisk_maps_legacy_ukrainian_to_code(string legacy, string expected)
    {
        Assert.Equal(expected, ClaimContractCodes.NormalizeRisk(legacy));
        Assert.True(ClaimContractCodes.IsKnownRisk(legacy));
    }

    [Theory]
    [InlineData("Очікує AI", ClaimContractCodes.AiStatus.AwaitingAi)]
    [InlineData("AI-перевірено", ClaimContractCodes.AiStatus.AiVerified)]
    [InlineData("Потрібна перевірка", ClaimContractCodes.AiStatus.NeedsReview)]
    [InlineData("Очікує документи", ClaimContractCodes.AiStatus.AwaitingDocuments)]
    [InlineData("Обробляється", ClaimContractCodes.AiStatus.Processing)]
    [InlineData("Готова", ClaimContractCodes.AiStatus.Ready)]
    public void NormalizeAiStatus_maps_legacy_ukrainian_to_code(string legacy, string expected)
    {
        Assert.Equal(expected, ClaimContractCodes.NormalizeAiStatus(legacy));
        Assert.True(ClaimContractCodes.IsKnownAiStatus(legacy));
    }

    // ---- Idempotency ----------------------------------------------------------------------

    [Theory]
    [InlineData(ClaimContractCodes.Status.New)]
    [InlineData(ClaimContractCodes.Status.InProgress)]
    [InlineData(ClaimContractCodes.Status.CollectingDocuments)]
    [InlineData(ClaimContractCodes.Status.AiProcessing)]
    [InlineData(ClaimContractCodes.Status.HighRisk)]
    [InlineData(ClaimContractCodes.Status.Ready)]
    [InlineData(ClaimContractCodes.Status.Completed)]
    public void NormalizeStatus_is_idempotent_for_codes(string code)
    {
        Assert.Equal(code, ClaimContractCodes.NormalizeStatus(code));
        Assert.Equal(code, ClaimContractCodes.NormalizeStatus(ClaimContractCodes.NormalizeStatus(code)));
    }

    [Theory]
    [InlineData(ClaimContractCodes.Risk.Undetermined)]
    [InlineData(ClaimContractCodes.Risk.Low)]
    [InlineData(ClaimContractCodes.Risk.Medium)]
    [InlineData(ClaimContractCodes.Risk.High)]
    public void NormalizeRisk_is_idempotent_for_codes(string code)
    {
        Assert.Equal(code, ClaimContractCodes.NormalizeRisk(code));
        Assert.Equal(code, ClaimContractCodes.NormalizeRisk(ClaimContractCodes.NormalizeRisk(code)));
    }

    [Theory]
    [InlineData(ClaimContractCodes.AiStatus.AwaitingAi)]
    [InlineData(ClaimContractCodes.AiStatus.AiVerified)]
    [InlineData(ClaimContractCodes.AiStatus.NeedsReview)]
    [InlineData(ClaimContractCodes.AiStatus.AwaitingDocuments)]
    [InlineData(ClaimContractCodes.AiStatus.Processing)]
    [InlineData(ClaimContractCodes.AiStatus.Ready)]
    public void NormalizeAiStatus_is_idempotent_for_codes(string code)
    {
        Assert.Equal(code, ClaimContractCodes.NormalizeAiStatus(code));
        Assert.Equal(code, ClaimContractCodes.NormalizeAiStatus(ClaimContractCodes.NormalizeAiStatus(code)));
    }

    // ---- Unknown values stay explicit -----------------------------------------------------

    [Theory]
    [InlineData("Ліквідовано")]
    [InlineData("SomethingElse")]
    [InlineData("in progress")]
    public void NormalizeStatus_returns_unknown_verbatim_and_reports_not_known(string raw)
    {
        Assert.Equal(raw, ClaimContractCodes.NormalizeStatus(raw));
        Assert.False(ClaimContractCodes.IsKnownStatus(raw));
    }

    [Fact]
    public void Unknown_risk_and_ai_status_are_not_coerced_into_a_real_state()
    {
        Assert.Equal("Критичний", ClaimContractCodes.NormalizeRisk("Критичний"));
        Assert.False(ClaimContractCodes.IsKnownRisk("Критичний"));

        Assert.Equal("Escalated", ClaimContractCodes.NormalizeAiStatus("Escalated"));
        Assert.False(ClaimContractCodes.IsKnownAiStatus("Escalated"));
    }

    // ---- Null / empty / whitespace --------------------------------------------------------

    [Fact]
    public void Null_and_blank_input_is_safe_and_not_known()
    {
        Assert.Equal(string.Empty, ClaimContractCodes.NormalizeStatus(null));
        Assert.Equal("   ", ClaimContractCodes.NormalizeRisk("   "));
        Assert.Equal(string.Empty, ClaimContractCodes.NormalizeAiStatus(""));

        Assert.False(ClaimContractCodes.IsKnownStatus(null));
        Assert.False(ClaimContractCodes.IsKnownRisk("   "));
        Assert.False(ClaimContractCodes.IsKnownAiStatus(""));
    }

    [Fact]
    public void Legacy_lookup_tolerates_surrounding_whitespace_and_case()
    {
        Assert.Equal(ClaimContractCodes.Status.InProgress, ClaimContractCodes.NormalizeStatus("  В роботі  "));
        Assert.Equal(ClaimContractCodes.Risk.High, ClaimContractCodes.NormalizeRisk("високий"));
    }
}
