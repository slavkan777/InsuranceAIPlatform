using InsuranceAIPlatform.Api.Contracts.Common;
using InsuranceAIPlatform.Services.CustomersPolicies;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceAIPlatform.Api.Controllers;

/// <summary>
/// BFF endpoint surface for the synthetic-customer directory. Read-only.
/// All rows are <c>IsSynthetic=true</c>; the controller never returns
/// production / real-PII rows. Pagination + case-insensitive substring
/// search are supported via query params.
/// </summary>
[ApiController]
[Route("api/customers")]
[Tags("Customers")]
public sealed class CustomersController : ControllerBase
{
    private readonly ICustomersPoliciesService _service;

    public CustomersController(ICustomersPoliciesService service)
    {
        _service = service;
    }

    /// <summary>
    /// Returns a paginated list of synthetic customers. Optional <paramref name="search"/>
    /// matches FullName / Email / Id (case-insensitive substring).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(CustomerListResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<CustomerListResult>> List(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken ct = default)
    {
        var result = await _service.ListCustomersAsync(search, page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>Total count of synthetic customers (rows with IsSynthetic=true).</summary>
    [HttpGet("count")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> Count(CancellationToken ct)
    {
        var count = await _service.CountSyntheticCustomersAsync(ct);
        return Ok(new { count, syntheticOnly = true });
    }

    /// <summary>Looks up a single synthetic customer by id; 404 if not found.</summary>
    [HttpGet("{customerId}")]
    [ProducesResponseType(typeof(CustomerSummary), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerSummary>> GetById(string customerId, CancellationToken ct)
    {
        var row = await _service.GetCustomerByIdAsync(customerId, ct);
        if (row is null)
        {
            return NotFound(new ApiErrorResponse(
                Code: "CUSTOMER_NOT_FOUND",
                Message: $"Customer '{customerId}' was not found (synthetic directory).",
                TraceId: HttpContext.TraceIdentifier));
        }
        return Ok(row);
    }

    /// <summary>
    /// Creates a new synthetic customer in the local sandbox. ID is allocated server-side
    /// (next free CUST-T0XXX after the seed). Row is always <c>IsSynthetic=true</c>; this
    /// endpoint refuses to write real PII / non-synthetic markers.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateCustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateCustomerResponse>> Create(
        [FromBody] CreateCustomerRequest body,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(body.FullName))
        {
            return BadRequest(new ApiErrorResponse(
                Code: "MISSING_REQUIRED_FIELDS",
                Message: "fullName is required.",
                TraceId: HttpContext.TraceIdentifier));
        }
        if (body.FullName.Trim().Length > 200)
        {
            return BadRequest(new ApiErrorResponse(
                Code: "FIELD_TOO_LONG",
                Message: "fullName must be 200 characters or fewer.",
                TraceId: HttpContext.TraceIdentifier));
        }

        var input = new NewSyntheticCustomer(
            FullName: body.FullName,
            Email: body.Email,
            Phone: body.Phone,
            AddressLine: body.AddressLine,
            CustomerSince: body.CustomerSince);

        var created = await _service.CreateSyntheticCustomerAsync(input, ct);
        var commandId = $"cmd-{Guid.NewGuid():N}";

        return Ok(new CreateCustomerResponse(
            Success: true,
            CommandId: commandId,
            CustomerId: created.Id,
            FullName: created.FullName,
            Email: created.Email,
            Phone: created.Phone,
            AddressLine: created.AddressLine,
            CustomerSince: created.CustomerSince.ToString("yyyy-MM-dd"),
            IsSynthetic: created.IsSynthetic,
            Message: $"Synthetic customer {created.Id} created. No real PII, no real money."));
    }
}

/// <summary>Body for <c>POST /api/customers</c> (create a synthetic customer).</summary>
public sealed record CreateCustomerRequest(
    string FullName,
    string? Email,
    string? Phone,
    string? AddressLine,
    DateOnly? CustomerSince);

/// <summary>Returned by <c>POST /api/customers</c>.</summary>
public sealed record CreateCustomerResponse(
    bool Success,
    string CommandId,
    string CustomerId,
    string FullName,
    string Email,
    string Phone,
    string AddressLine,
    string CustomerSince,
    bool IsSynthetic,
    string Message);
