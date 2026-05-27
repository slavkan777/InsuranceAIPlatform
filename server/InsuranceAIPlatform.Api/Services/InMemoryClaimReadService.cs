using InsuranceAIPlatform.Api.Contracts.Claims;

namespace InsuranceAIPlatform.Api.Services;

/// <summary>
/// Singleton in-memory claim service. Seed data is deterministic, synthetic, and mirrors
/// the frontend mock at src/data/mock/claims.ts and src/data/mock/claim-1006.ts.
/// No EF Core, no DB, no external providers.
/// All AI outputs are advisory — human approval is always final.
/// </summary>
public sealed class InMemoryClaimReadService : IClaimReadService
{
    // -----------------------------------------------------------------------
    // Seed — CLM-1006 golden claim (mirrors frontend mock exactly)
    // -----------------------------------------------------------------------

    private static readonly ClaimDetailsDto Clm1006Details = new(
        Id: "CLM-1006",
        Customer: "Роберт Джонсон",
        CustomerId: "CUST-4421",
        Vehicle: "Toyota Camry 2021",
        VehicleVin: "VIN ****8842",
        Policy: "Auto Comprehensive",
        PolicyId: "POL-2025-AC-4421",
        EventType: "ДТП",
        EventDate: new DateOnly(2026, 5, 18),
        Location: "Бориспіль, вул. Київська 24",
        Description: "Зіткнення на перехресті при здійсненні маневру повороту праворуч.",
        Status: "В роботі",
        Risk: "Високий",
        RiskScore: 82,
        Confidence: 78,
        SlaDeadline: new DateTimeOffset(2026, 5, 27, 18, 0, 0, TimeSpan.Zero),
        DocumentsReceived: 6,
        DocumentsTotal: 7,
        MissingDocument: "Фото пошкодження заднього бампера",
        Estimate: 2720.00m,
        ExpectedBenchmark: 1970.00m,
        Deductible: 500.00m,
        RecommendedPayout: 1800.00m,
        TraceId: "trc_8f3d2a7e",
        RunId: "run_8f3d2a7e",
        Tokens: 4261,
        Cost: 0.0187m,
        DurationSec: 18.9);

    // -----------------------------------------------------------------------
    // Seed — claims list (mirrors src/data/mock/claims.ts claimRows)
    // -----------------------------------------------------------------------

    private static readonly IReadOnlyList<ClaimListItemDto> SeedClaimList = new List<ClaimListItemDto>
    {
        new("CLM-1006", "Роберт Джонсон", "Toyota Camry 2021", "ДТП",
            "В роботі", "6/7", "AI-перевірено", "Високий", "4 год",
            "Запросити фото", new DateTimeOffset(2026, 5, 18, 10, 22, 0, TimeSpan.Zero)),
        new("CLM-1007", "Марія Коваль", "VW Golf 2019", "Паркування",
            "Збір документів", "3/5", "Потрібна перевірка", "Середній", "6 год",
            "Запитати рахунок СТО", new DateTimeOffset(2026, 5, 26, 8, 15, 0, TimeSpan.Zero)),
        new("CLM-1008", "Іван Петренко", "Ford Focus 2020", "Зіткнення",
            "Готова", "4/4", "AI-перевірено", "Низький", "2 год",
            "Погодити", new DateTimeOffset(2026, 5, 26, 12, 0, 0, TimeSpan.Zero)),
        new("CLM-1009", "Олена Шевченко", "Renault Megane 2018", "Пошкодження",
            "Збір документів", "2/6", "Очікує документи", "Середній", "1 день",
            "Запросити документи", new DateTimeOffset(2026, 5, 25, 9, 0, 0, TimeSpan.Zero)),
        new("CLM-1010", "David Wilson", "BMW X3 2022", "ДТП",
            "AI-обробка", "5/5", "Обробляється", "Низький", "3 год",
            "Очікувати AI", new DateTimeOffset(2026, 5, 27, 7, 25, 0, TimeSpan.Zero)),
    };

