namespace InsuranceAIPlatform.Services.AuditCost.Contracts;

/// <summary>
/// Forward-declaration marker for the Audit &amp; Cost service's future read contract.
/// Correlation-id-only placeholder — no business fields, no PII — formalised in a later gate.
/// </summary>
public record AuditTraceRef(string CorrelationId);
