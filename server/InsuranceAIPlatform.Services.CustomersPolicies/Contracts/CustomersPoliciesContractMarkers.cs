namespace InsuranceAIPlatform.Services.CustomersPolicies.Contracts;

/// <summary>
/// Forward-declaration markers for the Customers &amp; Policies future read contracts.
/// Id-only placeholders — no business fields, no PII — to be formalised in a later gate.
/// </summary>
public record PolicyRef(string PolicyId);

/// <inheritdoc cref="PolicyRef"/>
public record CustomerRef(string CustomerId);
