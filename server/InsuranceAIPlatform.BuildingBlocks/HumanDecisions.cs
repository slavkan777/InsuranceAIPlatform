namespace InsuranceAIPlatform.BuildingBlocks;

/// <summary>
/// Allowed values for the human-controlled decision field.
/// These are the ONLY accepted values for SubmitHumanDecision.
/// AI is never the final authority — all values represent human choices only.
/// No payout, no messaging, no binary operation is triggered by these values.
/// </summary>
public static class HumanDecisions
{
    public const string ApproveForReview       = "ApproveForReview";
    public const string RejectForReview        = "RejectForReview";
    public const string NeedsMoreInformation   = "NeedsMoreInformation";
    public const string RequestDocuments       = "RequestDocuments";

    private static readonly HashSet<string> AllowedSet = new(StringComparer.Ordinal)
    {
        ApproveForReview,
        RejectForReview,
        NeedsMoreInformation,
        RequestDocuments,
    };

    /// <summary>Returns true if <paramref name="decision"/> is in the allowed human-decision set.</summary>
    public static bool IsAllowed(string? decision) =>
        decision is not null && AllowedSet.Contains(decision);

    /// <summary>
    /// Maps a human decision to a requested-status label for the outbox payload.
    /// Does NOT mutate any Claim row — outbox-only.
    /// </summary>
    public static string ToRequestedStatus(string decision) => decision switch
    {
        ApproveForReview     => "PendingApproval",
        RejectForReview      => "PendingRejection",
        NeedsMoreInformation => "AwaitingInformation",
        RequestDocuments     => "AwaitingDocuments",
        _                    => "Unknown",
    };
}