    // -----------------------------------------------------------------------
    // Seed — documents for CLM-1006 (mirrors documentsChecklist + damagePhotos)
    // -----------------------------------------------------------------------

    private static readonly IReadOnlyList<ClaimDocumentDto> Clm1006Documents = new List<ClaimDocumentDto>
    {
        new("application",   "Заява клієнта",           "19.05.2026",        "ok",      "document", null),
        new("police",        "Поліцейський звіт",       "NoБРС-2026/05/441", "ok",      "document", null),
        new("photo-front",   "Фото — переднє",          "AI conf 92%",       "ok",      "photo",    92),
        new("photo-side",    "Фото — бокове",           "AI conf 87%",       "ok",      "photo",    87),
        new("invoice",       "Рахунок СТО",             "Сума +38%",         "warn",    "document", null),
        new("policy-terms",  "Умови полісу",            "Auto Comprehensive","ok",      "document", null),
        new("photo-rear",    "Фото — задній бампер",    "ВІДСУТНЄ",          "missing", "photo",    null),
    };

    // -----------------------------------------------------------------------
    // Seed — AI evidence for CLM-1006
    // -----------------------------------------------------------------------

    private static readonly AiEvidenceDto Clm1006AiEvidence = new(
        RunId: "run_8f3d2a7e",
        ModelConfidence: 78,
        Findings: new AiFindingDto[]
        {
            new("f1", "Документи",     "Відсутнє фото заднього бампера. 6 з 7 документів надано.", "warn"),
            new("f2", "Оцінка збитку", "Оцінка $2720 перевищує бенчмарк $1970 на 38%.",            "warn"),
            new("f3", "Покриття",      "Подія ДТП підпадає під Auto Comprehensive. Франшиза $500 застосовна.", "ok"),
        },
        Evidence: new EvidenceSourceDto[]
        {
            new("e1", "Поліцейський звіт", "Підтверджено факт ДТП 18.05.2026, Бориспіль.",                          95),
            new("e2", "Рахунок СТО",       "Загальна сума $2720. Деталізація: бампер $980, лак $740, кузов $1000.", 87),
        },
        ExtractedEntities: new ExtractedEntityDto[]
        {
            new("Дата ДТП",    "18.05.2026",        "Поліцейський звіт", 99),
            new("Авто",        "Toyota Camry 2021", "Поліс",             98),
            new("Сума",        "$2 720",            "Рахунок СТО",       94),
            new("Поліс",       "POL-2025-AC-4421",  "Поліс",             100),
            new("Заявник",     "Роберт Джонсон",    "Заява",             100),
            new("Локація",     "Бориспіль, вул. Київська 24", "Звіт",   95),
        },
        ModelConfidenceBreakdown: new ConfidenceBreakdownItemDto[]
        {
            new("Витягування",   95),
            new("Покриття",      92),
            new("Пошкодження",   71),
            new("Рекомендація",  78),
        });

    // -----------------------------------------------------------------------
    // Seed — risk assessment for CLM-1006
    // -----------------------------------------------------------------------

    private static readonly RiskAssessmentDto Clm1006Risks = new(
        Score: 82,
        Threshold: 60,
        Level: "Високий",
        Factors: new RiskFactorDto[]
        {
            new("amount",        "Сума ремонту вище очікуваного діапазону",    25),
            new("mismatch",      "Розбіжності у поясненнях водіїв",            18),
            new("missing-photo", "Відсутнє фото пошкодження",                  22),
            new("prior",         "Попередні claims клієнта",                    8),
            new("confidence",    "Confidence нижче порогу 85%",                 9),
        },
        Pipeline: new PipelineStageDto[]
        {
            new("Класифікатор документів", "OK"),
            new("Вилучення полів",         "OK"),
            new("Рушій ризиків",           "WARN"),
            new("Рекомендатор",            "OK"),
            new("Управління",              "BLOCK"),
        });

    // -----------------------------------------------------------------------
    // Seed — policy for CLM-1006
    // -----------------------------------------------------------------------

