using InsuranceAIPlatform.DbMigrator;

namespace InsuranceAIPlatform.Tests;

/// <summary>
/// Direct tests for the Lease 15 follow-up migration (v7), which closes the
/// critic-found Cyrillic in live free-typed detail columns.
///
/// The value it protects: observed values get faithful literal translations;
/// anything unobserved falls back to a truthful neutral sentence — and every
/// replacement string must itself be clean so the step stays idempotent.
/// </summary>
public class LiveResidueBackfillTests
{
    // ---- observed literals -------------------------------------------------

    [Theory]
    [InlineData("Зіткнення, пошкодження бампера.", "Collision, bumper damage.")]
    [InlineData("випадок страховий", "Insurance incident")]
    [InlineData("Подруга на рено вїхала)))", "A friend in a Renault drove into it)))")]
    [InlineData("Трохи коцнув", "Scraped it a little")]
    [InlineData("  Трохи коцнув  ", "Scraped it a little")]   // trimmed match
    public void Observed_descriptions_get_their_literal_translation(string ua, string expected) =>
        Assert.Equal(expected, EnglishOnlyLiveResidueBackfill.MapValue(
            ua, EnglishOnlyLiveResidueBackfill.NeutralDescription));

    [Theory]
    [InlineData("Полытей ,Киъв", "Springfield")]
    [InlineData("Киев", "Springfield")]
    [InlineData("Киъв, грушевского 6", "Springfield, Main Street 6")]
    public void Observed_locations_follow_the_established_springfield_convention(string ua, string expected) =>
        Assert.Equal(expected, EnglishOnlyLiveResidueBackfill.MapValue(
            ua, EnglishOnlyLiveResidueBackfill.NeutralLocation));

    [Fact]
    public void Observed_customer_name_is_transliterated_not_invented() =>
        Assert.Equal("Ihor Kruzak))))", EnglishOnlyLiveResidueBackfill.MapValue(
            "Ігор Крузак))))", EnglishOnlyLiveResidueBackfill.NeutralFullName));

    // ---- unobserved values -> per-column neutral fallback -------------------

    [Fact]
    public void Unobserved_cyrillic_gets_the_columns_neutral_fallback_not_a_guess()
    {
        var mapped = EnglishOnlyLiveResidueBackfill.MapValue(
            "Абсолютно невідомий опис", EnglishOnlyLiveResidueBackfill.NeutralDescription);
        Assert.Equal(EnglishOnlyLiveResidueBackfill.NeutralDescription, mapped);
        Assert.DoesNotContain("невідом", mapped);
    }

    // ---- replacement safety (idempotency precondition) ----------------------

    [Fact]
    public void Every_replacement_string_is_clean_in_both_representations()
    {
        var all = new[]
        {
            EnglishOnlyLiveResidueBackfill.NeutralDescription,
            EnglishOnlyLiveResidueBackfill.NeutralLocation,
            EnglishOnlyLiveResidueBackfill.NeutralFullName,
            EnglishOnlyLiveResidueBackfill.NeutralAddress,
            EnglishOnlyLiveResidueBackfill.MapValue("Зіткнення, пошкодження бампера.", "x"),
            EnglishOnlyLiveResidueBackfill.MapValue("випадок страховий", "x"),
            EnglishOnlyLiveResidueBackfill.MapValue("Подруга на рено вїхала)))", "x"),
            EnglishOnlyLiveResidueBackfill.MapValue("Трохи коцнув", "x"),
            EnglishOnlyLiveResidueBackfill.MapValue("Полытей ,Киъв", "x"),
            EnglishOnlyLiveResidueBackfill.MapValue("Ігор Крузак))))", "x"),
            EnglishOnlyLiveResidueBackfill.MapValue("Киъв, грушевского 6", "x"),
            EnglishOnlyLiveResidueBackfill.MapValue("Киев", "x"),
        };
        foreach (var s in all)
        {
            Assert.False(EnglishOnlyLiveResidueBackfill.HasCyrillic(s));
            Assert.DoesNotContain("\\u04", s, StringComparison.OrdinalIgnoreCase);
            Assert.False(string.IsNullOrWhiteSpace(s));
        }
    }

    [Fact]
    public void Detection_matches_the_program_wide_definition()
    {
        Assert.True(EnglishOnlyLiveResidueBackfill.HasCyrillic("Ігор"));
        Assert.False(EnglishOnlyLiveResidueBackfill.HasCyrillic("Ihor Kruzak))))"));
        Assert.False(EnglishOnlyLiveResidueBackfill.HasCyrillic(""));
        Assert.False(EnglishOnlyLiveResidueBackfill.HasCyrillic(null));
    }
}
