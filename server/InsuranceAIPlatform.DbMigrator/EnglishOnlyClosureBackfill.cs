using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using InsuranceAIPlatform.Services.AiAnalysis.Persistence;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Embedding;
using InsuranceAIPlatform.Services.AuditCost.Persistence;
using InsuranceAIPlatform.Services.Claims.Persistence;
using InsuranceAIPlatform.Services.CustomersPolicies.Persistence;
using InsuranceAIPlatform.Services.Documents.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAIPlatform.DbMigrator;

/// <summary>
/// Lease 6 closure backfill — a SEPARATE version from <c>english-only-v1</c>, whose
/// recorded contract is deliberately left untouched.
///
/// v1 handled seeded demo data by exact whole-value match. The rows it could not
/// touch are RUNTIME/E2E-created: their values embed Ukrainian fragments inside
/// otherwise-English text (audit messages), or use generated numbers (document
/// titles), so whole-value equality can never match them.
///
/// This pass therefore works at FRAGMENT level, with three hard safety rules:
///   1. Fragments are applied LONGEST-FIRST. (v1 had a real defect here: "Київ" is a
///      prefix of "Київська", so a short token consumed part of a longer one and left
///      a half-translated string, which was then correctly refused. Ordering fixes it.)
///   2. Every fragment has an English counterpart evidenced by a current fixture,
///      seeder or template — nothing is invented.
///   3. POST-CHECK: if a value still contains Cyrillic after substitution, the row is
///      NOT written and is reported as skipped. A partially-translated value can never
///      reach the database.
///
/// UPDATE-only. No deletes, truncates or drops. Ids, foreign keys and claim scoping
/// are never modified.
/// </summary>
public static class EnglishOnlyClosureBackfill
{
    public const string Version = "english-only-closure-v2";

