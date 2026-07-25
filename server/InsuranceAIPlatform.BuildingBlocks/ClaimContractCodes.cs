namespace InsuranceAIPlatform.BuildingBlocks;

/// <summary>
/// Stable, language-neutral codes for the claim <c>Status</c> / <c>Risk</c> / <c>AiStatus</c>
/// contract, plus normalization of legacy Ukrainian values that are already persisted.
///
/// Design rules (see Lease 3 contract):
///   - Codes are API/domain values. They are NEVER display labels — the frontend owns
///     presentation and maps code -> English label.
///   - Normalization is applied at the read/API boundary so already-persisted legacy rows
///     keep working without any database write or migration.
///   - An unrecognised value is returned VERBATIM and is never coerced into a real state.
///     Callers can detect that case with the <c>IsKnown*</c> predicates. This keeps unknown
///     data explicit and safe, and preserves the raw value for display/diagnostics.
/// </summary>
public static class ClaimContractCodes
{
    /// <summary>Claim lifecycle status codes.</summary>
    public static class Status
    {
        public const string New = "New";
        public const string InProgress = "InProgress";
        public const string CollectingDocuments = "CollectingDocuments";
        public const string AiProcessing = "AiProcessing";
        public const string HighRisk = "HighRisk";
        public const string Ready = "Ready";
        public const string Completed = "Completed";
    }

    /// <summary>Claim risk-level codes.</summary>
    public static class Risk
    {
        public const string Undetermined = "Undetermined";
        public const string Low = "Low";
        public const string Medium = "Medium";
        public const string High = "High";
    }

    /// <summary>AI pipeline status codes for a claim.</summary>
    public static class AiStatus
    {
        public const string AwaitingAi = "AwaitingAi";
        public const string AiVerified = "AiVerified";
        public const string NeedsReview = "NeedsReview";
        public const string AwaitingDocuments = "AwaitingDocuments";
        public const string Processing = "Processing";
        public const string Ready = "Ready";
    }

    private static readonly HashSet<string> StatusCodes = new(StringComparer.Ordinal)
    {
        Status.New, Status.InProgress, Status.CollectingDocuments,
        Status.AiProcessing, Status.HighRisk, Status.Ready, Status.Completed,
    };

    private static readonly HashSet<string> RiskCodes = new(StringComparer.Ordinal)
    {
        Risk.Undetermined, Risk.Low, Risk.Medium, Risk.High,
    };

    private static readonly HashSet<string> AiStatusCodes = new(StringComparer.Ordinal)
    {
        AiStatus.AwaitingAi, AiStatus.AiVerified, AiStatus.NeedsReview,
        AiStatus.AwaitingDocuments, AiStatus.Processing, AiStatus.Ready,
    };

    // Legacy Ukrainian values observed in persisted rows and in pre-Lease-3 seed data.
    private static readonly Dictionary<string, string> StatusLegacy = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Новий"] = Status.New,
        ["В роботі"] = Status.InProgress,
        ["Збір документів"] = Status.CollectingDocuments,
        ["AI-обробка"] = Status.AiProcessing,
        ["Високий ризик"] = Status.HighRisk,
        ["Готова"] = Status.Ready,
        ["Завершено"] = Status.Completed,
    };

    private static readonly Dictionary<string, string> RiskLegacy = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Невизначений"] = Risk.Undetermined,
        ["Низький"] = Risk.Low,
        ["Середній"] = Risk.Medium,
        ["Високий"] = Risk.High,
    };

    private static readonly Dictionary<string, string> AiStatusLegacy = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Очікує AI"] = AiStatus.AwaitingAi,
        ["AI-перевірено"] = AiStatus.AiVerified,
        ["Потрібна перевірка"] = AiStatus.NeedsReview,
        ["Очікує документи"] = AiStatus.AwaitingDocuments,
        ["Обробляється"] = AiStatus.Processing,
        ["Готова"] = AiStatus.Ready,
    };

    /// <summary>Maps a persisted/legacy status to its code. Unknown input is returned unchanged.</summary>
    public static string NormalizeStatus(string? raw) => Normalize(raw, StatusCodes, StatusLegacy);

    /// <summary>Maps a persisted/legacy risk level to its code. Unknown input is returned unchanged.</summary>
    public static string NormalizeRisk(string? raw) => Normalize(raw, RiskCodes, RiskLegacy);

    /// <summary>Maps a persisted/legacy AI status to its code. Unknown input is returned unchanged.</summary>
    public static string NormalizeAiStatus(string? raw) => Normalize(raw, AiStatusCodes, AiStatusLegacy);

    /// <summary>True when the value is (or normalizes to) a known status code.</summary>
    public static bool IsKnownStatus(string? raw) => StatusCodes.Contains(NormalizeStatus(raw));

    /// <summary>True when the value is (or normalizes to) a known risk code.</summary>
    public static bool IsKnownRisk(string? raw) => RiskCodes.Contains(NormalizeRisk(raw));

    /// <summary>True when the value is (or normalizes to) a known AI status code.</summary>
    public static bool IsKnownAiStatus(string? raw) => AiStatusCodes.Contains(NormalizeAiStatus(raw));

    private static string Normalize(
        string? raw,
        HashSet<string> codes,
        Dictionary<string, string> legacy)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return raw ?? string.Empty;

        var trimmed = raw.Trim();

        // Already a canonical code.
        if (codes.Contains(trimmed))
            return trimmed;

        // Known legacy value -> canonical code.
        if (legacy.TryGetValue(trimmed, out var code))
            return code;

        // Unknown: return verbatim. Never silently classified as a real state.
        return trimmed;
    }
}
