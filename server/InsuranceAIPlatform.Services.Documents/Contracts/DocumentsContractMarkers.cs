namespace InsuranceAIPlatform.Services.Documents.Contracts;

/// <summary>
/// Forward-declaration marker for the Documents service's future read contract.
/// Id-only placeholder — no business fields, no PII, no file bytes — formalised in a later gate.
/// </summary>
public record DocumentRef(string DocumentId);
