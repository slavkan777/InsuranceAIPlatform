using InsuranceAIPlatform.Services.Claims.Persistence;
using InsuranceAIPlatform.Services.CustomersPolicies.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAIPlatform.DbMigrator;

/// <summary>
/// Lease 15 follow-up (critic finding) — a SEVENTH, separately recorded step.
/// The recorded v1..v6 contracts are NOT edited.
///
/// The independent live-site critic swept every claim detail and customer page and
/// found Cyrillic in free-typed DEMO DATA the earlier steps never saw: claim
/// descriptions/locations typed through the running product, and one hand-created
/// customer. List endpoints do not expose these columns, which is why the earlier
/// endpoint censuses read 0.
///
/// Strategy, two tiers per column (Claims.Description, Claims.Location,
/// SyntheticCustomers.FullName, SyntheticCustomers.AddressLine):
///   1. Exact whole-value map for every OBSERVED value — faithful literal
///      translation / transliteration, keeping the demo's informal tone.
///   2. Generic fallback for ANY OTHER Cyrillic value in these columns — a truthful
///      neutral English sentence per column. This closes the class, not just the
///      observed rows, so unseen free-typed rows cannot resurface later.
///
/// Contract: UPDATE only; no delete/truncate/id change; idempotent (replacements
/// contain no Cyrillic); every fallback replacement is counted and sampled in the
/// report so nothing is silently rewritten.
/// </summary>
public static class EnglishOnlyLiveResidueBackfill
{
    public const string Version = "english-only-live-residue-v7";

    // Truthful neutral fallbacks — one per column, stating what the row is.
    public const string NeutralDescription =
        "Synthetic demo incident description migrated to English-only mode.";
    public const string NeutralLocation = "Springfield";
    public const string NeutralFullName = "Synthetic Demo Customer";
    public const string NeutralAddress = "Springfield, Main Street 1";

    /// <summary>
    /// Observed live values -> faithful literal English equivalents.
    /// «Київ»-style locations follow the established v5 convention (Springfield).
    /// </summary>
    private static readonly Dictionary<string, string> KnownValues = new(StringComparer.Ordinal)
    {
        // Claim descriptions (free-typed, informal tone preserved)
        ["Зіткнення, пошкодження бампера."] = "Collision, bumper damage.",
        ["випадок страховий"] = "Insurance incident",
        ["Подруга на рено вїхала)))"] = "A friend in a Renault drove into it)))",
        ["Трохи коцнув"] = "Scraped it a little",
        // Claim location (garbled demo text; city per v5 Київ -> Springfield convention)
        ["Полытей ,Киъв"] = "Springfield",
        // Customer directory (transliteration for the name; v5 address convention)
        ["Ігор Крузак))))"] = "Ihor Kruzak))))",
        ["Киъв, грушевского 6"] = "Springfield, Main Street 6",
        ["Киев"] = "Springfield",
    };

    public static bool HasCyrillic(string? s) =>
        !string.IsNullOrEmpty(s) && s.Any(c => c >= 'Ѐ' && c <= 'ӿ');

    /// <summary>Known literal first; otherwise the column's truthful neutral fallback.</summary>
    public static string MapValue(string current, string neutralFallback) =>
        KnownValues.TryGetValue(current.Trim(), out var mapped) ? mapped : neutralFallback;

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
        ClaimsDbContext claims,
        CustomersPoliciesDbContext customers,
        DbContext checkpointDb,
        CancellationToken ct)
    {
        var r = new BackfillReport { Version = Version };

        foreach (var c in await claims.Claims.ToListAsync(ct))
        {
            var touched = false;

            if (HasCyrillic(c.Description))
            {
                var mapped = MapValue(c.Description, NeutralDescription);
                if (mapped == NeutralDescription)
                    r.Skip($"Claim[{c.ClaimId}].Description: no literal map — neutral fallback applied");
                c.Description = mapped;
                touched = true;
            }

            if (HasCyrillic(c.Location))
            {
                var mapped = MapValue(c.Location, NeutralLocation);
                if (mapped == NeutralLocation && !KnownValues.ContainsKey(c.Location.Trim()))
                    r.Skip($"Claim[{c.ClaimId}].Location: no literal map — neutral fallback applied");
                c.Location = mapped;
                touched = true;
            }

            if (touched) r.Updated++; else r.AlreadyEnglish++;
        }
        await claims.SaveChangesAsync(ct);

        foreach (var cu in await customers.SyntheticCustomers.ToListAsync(ct))
        {
            var touched = false;

            if (HasCyrillic(cu.FullName))
            {
                var mapped = MapValue(cu.FullName, NeutralFullName);
                if (mapped == NeutralFullName && !KnownValues.ContainsKey(cu.FullName.Trim()))
                    r.Skip($"Customer[{cu.Id}].FullName: no literal map — neutral fallback applied");
                cu.FullName = mapped;
                touched = true;
            }

            if (HasCyrillic(cu.AddressLine))
            {
                var mapped = MapValue(cu.AddressLine, NeutralAddress);
                if (mapped == NeutralAddress && !KnownValues.ContainsKey(cu.AddressLine.Trim()))
                    r.Skip($"Customer[{cu.Id}].AddressLine: no literal map — neutral fallback applied");
                cu.AddressLine = mapped;
                touched = true;
            }

            if (touched) r.Updated++; else r.AlreadyEnglish++;
        }
        await customers.SaveChangesAsync(ct);

        await RecordAsync(checkpointDb, r, ct);
        return r;
    }
}