    private static readonly PolicyDto Clm1006Policy = new(
        PolicyId: "POL-2025-AC-4421",
        ProductName: "Auto Comprehensive",
        CoverageBlocks: new PolicyCoverageDto[]
        {
            new("cov-collision",  "Зіткнення",        "$50 000",  "$500",    true,  null),
            new("cov-liability",  "Відповідальність", "$100 000", "$0",      false, null),
            new("cov-glass",      "Скло",             "$1 500",   "$100",    false, null),
            new("cov-theft",      "Викрадення",       "Ринкова",  "$1 000",  false, null),
            new("cov-roadside",   "Дорожня допомога", "24/7",     "$0",      false, null),
        },
        Validation: new PolicyCheckResultDto(
            Covered: true,
            CoverageType: "Collision",
            ValidationNotes: new[]
            {
                "Покриття підтверджено",
                "ДТП дата у межах періоду",
                "Lapse не виявлено",
                "Зіткнення входить у покриття",
                "Франшиза $500 застосовується",
                "Виключень не виявлено",
            },
            ExclusionTriggered: false));

    // -----------------------------------------------------------------------
    // Seed — customer & vehicle for CLM-1006
    // -----------------------------------------------------------------------

    private static readonly CustomerVehicleContextDto Clm1006CustomerVehicle = new(
        Customer: new CustomerDto(
            CustomerId: "CUST-4421",
            FullName: "Роберт Джонсон",
            PreviousClaimsCount: 2,
            CustomerSince: new DateOnly(2021, 3, 15),
            CommunicationHistory: new CommunicationEntryDto[]
            {
                new(new DateOnly(2026, 5, 19), "Email",    "Запит фото заднього бампера"),
                new(new DateOnly(2026, 5, 19), "Чат",      "Рахунок СТО надано"),
                new(new DateOnly(2026, 5, 18), "Телефон",  "Перевірка по полісу"),
                new(new DateOnly(2026, 5, 18), "Web",      "Заявка про ДТП"),
            }),
        Vehicle: new VehicleDto(
            Make: "Toyota",
            Model: "Camry",
            Year: 2021,
            Vin: "VIN ****8842",
            Color: "Срібний",
            Mileage: 42300));

    // -----------------------------------------------------------------------
    // Seed — approval draft for CLM-1006
    // -----------------------------------------------------------------------

    private static readonly ApprovalDraftDto Clm1006Approval = new(
        ClaimId: "CLM-1006",
        CurrentDecision: null,
        Notes: null,
        SavedAt: null,
        Submitted: false,
        SubmittedAt: null,
        AvailableOptions: new HumanDecisionOptionDto[]
        {
            new("request",  "Запросити додаткові документи", true,  "Рекомендовано AI — запросити фото заднього бампера"),
            new("approve",  "Затвердити виплату",           false, "Якщо ризики прийнятні після перевірки"),
            new("reject",   "Відхилити заявку",             false, "З обґрунтуванням відмови"),
            new("escalate", "Передати до відділу розслідування", false, "Ескалація для детального розслідування"),
        },
        AiRecommendation: "Запросити додаткові документи",
        RecommendedPayout: 1800.00m);

    // -----------------------------------------------------------------------
    // Seed — audit trace for CLM-1006 (mirrors auditTrail + costDistribution)
    // -----------------------------------------------------------------------

    private static readonly AuditTraceDto Clm1006Audit = new(
        RunId: "run_8f3d2a7e",
        TraceId: "trc_8f3d2a7e",
        Model: "Azure OpenAI (mock)",
        Tokens: 4261,
        Cost: 0.0187m,
        DurationSec: 18.9,
        Events: new AuditEventDto[]
        {
            new("14:05:12", "AI Pipeline",    "Запуск аналізу CLM-1006",    "OK"),
            new("14:05:14", "Doc Classifier", "Класифікація 6 документів",  "OK"),
            new("14:05:19", "Field Extractor","Витягнуто 47 полів",          "OK"),
            new("14:05:25", "Risk Engine",    "Ризик 82/100 — Високий",     "WARN"),
            new("14:05:30", "Recommender",    "Рекомендація: запросити фото","OK"),
            new("14:05:31", "Governance",     "Авто-погодження заблоковано", "BLOCK"),
        },
        CostDistribution: new CostDistributionItemDto[]
        {
            new("Витягування",  0.0072m),
            new("RAG / докази", 0.0058m),
            new("Ризик",        0.0029m),
            new("Рекомендація", 0.0028m),
        });

