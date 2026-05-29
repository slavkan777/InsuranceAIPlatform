using System.Text.Json;
using InsuranceAIPlatform.Api.Contracts.Common;
using InsuranceAIPlatform.Api.Middleware;
using InsuranceAIPlatform.BuildingBlocks;
using InsuranceAIPlatform.Services.AuditCost;
using InsuranceAIPlatform.Services.Claims;
using InsuranceAIPlatform.Services.CustomersPolicies;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceAIPlatform.Api.Controllers;

/// <summary>Body for POST /api/claims (create new synthetic claim).</summary>
public sealed record CreateClaimRequest(
    string? CustomerId,
    string? CustomerName,
    string? Policy,
    string? PolicyId,
    string Vehicle,
    string? VehicleVin,
    string EventType,
    DateOnly EventDate,
    string Location,
    string? Description);

/// <summary>Returned by POST /api/claims.</summary>
public sealed record CreateClaimResult(
    bool Success,
    string CommandId,
    string ClaimId,
    string Status,
    int? AuditEventId,
    int? OutboxMessageId,
    string CorrelationId,
    string Message,
    IReadOnlyList<string> Warnings,
    string CustomerId,
    string Customer,
    string Vehicle);

/// <summary>
/// BFF write endpoint for synthetic claim creation. Local/sandbox only — every
/// row goes through audit + outbox. No real PII, no real money, no external messaging.
/// </summary>
[ApiController]
[Route("api/claims")]
[Tags("Claim Write")]
public sealed class ClaimWriteController : ControllerBase
{
    private readonly IClaimsService _claimsService;
    private readonly ICustomersPoliciesService _customersService;
    private readonly IAuditCostService _audit;

    public ClaimWriteController(
        IClaimsService claimsService,
        ICustomersPoliciesService customersService,
        IAuditCostService audit)
    {
        _claimsService    = claimsService;
        _customersService = customersService;
        _audit            = audit;
    }

    private string GetCorrelationId() =>
        HttpContext.Items[CorrelationIdMiddleware.CorrelationIdKey]?.ToString()
        ?? HttpContext.TraceIdentifier;

    private static string? IdempotencyKey(HttpContext ctx) =>
        ctx.Request.Headers.TryGetValue("Idempotency-Key", out var v) ? v.ToString() : null;

    /// <summary>
    /// Creates a new synthetic claim row in the claims domain. The chosen customer
    /// (or a random synthetic one if not provided) is validated against the synthetic
    /// directory — only IsSynthetic=true customers are linkable.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateClaimResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreateClaimResult>> Create(
        [FromBody] CreateClaimRequest body,
        CancellationToken ct)
    {
        // Minimal field validation — everything is synthetic/sandbox-only.
        if (string.IsNullOrWhiteSpace(body.Vehicle) ||
            string.IsNullOrWhiteSpace(body.EventType) ||
            string.IsNullOrWhiteSpace(body.Location))
        {
            return BadRequest(new ApiErrorResponse(
                Code: "MISSING_REQUIRED_FIELDS",
                Message: "vehicle, eventType, and location are required.",
                TraceId: HttpContext.TraceIdentifier));
        }

        // Resolve customer — caller may pass an existing synthetic id OR a free-text
        // customer-name. Real PII never enters here: we always re-link to a synthetic row.
        CustomerSummary? customer = null;
        if (!string.IsNullOrWhiteSpace(body.CustomerId))
        {
            customer = await _customersService.GetCustomerByIdAsync(body.CustomerId!, ct);
            if (customer is null)
            {
                return NotFound(new ApiErrorResponse(
                    Code: "CUSTOMER_NOT_FOUND",
                    Message: $"Synthetic customer '{body.CustomerId}' was not found.",
                    TraceId: HttpContext.TraceIdentifier));
            }
        }
        else
        {
            // Pick the first synthetic customer alphabetically — deterministic for testing.
            var list = await _customersService.ListCustomersAsync(null, 1, 1, ct);
            customer = list.Items.FirstOrDefault();
            if (customer is null)
            {
                return BadRequest(new ApiErrorResponse(
                    Code: "NO_SYNTHETIC_CUSTOMERS",
                    Message: "No synthetic customers available — seed the customers_policies directory first.",
                    TraceId: HttpContext.TraceIdentifier));
            }
        }

        var seed = new NewSyntheticClaim(
            CustomerId:   customer.Id,
            CustomerName: string.IsNullOrWhiteSpace(body.CustomerName) ? customer.FullName : body.CustomerName!,
            Policy:       body.Policy ?? "Auto Comprehensive",
            PolicyId:     body.PolicyId ?? $"POL-LOCAL-{customer.Id}",
            Vehicle:      body.Vehicle,
            VehicleVin:   body.VehicleVin ?? "VIN ****0000",
            EventType:    body.EventType,
            EventDate:    body.EventDate,
            Location:     body.Location,
            Description:  body.Description);

        var correlationId = GetCorrelationId();
        var actor         = CommandActors.SyntheticAdjuster();
        var idempKey      = IdempotencyKey(HttpContext);
        var commandId     = $"cmd-{Guid.NewGuid():N}";
        var warnings      = new List<string>();

        var newClaimId = await _claimsService.CreateClaimAsync(seed, actor, ct);

        var meta = JsonSerializer.Serialize(new
        {
            ClaimId      = newClaimId,
            CustomerId   = customer.Id,
            EventType    = seed.EventType,
            EventDate    = seed.EventDate.ToString("yyyy-MM-dd"),
            Sandbox      = true,
            Source       = "Human",
        });

        var auditId = await _audit.AppendAuditAsync(
            newClaimId, "ClaimCreated", actor, correlationId,
            "OK", $"Synthetic claim {newClaimId} created in local sandbox.",
            meta, ct);

        var payload = JsonSerializer.Serialize(new
        {
            ClaimId      = newClaimId,
            CustomerId   = customer.Id,
            EventType    = seed.EventType,
            EventDate    = seed.EventDate.ToString("yyyy-MM-dd"),
            ActionType   = "ClaimCreated",
            CommandId    = commandId,
            Sandbox      = true,
        });
        var (outboxId, outboxWarning) = await _audit.WriteOutboxAsync(
            "ClaimCreated", newClaimId, correlationId, payload, idempKey, ct);
        if (outboxWarning is not null) warnings.Add(outboxWarning);

        return Ok(new CreateClaimResult(
            Success:         true,
            CommandId:       commandId,
            ClaimId:         newClaimId,
            Status:          "Новий",
            AuditEventId:    auditId < 0 ? null : auditId,
            OutboxMessageId: outboxId < 0 ? null : outboxId,
            CorrelationId:   correlationId,
            Message:         $"Synthetic claim {newClaimId} created. Customer {customer.Id}. No real PII, no real money.",
            Warnings:        warnings,
            CustomerId:      customer.Id,
            Customer:        seed.CustomerName,
            Vehicle:         seed.Vehicle));
    }
}
