namespace InsuranceAIPlatform.Services.Approval.Contracts;

/// <summary>
/// Forward-declaration marker for the Approval service's future read contract.
/// Id-only placeholder — no business fields, no PII — formalised in a later gate.
/// </summary>
public record ApprovalDraftRef(string ClaimId);