    // -----------------------------------------------------------------------
    // Seed — demo scenario (mirrors demoSteps from claim-1006.ts)
    // -----------------------------------------------------------------------

    private static readonly DemoScenarioDto SeedDemoScenario = new(
        Steps: new DemoStepDto[]
        {
            new(1, "Огляд",          "Стан черги ДТП",          "Дошка 01", "/"),
            new(2, "Обрати CLM-1006","Toyota Camry",            "Дошка 03", "/claims/CLM-1006"),
            new(3, "Документи та фото","6/7 + відсутнє",        "Дошка 04", "/claims/CLM-1006/documents"),
            new(4, "AI-докази",      "4 знахідки + RAG",        "Дошка 05", "/claims/CLM-1006/ai-evidence"),
            new(5, "Оцінка ризиків", "82/100 Високий",          "Дошка 06", "/claims/CLM-1006/risks"),
            new(6, "Людське рішення","Експерт обирає",          "Дошка 07", "/claims/CLM-1006/approval"),
            new(7, "Audit & Cost",   "Trace + governance",      "Дошка 08", "/claims/CLM-1006/audit"),
        },
        GoldenClaimId: "CLM-1006");

    // -----------------------------------------------------------------------
    // Index for O(1) lookup by claimId
    // -----------------------------------------------------------------------

    private static readonly Dictionary<string, ClaimDetailsDto> ClaimIndex =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["CLM-1006"] = Clm1006Details,
        };

    // -----------------------------------------------------------------------
    // IClaimReadService implementation
    // -----------------------------------------------------------------------

    public ClaimSummaryDto GetSummary() => new(
        TotalActive: 47,
        PendingReview: 12,
        HighRisk: 8,
        AvgSlaRemainingHours: 14.3,
        ProcessedToday: 6,
        AiAnalysisRunning: 2);

    public IReadOnlyList<ClaimListItemDto> GetClaims() => SeedClaimList;

    public ClaimDetailsDto? GetClaim(string claimId) =>
        ClaimIndex.TryGetValue(claimId, out var claim) ? claim : null;

    public IReadOnlyList<ClaimDocumentDto>? GetDocuments(string claimId) =>
        claimId.Equals("CLM-1006", StringComparison.OrdinalIgnoreCase)
            ? Clm1006Documents
            : null;

    public AiEvidenceDto? GetAiEvidence(string claimId) =>
        claimId.Equals("CLM-1006", StringComparison.OrdinalIgnoreCase)
            ? Clm1006AiEvidence
            : null;

    public RiskAssessmentDto? GetRisks(string claimId) =>
        claimId.Equals("CLM-1006", StringComparison.OrdinalIgnoreCase)
            ? Clm1006Risks
            : null;

    public PolicyDto? GetPolicy(string claimId) =>
        claimId.Equals("CLM-1006", StringComparison.OrdinalIgnoreCase)
            ? Clm1006Policy
            : null;

    public CustomerVehicleContextDto? GetCustomerVehicle(string claimId) =>
        claimId.Equals("CLM-1006", StringComparison.OrdinalIgnoreCase)
            ? Clm1006CustomerVehicle
            : null;

    public ApprovalDraftDto? GetApproval(string claimId) =>
        claimId.Equals("CLM-1006", StringComparison.OrdinalIgnoreCase)
            ? Clm1006Approval
            : null;

    public AuditTraceDto? GetAudit(string claimId) =>
        claimId.Equals("CLM-1006", StringComparison.OrdinalIgnoreCase)
            ? Clm1006Audit
            : null;

    public DemoScenarioDto GetDemoScenario() => SeedDemoScenario;
}
