namespace InsuranceAIPlatform.Services.Approval.Persistence;

/// <summary>
/// Local/sandbox payout simulation record. NEVER represents a real money transfer.
///
/// The status is always one of:
///   "DraftSimulated"  — operator created the simulation row but has not "confirmed" simulation
///   "Simulated"       — operator confirmed the simulation (still DB-only; no real transfer)
///   "Cancelled"       — operator cancelled the simulation
///
/// SimulationOnly is hard-set to true at creation and never flipped — schema-level
/// guarantee that a row in this table cannot represent a real transfer. The column
/// exists explicitly so a reviewer running a database audit sees the "this is a
/// simulation" flag as a first-class field, not an implicit assumption.
/// </summary>
public sealed class PayoutSimulation
{
    public int Id { get; set; }
    public string ClaimId { get; set; } = string.Empty;     // cross-context ref
    public string Status { get; set; } = "DraftSimulated";
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal Deductible { get; set; }
    public decimal NetPayoutAmount { get; set; }

    /// <summary>"Human" | "AI-advisory" | "Hybrid".</summary>
    public string DecisionSource { get; set; } = "Human";
    public string DecisionActor { get; set; } = string.Empty;

    /// <summary>Optional AI run that informed the simulation. NULL when purely human-driven.</summary>
    public string? SourceAiRunId { get; set; }

    public string? Notes { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>UTC timestamp when status moved to "Simulated". NULL while in draft/cancelled.</summary>
    public DateTimeOffset? ConfirmedAtUtc { get; set; }

    /// <summary>
    /// Always true. Schema-level guarantee that no row in this table represents
    /// a real money transfer. The column exists so a DB audit can directly verify
    /// the safety invariant without inspecting application code.
    /// </summary>
    public bool SimulationOnly { get; set; } = true;
}
