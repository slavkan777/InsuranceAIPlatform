using System.Security.Cryptography;
using System.Text;
using InsuranceAIPlatform.Services.AiAnalysis.Persistence;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Embedding;
using InsuranceAIPlatform.Services.Approval.Persistence;
using InsuranceAIPlatform.Services.AuditCost.Persistence;
using InsuranceAIPlatform.Services.Claims.Persistence;
using InsuranceAIPlatform.Services.CustomersPolicies.Persistence;
using InsuranceAIPlatform.Services.Documents.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAIPlatform.DbMigrator;

/// <summary>Counters for one backfill run.</summary>
public sealed class BackfillReport
{
    public string Version { get; init; } = EnglishOnlyBackfill.Version;
    public int Updated { get; set; }
    public int AlreadyEnglish { get; set; }
    public int Skipped { get; set; }
    public int ChunksReEmbedded { get; set; }
    public List<string> SkippedSamples { get; } = new();

    public void Skip(string what)
    {
        Skipped++;
        if (SkippedSamples.Count < 20) SkippedSamples.Add(what);
    }
}

/// <summary>
/// Versioned, idempotent, NON-DESTRUCTIVE English-only data backfill.
///
/// Contract:
///   - Updates ONLY rows whose current value is a recognised legacy (Ukrainian) demo
///     value, or — for the RAG corpus — a known synthetic id whose stored text still
///     contains Cyrillic. Anything else is left untouched and counted as skipped.
///   - NEVER deletes, truncates or drops. It performs UPDATEs only.
///   - Preserves every primary key, foreign key and audit row: ids and relationships
///     are never rewritten, only display/content fields.
///   - Safe to rerun: a second pass finds nothing recognisable left and reports
///     0 updated. Row counts are unchanged by construction (no inserts, no deletes).
///   - RAG text changes recompute the embedding, the chunk hash and the language tag,
///     so no stale vector and no duplicate Ukrainian chunk can survive.
///   - Never guesses: an unrecognised value is reported, not translated.
/// </summary>
public static class EnglishOnlyBackfill
{
    public const string Version = "english-only-v1";

    // ---------------------------------------------------------------------
    // Recognised legacy demo values. Exact match only — this is the guard that
    // stops the backfill touching real or unknown data.
    // ---------------------------------------------------------------------

    private static readonly Dictionary<string, string> People = new(StringComparer.Ordinal)
    {
        ["Роберт Джонсон"] = "Robert Johnson",
        ["Марія Коваль"] = "Maria Coval",
        ["Іван Петренко"] = "Ivan Petrenko",
        ["Олена Шевченко"] = "Elena Shevchenko",
    };

    private static readonly Dictionary<string, string> Locations = new(StringComparer.Ordinal)
    {
        ["Бориспіль, вул. Київська 24"] = "Springfield, Main Street 24",
        ["Київ, вул. Шевченка 5"] = "Riverside, Oak Avenue 5",
        ["Харків, пр. Перемоги 12"] = "Fairview, Park Avenue 12",
        ["Одеса, вул. Лесі Українки 7"] = "Greenville, Maple Street 7",
        ["Дніпро, вул. Грушевського 3"] = "Madison, Cedar Road 3",
    };

    private static readonly Dictionary<string, string> Descriptions = new(StringComparer.Ordinal)
    {
        ["Зіткнення на перехресті при здійсненні маневру повороту праворуч."] =
            "Collision at an intersection while performing a right-turn manoeuvre.",
        ["Пошкодження під час паркування."] = "Damage sustained while parking.",
        ["Зіткнення на світлофорі."] = "Collision at a traffic light.",
        ["Пошкодження кузова."] = "Bodywork damage.",
        ["ДТП на перехресті."] = "Road accident at an intersection.",
    };

    private static readonly Dictionary<string, string> EventTypes = new(StringComparer.Ordinal)
    {
        ["ДТП"] = "RoadAccident",
        ["Паркування"] = "Parking",
        ["Зіткнення"] = "Collision",
        ["Пошкодження"] = "Damage",
        ["Скло"] = "Glass",
        ["Угон"] = "Theft",
        ["Викрадення"] = "Theft",
    };

