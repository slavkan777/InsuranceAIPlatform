using InsuranceAIPlatform.BuildingBlocks;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAIPlatform.Services.CustomersPolicies.Persistence;

/// <summary>
/// Idempotent deterministic seed for the Customers &amp; Policies context.
/// Produces exactly 200 SyntheticCustomer rows + 200 Policy + 200 Vehicle rows.
/// Also includes the golden CLM-1006 customer CUST-4421 (non-synthetic marker customer).
/// </summary>
public static class CustomersPoliciesSeeder
{
    private static readonly string[] Makes = ["Toyota", "Honda", "Ford", "Chevrolet", "BMW", "VW", "Renault", "Hyundai", "Kia", "Nissan"];
    private static readonly string[] Models = ["Camry", "Civic", "Focus", "Malibu", "X3", "Golf", "Megane", "Elantra", "Sportage", "Sentra"];
    private static readonly string[] Colors = ["Білий", "Чорний", "Срібний", "Синій", "Червоний", "Сірий", "Зелений", "Бежевий"];
    private static readonly string[] Cities = ["Київ", "Харків", "Одеса", "Дніпро", "Запоріжжя", "Львів", "Кривий Ріг", "Миколаїв"];
    private static readonly string[] Streets = ["вул. Шевченка", "вул. Лесі Українки", "вул. Хрещатик", "вул. Грушевського", "вул. Франка", "пр. Перемоги"];
    private static readonly string[] Products = ["Auto Comprehensive", "Auto Third Party", "Auto Basic", "Auto Premium"];

    public static async Task SeedAsync(CustomersPoliciesDbContext db, CancellationToken ct = default)
    {
        // Seed golden CLM-1006 customer CUST-4421 first (idempotent)
        await SeedGoldenCustomerAsync(db, ct);

        // Seed 200 synthetic customers (idempotent — skip if already present)
        var existingCount = await db.SyntheticCustomers
            .Where(c => c.Id.StartsWith("CUST-T"))
            .CountAsync(ct);

        if (existingCount >= SeedConstants.SyntheticUserCount)
            return;

        var customers = new List<SyntheticCustomer>(SeedConstants.SyntheticUserCount);
        var policies = new List<Policy>(SeedConstants.SyntheticUserCount);
        var vehicles = new List<Vehicle>(SeedConstants.SyntheticUserCount);

        for (int i = 1; i <= SeedConstants.SyntheticUserCount; i++)
        {
            var idx = i - 1;
            var num = i.ToString("D3");
            var customerId = $"CUST-T{num.PadLeft(4, '0')}";
            var make = Makes[idx % Makes.Length];
            var model = Models[idx % Models.Length];
            var color = Colors[idx % Colors.Length];
            var city = Cities[idx % Cities.Length];
            var street = Streets[idx % Streets.Length];
            var product = Products[idx % Products.Length];
            var year = 2018 + (idx % 7);
            var sinceYear = 2018 + (idx % 6);
            var month = 1 + (idx % 12);

            customers.Add(new SyntheticCustomer
            {
                Id = customerId,
                FullName = $"Synthetic Customer {num}",
                Email = $"testuser{num}@{SeedConstants.SyntheticEmailDomain}",
                Phone = $"+38050{(1000000 + i):D7}",
                AddressLine = $"{city}, {street} {(i % 99) + 1}",
                CustomerSince = new DateOnly(sinceYear, month, 1),
                PreviousClaimsCount = idx % 4,
                IsSynthetic = true
            });

            policies.Add(new Policy
            {
                PolicyId = $"POL-{year}-AC-T{num.PadLeft(4, '0')}",
                CustomerId = customerId,
                ProductName = product,
                StartDate = new DateOnly(year, 1, 1),
                EndDate = new DateOnly(year + 1, 12, 31),
                Premium = 800m + (idx % 50) * 12m,
                IsActive = true
            });

            vehicles.Add(new Vehicle
            {
                Id = $"VEH-T{num.PadLeft(4, '0')}",
                CustomerId = customerId,
                Make = make,
                Model = model,
                Year = 2015 + (idx % 10),
                Vin = $"SYNVIN{i:D10}",
                Color = color,
                Mileage = 10000 + (idx * 300)
            });
        }

        await db.SyntheticCustomers.AddRangeAsync(customers, ct);
        await db.Policies.AddRangeAsync(policies, ct);
        await db.Vehicles.AddRangeAsync(vehicles, ct);
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedGoldenCustomerAsync(CustomersPoliciesDbContext db, CancellationToken ct)
    {
        const string goldenCustomerId = "CUST-4421";

        if (await db.SyntheticCustomers.AnyAsync(c => c.Id == goldenCustomerId, ct))
            return;

        var customer = new SyntheticCustomer
        {
            Id = goldenCustomerId,
            FullName = "Роберт Джонсон",
            Email = $"robert.johnson@{SeedConstants.SyntheticEmailDomain}",
            Phone = "+380501234421",
            AddressLine = "Бориспіль, вул. Київська 24",
            CustomerSince = new DateOnly(2021, 3, 15),
            PreviousClaimsCount = 2,
            IsSynthetic = false
        };

        var policy = new Policy
        {
            PolicyId = "POL-2025-AC-4421",
            CustomerId = goldenCustomerId,
            ProductName = "Auto Comprehensive",
            StartDate = new DateOnly(2025, 1, 1),
            EndDate = new DateOnly(2025, 12, 31),
            Premium = 1200m,
            IsActive = true
        };

        var vehicle = new Vehicle
        {
            Id = "VEH-4421",
            CustomerId = goldenCustomerId,
            Make = "Toyota",
            Model = "Camry",
            Year = 2021,
            Vin = "VIN****8842",
            Color = "Срібний",
            Mileage = 42300
        };

        await db.SyntheticCustomers.AddAsync(customer, ct);
        await db.Policies.AddAsync(policy, ct);
        await db.Vehicles.AddAsync(vehicle, ct);
        await db.SaveChangesAsync(ct);
    }
}