    /// <summary>
    /// Exact Ukrainian fragments observed in runtime rows, each with an evidenced
    /// English counterpart. Applied longest-first (see rule 1).
    /// </summary>
    private static readonly (string From, string To)[] Fragments =
    {
        // Evidenced by UploadDocumentContentModal's sample documents (now English).
        ("ДТП на перехресті — синтетичний тестовий вміст.",
         "Road accident at an intersection — synthetic test content."),
        ("Короткий опис обставин події.", "Short description of the event circumstances."),
        ("Синтетичний тестовий кейс", "Synthetic test case"),
        // Evidenced by MockAiProvider / MockGroundedAnswerGenerator (now English).
        ("AI аналіз виявив 2 попереджувальні та 1 нейтральну знахідку. Оцінка збитку перевищує бенчмарк, відсутні деякі фото. Покриття підтверджено. Рекомендується перевірка людиною.",
         "AI analysis produced 2 warning findings and 1 neutral finding. The damage estimate exceeds the benchmark and some photos are missing. Coverage is confirmed. Human review is recommended."),
        ("Поліс Auto Comprehensive POL-2025-AC-4421 покриває збитки від ДТП після застосування франшизи $500.",
         "Policy Auto Comprehensive POL-2025-AC-4421 covers road-accident damage after the $500 deductible is applied."),
        ("Запросіть відсутнє фото бампера. Перевірте рахунок СТО. Рішення лише за людиною-ад'ютантом.",
         "Request the missing bumper photo. Review the repair invoice. The decision rests solely with the human adjuster."),
        ("AI-аналіз має лише рекомендаційний характер — фінальне рішення приймає людина-ад'юстер.",
         "AI analysis is advisory only — the final decision is made by a human adjuster."),
        ("Недостатньо релевантних доказів у матеріалах справи для відповіді. Рекомендується перегляд людиною.",
         "There is not enough relevant evidence in this claim to answer. Human review is recommended."),
        ("За умовами полісу та матеріалами справи:", "Based on the policy terms and the claim evidence:"),
        ("Перевірка повноти документів у справі:", "Document completeness check for this claim:"),
        ("Пояснення ризик-сигналів (рекомендаційно, без звинувачень):", "Explanation of the risk signals (advisory, no accusations):"),
        ("Справи зі схожими ознаками за наявними доказами:", "Claims with similar characteristics based on the available evidence:"),
        ("Зведення доказів для рішення людини:", "Evidence summary for the human decision:"),
        ("На основі знайдених доказів:", "Based on the retrieved evidence:"),
        ("Спільні категорії доказів:", "Shared evidence categories:"),
        ("Семантична близькість", "Semantic similarity"),
        ("за профілем справи.", "based on the claim profile."),
        ("ДТП на перехресті — синтетичний тестовий вміст", "Road accident at an intersection — synthetic test content"),
        ("Запит на відсутній документ", "Missing document request"),
        ("Потрібен для завершення оцінки", "Required to complete the assessment"),
        // Evidenced by DocumentsSeeder / ClaimsSeeder (now English).
        ("Фото пошкодження заднього бампера", "Rear bumper damage photo"),
        ("Поліцейський звіт", "Police report"),
        ("Рахунок СТО", "Repair invoice"),
        ("Заява клієнта", "Customer statement"),
        // Address tokens — evidenced by CustomersPoliciesSeeder / ClaimsSeeder.
        // NOTE longest-first: "вул. Київська" MUST precede "Київ".
        ("Бориспіль, вул. Київська", "Springfield, Main Street"),
        ("вул. Лесі Українки", "Oak Avenue"),
        ("проспект Перемоги", "Lake Drive"),
        ("вул. Грушевського", "Cedar Road"),
        ("вул. Шевченка", "Main Street"),
        ("вул. Хрещатик", "Maple Street"),
        ("вул. Київська", "Main Street"),
        ("вул. Тестова", "Test Street"),
        ("пр. Перемоги", "Lake Drive"),
        ("вул. Франка", "Park Avenue"),
        ("Бориспіль", "Springfield"),
        ("Запоріжжя", "Greenville"),
        ("Кривий Ріг", "Georgetown"),
        ("Миколаїв", "Clinton"),
        ("Харків", "Riverside"),
        ("Одеса", "Fairview"),
        ("Дніпро", "Madison"),
        ("Львів", "Salem"),
        ("Київ", "Springfield"),
    };

    /// <summary>
    /// Owner-authorized literal replacement (Lease 7), deliberately the narrowest
    /// possible rule: it fires ONLY for these two claim ids AND only when the stored
    /// description is still byte-identical to the malformed legacy value below.
    ///
    /// These two rows are stale artifacts of an earlier version of
    /// 21-created-claim-detail-binding.spec.ts. No current fixture generates this text
    /// any more (the spec now generates "Detail binding regression &lt;stamp&gt;-&lt;tag&gt;"),
    /// so there is no fixture-evidenced 1:1 translation to derive. The owner has
    /// authorized this exact replacement text for these exact ids.
    /// </summary>
    private static readonly string[] AuthorizedDescriptionClaimIds = { "CLM-1032", "CLM-1041" };

    /// <summary>Exact 86-character legacy value, read back from the database.</summary>
    private const string AuthorizedLegacyDescription =
        "Короткий опис обставин події. Синтетичний тестовий кейс для перевірки повного UI-flow.";

    private const string AuthorizedEnglishDescription =
        "Short description of the event circumstances. Synthetic test case.";

    // "Поліцейський звіт NoБРС-2026/1169" -> "Police report No. PR-2026/1169".
    // The generated number is preserved exactly; only the language changes.
    private static readonly Regex PoliceReportNo =
        new(@"Поліцейський звіт\s*No\s*БРС-(\d{4})/(\d+)", RegexOptions.Compiled);

    private static bool HasCyrillic(string? s) =>
        !string.IsNullOrEmpty(s) && s.Any(c => c >= 'Ѐ' && c <= 'ӿ');

