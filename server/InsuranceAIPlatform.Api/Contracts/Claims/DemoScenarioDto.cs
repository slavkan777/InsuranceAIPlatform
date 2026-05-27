namespace InsuranceAIPlatform.Api.Contracts.Claims;

/// <summary>Structured demo walkthrough steps for the guided tour.</summary>
public record DemoScenarioDto(
    DemoStepDto[] Steps,
    string GoldenClaimId);

/// <summary>One demo tour step with navigation target.</summary>
public record DemoStepDto(
    int Step,
    string Title,
    string Caption,
    string? PdfRef,
    string Route);
