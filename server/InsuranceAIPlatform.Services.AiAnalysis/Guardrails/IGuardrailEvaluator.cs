using InsuranceAIPlatform.Services.AiAnalysis.Contracts;

namespace InsuranceAIPlatform.Services.AiAnalysis.Guardrails;

/// <summary>
/// Evaluates raw AI provider output for unsafe authority language.
/// Implementation must be deterministic, synchronous, and never call external services.
/// </summary>
public interface IGuardrailEvaluator
{
    GuardrailAssessment Evaluate(AiProviderRawOutput raw);
}
