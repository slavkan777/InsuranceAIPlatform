namespace InsuranceAIPlatform.Services.Claims.Contracts;

/// <summary>
/// Forward-declaration marker for the Claims service's future read contract.
/// Id-only placeholder — no business fields, no PII — to be formalised in a later gate.
/// Cross-service references will be id-only, exactly like this.
/// </summary>
public record ClaimRef(string ClaimId);