    private static readonly Dictionary<string, string> Statuses = new(StringComparer.Ordinal)
    {
        ["Новий"] = "New",
        ["В роботі"] = "InProgress",
        ["Збір документів"] = "CollectingDocuments",
        ["AI-обробка"] = "AiProcessing",
        ["Високий ризик"] = "HighRisk",
        ["Готова"] = "Ready",
        ["Завершено"] = "Completed",
    };

    private static readonly Dictionary<string, string> Risks = new(StringComparer.Ordinal)
    {
        ["Невизначений"] = "Undetermined",
        ["Низький"] = "Low",
        ["Середній"] = "Medium",
        ["Високий"] = "High",
    };

    private static readonly Dictionary<string, string> DocumentTitles = new(StringComparer.Ordinal)
    {
        ["Заява клієнта"] = "Customer statement",
        ["Поліцейський звіт"] = "Police report",
        ["Фото — переднє"] = "Photo — front",
        ["Фото — бокове"] = "Photo — side",
        ["Фото — задній бампер"] = "Photo — rear bumper",
        ["Рахунок СТО"] = "Repair invoice",
        ["Умови полісу"] = "Policy terms",
    };

    private static readonly Dictionary<string, string> DocumentMeta = new(StringComparer.Ordinal)
    {
        ["NoБРС-2026/05/441"] = "No. PR-2026/05/441",
        ["Сума +38%"] = "Amount +38%",
        ["Сума OK"] = "Amount OK",
        ["ВІДСУТНЄ"] = "MISSING",
        ["ХАРКІВ-12345"] = "RIVERSIDE-12345",
    };

    private static readonly Dictionary<string, string> MissingDocuments = new(StringComparer.Ordinal)
    {
        ["Фото пошкодження заднього бампера"] = "Rear bumper damage photo",
        ["Рахунок СТО"] = "Repair invoice",
        ["Поліцейський звіт"] = "Police report",
    };

    private static readonly Dictionary<string, string> AiText = new(StringComparer.Ordinal)
    {
        ["Документи"] = "Documents",
        ["Оцінка збитку"] = "Damage estimate",
        ["Покриття"] = "Coverage",
        ["Загальне"] = "General",
        ["Відсутнє фото заднього бампера. 6 з 7 документів надано."] =
            "Rear bumper photo is missing. 6 of 7 documents provided.",
        ["Оцінка $2720 перевищує бенчмарк $1970 на 38%."] =
            "Estimate $2,720 exceeds the $1,970 benchmark by 38%.",
        ["Подія ДТП підпадає під Auto Comprehensive. Франшиза $500 застосовна."] =
            "The road accident is covered by Auto Comprehensive. The $500 deductible applies.",
        ["Поліцейський звіт"] = "Police report",
        ["Рахунок СТО"] = "Repair invoice",
        ["Підтверджено факт ДТП 18.05.2026, Бориспіль."] =
            "Road accident on 18.05.2026 in Springfield confirmed.",
        ["Загальна сума $2720. Деталізація: бампер $980, лак $740, кузов $1000."] =
            "Total $2,720. Breakdown: bumper $980, paint $740, bodywork $1,000.",
        ["Сума ремонту вище очікуваного діапазону"] = "Repair amount above the expected range",
        ["Відсутнє фото пошкодження"] = "Damage photo missing",
        ["Розбіжності у поясненнях водіїв"] = "Discrepancies between driver statements",
        ["Confidence нижче порогу 85%"] = "Confidence below the 85% threshold",
    };

    private static readonly Dictionary<string, string> AuditMessages = new(StringComparer.Ordinal)
    {
        ["Запуск аналізу CLM-1006"] = "Analysis started for CLM-1006",
        ["Класифікація 6 документів"] = "Classified 6 documents",
        ["Витягнуто 47 полів"] = "Extracted 47 fields",
        ["Ризик 82/100 — Високий"] = "Risk 82/100 — High",
        ["Рекомендація: запросити фото"] = "Recommendation: request photo",
        ["Авто-погодження заблоковано"] = "Auto-approval blocked",
    };

    private static readonly Dictionary<string, string> CostCategories = new(StringComparer.Ordinal)
    {
        ["Витягування"] = "Extraction",
        ["RAG / докази"] = "RAG / evidence",
        ["Ризик"] = "Risk",
        ["Рекомендація"] = "Recommendation",
    };