    private static string Hash(string text) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)))[..16];

    /// <summary>
    /// Translate known fragments. Returns false when the result would still contain
    /// Cyrillic — in that case the caller must leave the row untouched.
    /// </summary>
    private static bool TryTranslate(string? current, out string result)
    {
        result = current ?? string.Empty;
        if (!HasCyrillic(current)) return false;

        var s = PoliceReportNo.Replace(result, "Police report No. PR-$1/$2");
        foreach (var (from, to) in Fragments) s = s.Replace(from, to);

        if (HasCyrillic(s)) return false;   // refuse partial translation
        result = s;
        return true;
    }

    private const string EnsureHistorySql = """
        IF OBJECT_ID(N'[dbo].[DataBackfillHistory]', N'U') IS NULL
        CREATE TABLE [dbo].[DataBackfillHistory](
            [Version]       NVARCHAR(100)     NOT NULL PRIMARY KEY,
            [AppliedAtUtc]  DATETIMEOFFSET    NOT NULL,
            [Updated]       INT               NOT NULL,
            [Skipped]       INT               NOT NULL,
            [RunCount]      INT               NOT NULL
        );
        """;

    public static async Task<bool> WasAppliedAsync(DbContext db, CancellationToken ct)
    {
        await db.Database.ExecuteSqlRawAsync(EnsureHistorySql, ct);
        var rows = await db.Database
            .SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM [dbo].[DataBackfillHistory] WHERE [Version] = {0}", Version)
            .ToListAsync(ct);
        return rows.FirstOrDefault() > 0;
    }

    private static async Task RecordAsync(DbContext db, BackfillReport r, CancellationToken ct)
    {
        const string sql = """
            IF EXISTS (SELECT 1 FROM [dbo].[DataBackfillHistory] WHERE [Version] = {0})
                UPDATE [dbo].[DataBackfillHistory]
                   SET [AppliedAtUtc] = SYSDATETIMEOFFSET(),
                       [Updated] = [Updated] + {1}, [Skipped] = {2}, [RunCount] = [RunCount] + 1
                 WHERE [Version] = {0};
            ELSE
                INSERT INTO [dbo].[DataBackfillHistory]([Version],[AppliedAtUtc],[Updated],[Skipped],[RunCount])
                VALUES ({0}, SYSDATETIMEOFFSET(), {1}, {2}, 1);
            """;
        await db.Database.ExecuteSqlRawAsync(sql, new object[] { Version, r.Updated, r.Skipped }, ct);
    }

    public static async Task<BackfillReport> RunAsync(
        ClaimsDbContext claims, DocumentsDbContext documents, CustomersPoliciesDbContext customers,
        AiAnalysisDbContext ai, AuditCostDbContext audit,
        IEmbeddingProvider embed, CancellationToken ct)
    {
        var r = new BackfillReport { Version = Version };

        void Field(string entity, string field, string? cur, Action<string> set)
        {
            if (!HasCyrillic(cur)) { r.AlreadyEnglish++; return; }
            if (TryTranslate(cur, out var t)) { set(t); r.Updated++; }
            else r.Skip($"{entity}.{field}: no evidenced English counterpart — left untouched");
        }

        foreach (var c in await claims.Claims.ToListAsync(ct))
        {
            Field($"Claim[{c.ClaimId}]", "Location", c.Location, v => c.Location = v);

            // Owner-authorized exact-value replacement, scoped to two claim ids.
            // Both guards must hold: known id AND byte-identical legacy value.
            if (AuthorizedDescriptionClaimIds.Contains(c.ClaimId, StringComparer.Ordinal)
                && string.Equals(c.Description, AuthorizedLegacyDescription, StringComparison.Ordinal))
            {
                c.Description = AuthorizedEnglishDescription;
                r.Updated++;
                continue;
            }

            Field($"Claim[{c.ClaimId}]", "Description", c.Description, v => c.Description = v);
        }
        await claims.SaveChangesAsync(ct);

        foreach (var d in await documents.ClaimDocuments.ToListAsync(ct))
        {
            Field($"Document[{d.Id}]", "Title", d.Title, v => d.Title = v);
            Field($"Document[{d.Id}]", "Meta", d.Meta, v => d.Meta = v);
            Field($"Document[{d.Id}]", "Content", d.Content, v => d.Content = v);
        }
        foreach (var m in await documents.MissingDocumentRequests.ToListAsync(ct))
        {
            Field($"MissingDocRequest[{m.Id}]", "DocumentTitle", m.DocumentTitle, v => m.DocumentTitle = v);
            Field($"MissingDocRequest[{m.Id}]", "Reason", m.Reason, v => m.Reason = v);
        }
        await documents.SaveChangesAsync(ct);

        foreach (var cu in await customers.SyntheticCustomers.ToListAsync(ct))
            Field($"Customer[{cu.Id}]", "AddressLine", cu.AddressLine, v => cu.AddressLine = v);
        await customers.SaveChangesAsync(ct);

        foreach (var a in await audit.AuditEvents.ToListAsync(ct))
            Field($"AuditEvent[{a.Id}]", "Message", a.Message, v => a.Message = v);
        await audit.SaveChangesAsync(ct);

        // Runtime-uploaded evidence chunks: translate IN PLACE (same ChunkId, same
        // ClaimId => claim scoping preserved, no duplicate retrievable chunk) and
        // recompute hash + vector + language so no stale embedding survives.
        foreach (var ch in await ai.EvidenceChunks.ToListAsync(ct))
        {
            if (!HasCyrillic(ch.Text))
            {
                if (!string.Equals(ch.Language, "en", StringComparison.OrdinalIgnoreCase))
                {
                    ch.Language = "en";
                    r.Updated++;
                }
                else r.AlreadyEnglish++;
                continue;
            }

            if (!TryTranslate(ch.Text, out var text))
            {
                r.Skip($"EvidenceChunk[{ch.ChunkId}]: no evidenced English counterpart — left untouched");
                continue;
            }

            ch.Text = text;
            ch.TokenCount = Math.Max(1, text.Length / 4);
            ch.ChunkHash = Hash(text);
            ch.Language = "en";
            ch.EmbeddingModel = embed.ModelName;
            ch.EmbeddingDim = embed.Dimensions;
            ch.EmbeddingJson = EmbeddingCodec.ToJson(embed.Embed(text));
            r.Updated++;
            r.ChunksReEmbedded++;
        }
        // Persisted AI run output and RAG audit history — user-visible via the AI
        // evidence and audit panels, and missed by the earlier hand-listed census.
        foreach (var run in await ai.AiAnalysisRuns.ToListAsync(ct))
        {
            Field($"AiAnalysisRun[{run.RunId}]", "SummaryText", run.SummaryText, v => run.SummaryText = v);
            Field($"AiAnalysisRun[{run.RunId}]", "PolicyExplanationText", run.PolicyExplanationText, v => run.PolicyExplanationText = v);
            Field($"AiAnalysisRun[{run.RunId}]", "RecommendedActionJson", run.RecommendedActionJson, v => run.RecommendedActionJson = v);
            Field($"AiAnalysisRun[{run.RunId}]", "GuardrailFlagsJson", run.GuardrailFlagsJson, v => run.GuardrailFlagsJson = v);
        }
        foreach (var tr in await ai.RagAuditTraces.ToListAsync(ct))
        {
            Field($"RagAuditTrace[{tr.TraceId}]", "QueryText", tr.QueryText, v => tr.QueryText = v);
            Field($"RagAuditTrace[{tr.TraceId}]", "AnswerText", tr.AnswerText, v => tr.AnswerText = v);
        }
        await ai.SaveChangesAsync(ct);

        await RecordAsync(claims, r, ct);
        return r;
    }
}
