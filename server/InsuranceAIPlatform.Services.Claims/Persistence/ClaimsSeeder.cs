using InsuranceAIPlatform.BuildingBlocks;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAIPlatform.Services.Claims.Persistence;

/// <summary>
/// Idempotent deterministic seed for the Claims context.
/// Preserves CLM-1006 golden values EXACTLY as in InMemoryClaimReadService.
/// Also seeds CLM-1007..1010 and 10 additional synthetic claims for status variety.
/// </summary>
public static class ClaimsSeeder
{
    public static async Task SeedAsync(ClaimsDbContext db, CancellationToken ct = default)
    {
        if (await db.Claims.AnyAsync(ct))
            return;

        var claims = new List<Claim>
        {
            // ---------------------------------------------------------------
            // CLM-1006 golden claim (mirrors InMemoryClaimReadService exactly)
            // ---------------------------------------------------------------
            new()
            {
                ClaimId = "CLM-1006",
                CustomerId = "CUST-4421",
                PolicyId = "POL-2025-AC-4421",
                Customer = "Robert Johnson",
                Vehicle = "Toyota Camry 2021",
                VehicleVin = "VIN ****8842",
                Policy = "Auto Comprehensive",
                EventType = "RoadAccident",
                EventDate = new DateOnly(2026, 5, 18),
                Location = "Springfield, Main Street 24",
                Description = "Collision at an intersection while performing a right-turn manoeuvre.",
                Status = ClaimContractCodes.Status.InProgress,
                Risk = ClaimContractCodes.Risk.High,
                RiskScore = 82,
                Confidence = 78,
                SlaDeadline = new DateTimeOffset(2026, 5, 27, 18, 0, 0, TimeSpan.Zero),
                DocumentsReceived = 6,
                DocumentsTotal = 7,
                MissingDocument = "Rear bumper damage photo",
                Estimate = 2720.00m,
                ExpectedBenchmark = 1970.00m,
                Deductible = 500.00m,
                RecommendedPayout = 1800.00m,
                TraceId = "trc_8f3d2a7e",
                RunId = "run_8f3d2a7e",
                Tokens = 4261,
                Cost = 0.0187m,
                DurationSec = 18.9,
            },
            // ---------------------------------------------------------------
            // CLM-1007..1010 from SeedClaimList
            // ---------------------------------------------------------------
            new()
            {
                ClaimId = "CLM-1007",
                CustomerId = "CUST-T0007",
                PolicyId = "POL-2025-AC-T0007",
                Customer = "Maria Coval",
                Vehicle = "VW Golf 2019",
                VehicleVin = "SYNVIN0000000007",
                Policy = "Auto Third Party",
                EventType = "Parking",
                EventDate = new DateOnly(2026, 5, 26),
                Location = "Riverside, Oak Avenue 5",
                Description = "Damage sustained while parking.",
                Status = ClaimContractCodes.Status.CollectingDocuments,
                Risk = ClaimContractCodes.Risk.Medium,
                RiskScore = 45,
                Confidence = 62,
                SlaDeadline = new DateTimeOffset(2026, 6, 2, 18, 0, 0, TimeSpan.Zero),
                DocumentsReceived = 3,
                DocumentsTotal = 5,
                MissingDocument = "Repair invoice",
                Estimate = 850.00m,
                ExpectedBenchmark = 700.00m,
                Deductible = 200.00m,
                RecommendedPayout = 650.00m,
                TraceId = "trc_1007aaaa",
                RunId = "run_1007aaaa",
                Tokens = 0,
                Cost = 0m,
                DurationSec = 0,
            },
            new()
            {
                ClaimId = "CLM-1008",
                CustomerId = "CUST-T0008",
                PolicyId = "POL-2020-AC-T0008",
                Customer = "Ivan Petrenko",
                Vehicle = "Ford Focus 2020",
                VehicleVin = "SYNVIN0000000008",
                Policy = "Auto Comprehensive",
                EventType = "Collision",
                EventDate = new DateOnly(2026, 5, 26),
                Location = "Fairview, Park Avenue 12",
                Description = "Collision at a traffic light.",
                Status = ClaimContractCodes.Status.Ready,
                Risk = ClaimContractCodes.Risk.Low,
                RiskScore = 22,
                Confidence = 91,
                SlaDeadline = new DateTimeOffset(2026, 6, 5, 18, 0, 0, TimeSpan.Zero),
                DocumentsReceived = 4,
                DocumentsTotal = 4,
                MissingDocument = null,
                Estimate = 1200.00m,
                ExpectedBenchmark = 1150.00m,
                Deductible = 300.00m,
                RecommendedPayout = 900.00m,
                TraceId = "trc_1008bbbb",
                RunId = "run_1008bbbb",
                Tokens = 0,
                Cost = 0m,
                DurationSec = 0,
            },
            new()
            {
                ClaimId = "CLM-1009",
                CustomerId = "CUST-T0009",
                PolicyId = "POL-2018-AC-T0009",
                Customer = "Elena Shevchenko",
                Vehicle = "Renault Megane 2018",
                VehicleVin = "SYNVIN0000000009",
                Policy = "Auto Basic",
                EventType = "Damage",
                EventDate = new DateOnly(2026, 5, 25),
                Location = "Greenville, Maple Street 7",
                Description = "Bodywork damage.",
                Status = ClaimContractCodes.Status.CollectingDocuments,
                Risk = ClaimContractCodes.Risk.Medium,
                RiskScore = 51,
                Confidence = 55,
                SlaDeadline = new DateTimeOffset(2026, 6, 4, 18, 0, 0, TimeSpan.Zero),
                DocumentsReceived = 2,
                DocumentsTotal = 6,
                MissingDocument = "Police report",
                Estimate = 980.00m,
                ExpectedBenchmark = 800.00m,
                Deductible = 150.00m,
                RecommendedPayout = 700.00m,
                TraceId = "trc_1009cccc",
                RunId = "run_1009cccc",
                Tokens = 0,
                Cost = 0m,
                DurationSec = 0,
            },
            new()
            {
                ClaimId = "CLM-1010",
                CustomerId = "CUST-T0010",
                PolicyId = "POL-2022-AC-T0010",
                Customer = "David Wilson",
                Vehicle = "BMW X3 2022",
                VehicleVin = "SYNVIN0000000010",
                Policy = "Auto Premium",
                EventType = "RoadAccident",
                EventDate = new DateOnly(2026, 5, 27),
                Location = "Madison, Cedar Road 3",
                Description = "Road accident at an intersection.",
                Status = ClaimContractCodes.Status.AiProcessing,
                Risk = ClaimContractCodes.Risk.Low,
                RiskScore = 30,
                Confidence = 70,
                SlaDeadline = new DateTimeOffset(2026, 6, 3, 18, 0, 0, TimeSpan.Zero),
                DocumentsReceived = 5,
                DocumentsTotal = 5,
                MissingDocument = null,
                Estimate = 3500.00m,
                ExpectedBenchmark = 3200.00m,
                Deductible = 500.00m,
                RecommendedPayout = 3000.00m,
                TraceId = "trc_1010dddd",
                RunId = "run_1010dddd",
                Tokens = 0,
                Cost = 0m,
                DurationSec = 0,
            },
        };

        // Add 10 more synthetic claims for dashboard status variety
        var statuses = new[]
        {
            ClaimContractCodes.Status.InProgress,
            ClaimContractCodes.Status.CollectingDocuments,
            ClaimContractCodes.Status.Ready,
            ClaimContractCodes.Status.AiProcessing,
        };
        var risks = new[]
        {
            ClaimContractCodes.Risk.Low,
            ClaimContractCodes.Risk.Medium,
            ClaimContractCodes.Risk.High,
        };
        var eventTypes = new[] { "RoadAccident", "Parking", "Collision", "Damage", "Theft" };

        for (int i = 1; i <= 10; i++)
        {
            var claimNum = 1011 + i - 1;
            var custNum = (20 + i).ToString("D4");
            claims.Add(new Claim
            {
                ClaimId = $"CLM-{claimNum}",
                CustomerId = $"CUST-T0{custNum}",
                PolicyId = $"POL-2024-AC-T0{custNum}",
                Customer = $"Synthetic Customer {custNum}",
                Vehicle = $"Synthetic Vehicle {i}",
                VehicleVin = $"SYNVIN{(20 + i):D10}",
                Policy = "Auto Basic",
                EventType = eventTypes[i % eventTypes.Length],
                EventDate = new DateOnly(2026, 5, 10 + i),
                Location = $"Riverside, Test Street {i}",
                Description = $"Synthetic claim description {claimNum}.",
                Status = statuses[i % statuses.Length],
                Risk = risks[i % risks.Length],
                RiskScore = 20 + (i * 7),
                Confidence = 50 + (i * 4),
                SlaDeadline = new DateTimeOffset(2026, 6, 1 + i, 18, 0, 0, TimeSpan.Zero),
                DocumentsReceived = i % 5,
                DocumentsTotal = 5,
                MissingDocument = i % 3 == 0 ? "Repair invoice" : null,
                Estimate = 500m + (i * 150m),
                ExpectedBenchmark = 450m + (i * 130m),
                Deductible = 200m,
                RecommendedPayout = 400m + (i * 100m),
                TraceId = $"trc_{claimNum:X8}",
                RunId = $"run_{claimNum:X8}",
                Tokens = 0,
                Cost = 0m,
                DurationSec = 0,
            });
        }

        await db.Claims.AddRangeAsync(claims, ct);
        await db.SaveChangesAsync(ct);
    }
}