    private static readonly Dictionary<string, string> ApprovalText = new(StringComparer.Ordinal)
    {
        ["Запросити додаткові документи"] = "Request additional documents",
        ["Затвердити виплату"] = "Approve payout",
        ["Відхилити заявку"] = "Reject the claim",
        ["Передати до відділу розслідування"] = "Escalate to the investigation unit",
        ["Рекомендовано AI — запросити фото заднього бампера"] = "AI recommended — request the rear bumper photo",
        ["Якщо ризики прийнятні після перевірки"] = "If the risks are acceptable after review",
        ["З обґрунтуванням відмови"] = "With a written justification",
        ["Ескалація для детального розслідування"] = "Escalation for a detailed investigation",
    };

    /// <summary>Colours and address tokens used by the 200 generated synthetic customers.</summary>
    private static readonly Dictionary<string, string> Colors = new(StringComparer.Ordinal)
    {
        ["Білий"] = "White", ["Чорний"] = "Black", ["Срібний"] = "Silver", ["Синій"] = "Blue",
        ["Червоний"] = "Red", ["Сірий"] = "Grey", ["Зелений"] = "Green", ["Бежевий"] = "Beige",
    };

    private static readonly (string From, string To)[] AddressTokens =
    {
        ("Кривий Ріг", "Georgetown"), ("Київ", "Springfield"), ("Харків", "Riverside"),
        ("Одеса", "Fairview"), ("Дніпро", "Madison"), ("Запоріжжя", "Greenville"),
        ("Львів", "Salem"), ("Миколаїв", "Clinton"), ("Бориспіль", "Springfield"),
        ("вул. Шевченка", "Main Street"), ("вул. Лесі Українки", "Oak Avenue"),
        ("вул. Хрещатик", "Maple Street"), ("вул. Грушевського", "Cedar Road"),
        ("вул. Франка", "Park Avenue"), ("пр. Перемоги", "Lake Drive"),
        ("вул. Київська", "Main Street"), ("вул. Тестова", "Test Street"),
    };

    private static bool HasCyrillic(string? s) =>
        !string.IsNullOrEmpty(s) && s.Any(c => c >= 'Ѐ' && c <= 'ӿ');

