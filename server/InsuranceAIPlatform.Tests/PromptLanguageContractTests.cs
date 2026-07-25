using System.Reflection;
using System.Text.RegularExpressions;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Generation;

namespace InsuranceAIPlatform.Tests;

/// <summary>
/// Regression guard for the Lease 6 closure finding.
///
/// The English-only sweeps searched for Cyrillic CHARACTERS, so a Latin-script
/// instruction like "Respond in Ukrainian" inside a model prompt slipped through
/// every scan while still producing Ukrainian output on the local-LLM path.
/// These tests assert the language contract directly, in both directions:
/// the prompt must demand English, and must not permit Ukrainian.
/// </summary>
public class PromptLanguageContractTests
{
    private static string LocalLlamaSystemPrompt()
    {
        var m = typeof(LocalLlamaGroundedAnswerGenerator)
            .GetMethod("BuildSystemPrompt", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(m);
        return (string)m!.Invoke(null, null)!;
    }

    [Fact]
    public void Ollama_path_system_prompt_requires_English_output()
    {
        Assert.Contains("answer in English", LocalLlamaSystemPrompt(), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("Ukrainian")]
    [InlineData("uk-UA")]
    [InlineData("мовою запитання")]   // "in the language of the question" — the old directive
    public void Ollama_path_system_prompt_does_not_permit_Ukrainian(string banned)
    {
        Assert.DoesNotContain(banned, LocalLlamaSystemPrompt(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Ollama_path_system_prompt_contains_no_Cyrillic()
    {
        Assert.DoesNotMatch(new Regex(@"\p{IsCyrillic}"), LocalLlamaSystemPrompt());
    }

    [Fact]
    public void Ollama_path_system_prompt_keeps_its_advisory_only_guardrails()
    {
        // The language change must not quietly drop the safety clauses.
        var p = LocalLlamaSystemPrompt();
        Assert.Contains("fraud", p, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("human review", p, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("do not invent facts", p, StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Lease 10: newly ingested evidence must be tagged as English at source, so the
/// stale-tag backfill never has to run again for fresh content.
/// </summary>
public class EvidenceIngestionLanguageTests
{
    [Fact]
    public void Ingestion_source_tags_new_chunks_as_english()
    {
        var src = System.IO.File.ReadAllText(LocateIngestionSource());
        Assert.Contains("Language = \"en\"", src);
        Assert.DoesNotContain("Language = \"uk\"", src);
    }

    private static string LocateIngestionSource()
    {
        var dir = new System.IO.DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !System.IO.Directory.Exists(System.IO.Path.Combine(dir.FullName, "InsuranceAIPlatform.Services.AiAnalysis")))
            dir = dir.Parent;
        Assert.NotNull(dir);
        return System.IO.Path.Combine(dir!.FullName, "InsuranceAIPlatform.Services.AiAnalysis",
            "Rag", "Ingestion", "EvidenceIngestionService.cs");
    }
}
