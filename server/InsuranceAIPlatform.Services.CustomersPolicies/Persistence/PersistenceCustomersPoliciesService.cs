using System.Text.RegularExpressions;
using InsuranceAIPlatform.BuildingBlocks;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAIPlatform.Services.CustomersPolicies.Persistence;

/// <summary>
/// DB-backed read-only implementation of <see cref="ICustomersPoliciesService"/>
/// (singleton-safe via IDbContextFactory). All rows are synthetic / IsSynthetic=true;
/// the service never returns rows with IsSynthetic=false to UI/API consumers.
/// </summary>
public sealed class PersistenceCustomersPoliciesService : ICustomersPoliciesService
{
    private readonly IDbContextFactory<CustomersPoliciesDbContext> _factory;

    public PersistenceCustomersPoliciesService(IDbContextFactory<CustomersPoliciesDbContext> factory)
    {
        _factory = factory;
    }

    public string ServiceName => ServiceNames.CustomersPolicies;

    public ServiceHealthSnapshot GetHealth() => new(
        ServiceNames.CustomersPolicies,
        ServiceReadinessStatus.Ready,
        "persistence-v0.1",
        new[] { "customers", "vehicles", "policies", "list-search", "by-id" });

    public async Task<int> CountSyntheticCustomersAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        return await db.SyntheticCustomers.CountAsync(c => c.IsSynthetic, cancellationToken);
    }

    public async Task<CustomerListResult> ListCustomersAsync(
        string? search, int page, int pageSize, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 25;
        if (pageSize > 200) pageSize = 200;

        await using var db = await _factory.CreateDbContextAsync(ct);
        var query = db.SyntheticCustomers.AsNoTracking().Where(c => c.IsSynthetic);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(c =>
                EF.Functions.Like(c.FullName, $"%{s}%") ||
                EF.Functions.Like(c.Email, $"%{s}%") ||
                EF.Functions.Like(c.Id, $"%{s}%"));
        }

        var total = await query.CountAsync(ct);
        var rows = await query
            .OrderBy(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CustomerSummary(
                c.Id, c.FullName, c.Email, c.Phone, c.AddressLine,
                c.CustomerSince, c.PreviousClaimsCount, c.IsSynthetic))
            .ToListAsync(ct);

        return new CustomerListResult(total, page, pageSize, rows);
    }

    public async Task<CustomerSummary?> GetCustomerByIdAsync(string customerId, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var row = await db.SyntheticCustomers.AsNoTracking()
            .Where(c => c.IsSynthetic && c.Id == customerId)
            .Select(c => new CustomerSummary(
                c.Id, c.FullName, c.Email, c.Phone, c.AddressLine,
                c.CustomerSince, c.PreviousClaimsCount, c.IsSynthetic))
            .FirstOrDefaultAsync(ct);
        return row;
    }

    private static readonly Regex CustomerIdNumberPattern =
        new(@"^CUST-T(\d{4,})$", RegexOptions.Compiled);

    public async Task<CustomerSummary> CreateSyntheticCustomerAsync(
        NewSyntheticCustomer input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.FullName))
            throw new ArgumentException("FullName is required.", nameof(input));

        await using var db = await _factory.CreateDbContextAsync(ct);

        // Allocate next ID: scan all CUST-T#### rows, take max numeric tail, +1.
        // We default to 201 (one past seed end) so new IDs never collide with seed.
        var existingIds = await db.SyntheticCustomers
            .AsNoTracking()
            .Where(c => c.Id.StartsWith("CUST-T"))
            .Select(c => c.Id)
            .ToListAsync(ct);

        var maxNum = 200; // seed allocates CUST-T0001..CUST-T0200
        foreach (var id in existingIds)
        {
            var m = CustomerIdNumberPattern.Match(id);
            if (m.Success && int.TryParse(m.Groups[1].Value, out var n) && n > maxNum)
                maxNum = n;
        }
        var nextNum = maxNum + 1;
        var newId = $"CUST-T{nextNum:D4}";

        var fullName = input.FullName.Trim();
        var email = (input.Email ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(email))
            email = $"created-{nextNum:D4}@{SeedConstants.SyntheticEmailDomain}";

        var phone = (input.Phone ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(phone))
            phone = $"+38050{nextNum:D7}";

        var addressLine = (input.AddressLine ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(addressLine))
            addressLine = "Local sandbox · user-created · no real address";

        var customerSince = input.CustomerSince ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var row = new SyntheticCustomer
        {
            Id = newId,
            FullName = fullName,
            Email = email,
            Phone = phone,
            AddressLine = addressLine,
            CustomerSince = customerSince,
            PreviousClaimsCount = 0,
            IsSynthetic = true,
        };

        db.SyntheticCustomers.Add(row);
        await db.SaveChangesAsync(ct);

        return new CustomerSummary(
            row.Id, row.FullName, row.Email, row.Phone, row.AddressLine,
            row.CustomerSince, row.PreviousClaimsCount, row.IsSynthetic);
    }
}