    private static string Hash(string text) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)))[..16];

    /// <summary>Exact-match translate. Returns false when the value is not recognised.</summary>
    private static bool TryMap(Dictionary<string, string> map, string? current, out string translated)
    {
        translated = current ?? string.Empty;
        if (string.IsNullOrWhiteSpace(current)) return false;
        if (!HasCyrillic(current)) return false;            // already English — nothing to do
        return map.TryGetValue(current.Trim(), out translated!);
    }

    /// <summary>Applies a field map, updating counters. Returns true when the field changed.</summary>
    private static bool Apply(
        BackfillReport r, Dictionary<string, string> map, string entity, string field,
        string? current, Action<string> set)
    {
        if (!HasCyrillic(current)) { r.AlreadyEnglish++; return false; }
        if (TryMap(map, current, out var translated)) { set(translated); r.Updated++; return true; }
        r.Skip($"{entity}.{field}: unrecognised value left untouched");
        return false;
    }

    // ---------------------------------------------------------------------
    // Checkpoint table (additive DDL only — never dropped)
    // ---------------------------------------------------------------------

    private const string EnsureHistoryTableSql = """
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
        await db.Database.ExecuteSqlRawAsync(EnsureHistoryTableSql, ct);
        var rows = await db.Database
            .SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM [dbo].[DataBackfillHistory] WHERE [Version] = {0}", Version)
            .ToListAsync(ct);
        return rows.FirstOrDefault() > 0;
    }

    private static async Task RecordAsync(DbContext db, BackfillReport r, CancellationToken ct)
    {
        // MERGE-free upsert: bump RunCount when the version is already recorded.
        const string sql = """
            IF EXISTS (SELECT 1 FROM [dbo].[DataBackfillHistory] WHERE [Version] = {0})
                UPDATE [dbo].[DataBackfillHistory]
                   SET [AppliedAtUtc] = SYSDATETIMEOFFSET(),
                       [Updated] = [Updated] + {1},
                       [Skipped] = {2},
                       [RunCount] = [RunCount] + 1
                 WHERE [Version] = {0};
            ELSE
                INSERT INTO [dbo].[DataBackfillHistory]([Version],[AppliedAtUtc],[Updated],[Skipped],[RunCount])
                VALUES ({0}, SYSDATETIMEOFFSET(), {1}, {2}, 1);
            """;
        await db.Database.ExecuteSqlRawAsync(
            sql, new object[] { Version, r.Updated, r.Skipped }, ct);
    }

    // ---------------------------------------------------------------------
    // Run
    // ---------------------------------------------------------------------

    public static async Task<BackfillReport> RunAsync(
        ClaimsDbContext claims,
        DocumentsDbContext documents,
        CustomersPoliciesDbContext customers,
        AiAnalysisDbContext ai,
        AuditCostDbContext audit,
        ApprovalDbContext approval,
        IEmbeddingProvider embed,
        CancellationToken ct)
    {
        var r = new BackfillReport();

        // ---- claims -------------------------------------------------------
        foreach (var c in await claims.Claims.ToListAsync(ct))
        {
            Apply(r, People, "Claim", "Customer", c.Customer, v => c.Customer = v);
            ApplyLocation(r, "Claim", c.Location, v => c.Location = v);
            Apply(r, Descriptions, "Claim", "Description", c.Description, v => c.Description = v);
            Apply(r, EventTypes, "Claim", "EventType", c.EventType, v => c.EventType = v);
            Apply(r, Statuses, "Claim", "Status", c.Status, v => c.Status = v);
            Apply(r, Risks, "Claim", "Risk", c.Risk, v => c.Risk = v);
            if (c.MissingDocument is not null)
                Apply(r, MissingDocuments, "Claim", "MissingDocument", c.MissingDocument, v => c.MissingDocument = v);
        }

        foreach (var h in await claims.ClaimStatusHistories.ToListAsync(ct))
        {
            Apply(r, Statuses, "ClaimStatusHistory", "Status", h.Status, v => h.Status = v);
            // Note is templated: "Створено: <actor> (<type>). Локальний sandbox."
            if (HasCyrillic(h.Note))
            {
                var n = h.Note!.Replace("Створено:", "Created by:").Replace("Локальний sandbox.", "Local sandbox.");
                if (!HasCyrillic(n)) { h.Note = n; r.Updated++; }
                else r.Skip("ClaimStatusHistory.Note: unrecognised note left untouched");
            }
        }
        await claims.SaveChangesAsync(ct);

        // ---- documents ----------------------------------------------------
        foreach (var d in await documents.ClaimDocuments.ToListAsync(ct))
        {
            Apply(r, DocumentTitles, "ClaimDocument", "Title", d.Title, v => d.Title = v);
            Apply(r, DocumentMeta, "ClaimDocument", "Meta", d.Meta, v => d.Meta = v);
        }
        await documents.SaveChangesAsync(ct);

        // ---- customers / vehicles ----------------------------------------
        foreach (var cu in await customers.SyntheticCustomers.ToListAsync(ct))
        {
            Apply(r, People, "SyntheticCustomer", "FullName", cu.FullName, v => cu.FullName = v);
            if (HasCyrillic(cu.AddressLine))
            {
                var swapped = ReplaceTokens(cu.AddressLine);
                if (!HasCyrillic(swapped)) { cu.AddressLine = swapped; r.Updated++; }
                else r.Skip("SyntheticCustomer.AddressLine: unrecognised address left untouched");
            }
        }
        foreach (var v in await customers.Vehicles.ToListAsync(ct))
            Apply(r, Colors, "Vehicle", "Color", v.Color, x => v.Color = x);
        await customers.SaveChangesAsync(ct);

        // ---- ai analysis --------------------------------------------------
        foreach (var f in await ai.AiFindings.ToListAsync(ct))
        {
            Apply(r, AiText, "AiFinding", "Category", f.Category, v => f.Category = v);
            Apply(r, AiText, "AiFinding", "Text", f.Text, v => f.Text = v);
        }
        foreach (var e in await ai.AiEvidenceReferences.ToListAsync(ct))
        {
            Apply(r, AiText, "AiEvidenceReference", "Source", e.Source, v => e.Source = v);
            Apply(r, AiText, "AiEvidenceReference", "Note", e.Note, v => e.Note = v);
        }
        foreach (var s in await ai.AiRiskSignals.ToListAsync(ct))
            Apply(r, AiText, "AiRiskSignal", "Label", s.Label, v => s.Label = v);

        // ---- RAG corpus (canonical, id-keyed) + embedding recompute -------
        foreach (var cl in await ai.PolicyClauses.ToListAsync(ct))
        {
            if (!HasCyrillic(cl.Title) && !HasCyrillic(cl.Text)) { r.AlreadyEnglish++; continue; }
            if (EnglishOnlyBackfillCorpus.ClauseText.TryGetValue(cl.ClauseId, out var canon))
            {
                cl.Title = canon.Title;
                cl.Text = canon.Text;
                r.Updated++;
            }
            else r.Skip($"PolicyClause {cl.ClauseId}: not in canonical corpus, left untouched");
        }

        foreach (var q in await ai.RagEvaluationQuestions.ToListAsync(ct))
        {
            if (!HasCyrillic(q.Text) && !HasCyrillic(q.ExpectedAnswerKeywordsCsv)) { r.AlreadyEnglish++; continue; }
            if (EnglishOnlyBackfillCorpus.QuestionText.TryGetValue(q.QuestionId, out var canon))
            {
                q.Text = canon.Text;
                q.ExpectedAnswerKeywordsCsv = canon.Keywords;
                r.Updated++;
            }
            else r.Skip($"RagEvaluationQuestion {q.QuestionId}: not in canonical corpus, left untouched");
        }

        foreach (var ch in await ai.EvidenceChunks.ToListAsync(ct))
        {
            if (!HasCyrillic(ch.Text))
            {
                // Already English — make sure the language tag is not stale.
                if (!string.Equals(ch.Language, "en", StringComparison.OrdinalIgnoreCase))
                {
                    ch.Language = "en";
                    r.Updated++;
                }
                else r.AlreadyEnglish++;
                continue;
            }

            if (!EnglishOnlyBackfillCorpus.ChunkText.TryGetValue(ch.ChunkId, out var text))
            {
                r.Skip($"EvidenceChunk {ch.ChunkId}: not in canonical corpus, left untouched");
                continue;
            }

            // Update text IN PLACE (same ChunkId => no duplicate retrievable chunk)
            // and recompute the vector so no stale embedding survives.
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
        await ai.SaveChangesAsync(ct);

        // ---- audit / cost -------------------------------------------------
        foreach (var a in await audit.AuditEvents.ToListAsync(ct))
            Apply(r, AuditMessages, "AuditEvent", "Message", a.Message, v => a.Message = v);
        foreach (var c in await audit.CostTraces.ToListAsync(ct))
            Apply(r, CostCategories, "CostTrace", "Category", c.Category, v => c.Category = v);
        await audit.SaveChangesAsync(ct);

        // ---- approval -----------------------------------------------------
        foreach (var d in await approval.ApprovalDrafts.ToListAsync(ct))
            Apply(r, ApprovalText, "ApprovalDraft", "AiRecommendation", d.AiRecommendation, v => d.AiRecommendation = v);
        foreach (var o in await approval.ApprovalDecisionOptions.ToListAsync(ct))
        {
            Apply(r, ApprovalText, "ApprovalDecisionOption", "Label", o.Label, v => o.Label = v);
            Apply(r, ApprovalText, "ApprovalDecisionOption", "Rationale", o.Rationale, v => o.Rationale = v);
        }
        await approval.SaveChangesAsync(ct);

        await RecordAsync(claims, r, ct);
        return r;
    }

    /// <summary>
    /// Locations are either one of the five hand-written golden addresses or a generated
    /// "&lt;city&gt;, &lt;street&gt; N" built from known synthetic tokens. Exact map first, then
    /// token swap; only if BOTH fail is the row reported as skipped and left untouched.
    /// </summary>
    private static void ApplyLocation(BackfillReport r, string entity, string? current, Action<string> set)
    {
        if (!HasCyrillic(current)) { r.AlreadyEnglish++; return; }
        if (TryMap(Locations, current, out var exact)) { set(exact); r.Updated++; return; }

        var swapped = ReplaceTokens(current);
        if (!HasCyrillic(swapped)) { set(swapped); r.Updated++; return; }

        r.Skip($"{entity}.Location: unrecognised address left untouched");
    }

    private static string ReplaceTokens(string? value)
    {
        var s = value ?? string.Empty;
        foreach (var (from, to) in AddressTokens) s = s.Replace(from, to);
        return s;
    }
}
