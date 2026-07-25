using System.Security.Cryptography;
using System.Text;
using InsuranceAIPlatform.Services.AiAnalysis.Persistence;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Embedding;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAIPlatform.Services.AiAnalysis.Rag.Persistence;

/// <summary>
/// Idempotent golden RAG seed: synthetic policy clauses, an evidence chunk corpus for the deep
/// golden claims (CLM-1006 through CLM-1011), and a gold evaluation question set.
///
/// Design: ADDITIVE / per-key idempotency. On re-run it loads existing ids and only inserts
/// rows whose key is not yet present — it never deletes or updates existing data. This means
/// the seeder can be run safely on an already-seeded DB (e.g. after an incremental expansion)
/// without losing any existing rows. All data is synthetic — no PII.
/// </summary>
public static class RagSeeder
{
    public const string ProductAutoComprehensive = "AUTO-COMPREHENSIVE";
    public const string ProductAutoThirdParty = "AUTO-THIRD-PARTY";

    public static async Task SeedAsync(AiAnalysisDbContext db, IEmbeddingProvider embed, CancellationToken ct = default)
    {
        // ── Per-key top-up (additive idempotency) ───────────────────────────────────────────
        var existingClauseIds = (await db.PolicyClauses.Select(c => c.ClauseId).ToListAsync(ct)).ToHashSet();
        var existingChunkIds  = (await db.EvidenceChunks.Select(c => c.ChunkId).ToListAsync(ct)).ToHashSet();
        var existingQIds      = (await db.RagEvaluationQuestions.Select(q => q.QuestionId).ToListAsync(ct)).ToHashSet();

        // ---- Policy clauses (Auto Comprehensive product) ----
        var allClauses = new List<PolicyClause>
        {
            new() { ClauseId = "CLA-AC-COVER-001", ProductCode = ProductAutoComprehensive, PolicyId = "POL-2025-AC-4421", ClauseType = "coverage",   Ordinal = 1, Title = "Road accident coverage", Text = "The Auto Comprehensive policy covers losses resulting from a road traffic accident and collision of vehicles." },
            new() { ClauseId = "CLA-AC-COVER-002", ProductCode = ProductAutoComprehensive, PolicyId = "POL-2025-AC-4421", ClauseType = "coverage",   Ordinal = 2, Title = "Natural events and theft", Text = "Damage from natural events, fire and theft of the vehicle is covered." },
            new() { ClauseId = "CLA-AC-EXCL-001",  ProductCode = ProductAutoComprehensive, PolicyId = "POL-2025-AC-4421", ClauseType = "exclusion",  Ordinal = 3, Title = "Exclusion: intoxication", Text = "Losses caused while driving under the influence of alcohol or drugs are not covered." },
            new() { ClauseId = "CLA-AC-EXCL-002",  ProductCode = ProductAutoComprehensive, PolicyId = "POL-2025-AC-4421", ClauseType = "exclusion",  Ordinal = 4, Title = "Exclusion: racing", Text = "Losses incurred while taking part in racing or driving off public roads are not covered." },
            new() { ClauseId = "CLA-AC-DED-001",   ProductCode = ProductAutoComprehensive, PolicyId = "POL-2025-AC-4421", ClauseType = "deductible", Ordinal = 5, Title = "Deductible", Text = "A deductible of 500 US dollars applies to every insured event." },
            new() { ClauseId = "CLA-AC-LIM-001",   ProductCode = ProductAutoComprehensive, PolicyId = "POL-2025-AC-4421", ClauseType = "limit",      Ordinal = 6, Title = "Bodywork repair limit", Text = "The bodywork repair limit is determined by the market value of comparable work in the region." },
            // Auto Third-Party product (two extra clauses)
            new() { ClauseId = "CLA-TP-COVER-001", ProductCode = ProductAutoThirdParty,    PolicyId = "POL-2025-TP-0091", ClauseType = "coverage",   Ordinal = 1, Title = "Third-party liability coverage", Text = "The Auto Third-Party policy covers losses caused to third parties in a road traffic accident for which the policyholder is at fault." },
            new() { ClauseId = "CLA-TP-LIM-001",   ProductCode = ProductAutoThirdParty,    PolicyId = "POL-2025-TP-0091", ClauseType = "limit",      Ordinal = 2, Title = "Third-party liability limit", Text = "The maximum indemnity for third-party property damage is 130,000 US dollars; for bodily injury 260,000 US dollars per event." },
        };

        var newClauses = allClauses.Where(c => !existingClauseIds.Contains(c.ClauseId)).ToList();
        if (newClauses.Count > 0)
            await db.PolicyClauses.AddRangeAsync(newClauses, ct);

        // ---- Evidence chunks ----
        var newChunks = new List<EvidenceChunk>();
        void Add(string id, string claimId, string docId, string kind, int ord, string text)
        {
            if (!existingChunkIds.Contains(id))
                newChunks.Add(BuildChunk(embed, id, claimId, docId, kind, ord, text));
        }

        // ════════════════════════════════════════════════════════════════════════════════════
        // CLM-1006 — deep golden corpus (Toyota Camry 2021, POL-2025-AC-4421)
        //            Scenario: covered road accident collision; inflated invoice flagged for review
        // ════════════════════════════════════════════════════════════════════════════════════
        Add("CLM-1006-application#0", "CLM-1006", "CLM-1006-application", "application", 0,
            "Customer statement: a road traffic accident occurred on 18.05.2026 in Springfield. The Toyota Camry 2021 sustained damage to the front bumper and wing.");
        Add("CLM-1006-police#0", "CLM-1006", "CLM-1006-police", "police", 0,
            "Police report No. PR-2026/05/441: a collision between two vehicles on 18.05.2026 is confirmed. The other driver was found at fault and an administrative report was filed.");
        Add("CLM-1006-police#1", "CLM-1006", "CLM-1006-police", "police", 1,
            "Weather conditions at the time of the accident: rain, wet road surface. There were no injuries. Both vehicles remained at the scene until the patrol arrived.");
        Add("CLM-1006-invoice#0", "CLM-1006", "CLM-1006-invoice", "invoice", 0,
            "Repair invoice: front bumper replacement 980 dollars, painting 740 dollars, bodywork 1000 dollars. Total repair amount 2720 US dollars.");
        Add("CLM-1006-invoice#1", "CLM-1006", "CLM-1006-invoice", "invoice", 1,
            "The repair estimate of 2720 dollars exceeds the average benchmark of 1970 dollars by 38 percent. The discrepancy requires review by a human adjuster.");
        Add("CLM-1006-policy-terms#0", "CLM-1006", "CLM-1006-policy-terms", "policy-clause", 0,
            "Policy Auto Comprehensive POL-2025-AC-4421 covers losses from a road traffic accident. A deductible of 500 dollars applies to the insured event.");
        Add("CLM-1006-statement#0", "CLM-1006", "CLM-1006-application", "statement", 0,
            "Driver statement: the car ahead braked sharply and, because of the wet surface, the collision could not be avoided. Speed was within the limit.");
        Add("CLM-1006-photo-front#0", "CLM-1006", "CLM-1006-photo-front", "photo-caption", 0,
            "Photo of the front bumper: visible cracks and deformation, consistent with the description of the accident. Recognition confidence 92 percent.");
        Add("CLM-1006-photo-rear#0", "CLM-1006", "CLM-1006-photo-rear", "photo-caption", 0,
            "The rear bumper photo is missing. The document package is incomplete: 6 of 7 required items provided. The rear bumper photo must be requested.");
        // deepening chunks
        Add("CLM-1006-repair-detail#0", "CLM-1006", "CLM-1006-invoice", "invoice-detail", 0,
            "Repair breakdown: the front bumper of the Toyota Camry 2021 was replaced (original part, article 521190X910). The catalogue labour time for bodywork is 8 hours; 14 hours were billed.");
        Add("CLM-1006-repair-detail#1", "CLM-1006", "CLM-1006-invoice", "invoice-detail", 1,
            "Comparative assessment: average workshop rates in the Springfield area are 85-95 dollars per labour hour. The billed rate is 120 dollars. The rate excess requires human review.");
        Add("CLM-1006-coverage-check#0", "CLM-1006", "CLM-1006-policy-terms", "coverage-check", 0,
            "Exclusion check: the driver was sober at the time of the accident, no racing took place, and the road was a public road. No exclusion under policy POL-2025-AC-4421 was found. The insured event is covered.");
        Add("CLM-1006-approval-summary#0", "CLM-1006", "CLM-1006-application", "approval-summary", 0,
            "Preliminary conclusion: the insured event falls under Auto Comprehensive coverage. The amount payable after the 500 dollar deductible is to be confirmed once the adjuster reviews the invoice. Documents 6/7 — request the rear bumper photo.");

        // ════════════════════════════════════════════════════════════════════════════════════
        // CLM-1007 — separate claim (Skoda Octavia) — powers the cross-claim leakage guard
        //            Scenario: missing repair invoice
        // ════════════════════════════════════════════════════════════════════════════════════
        Add("CLM-1007-application#0", "CLM-1007", "CLM-1007-application", "application", 0,
            "Customer statement: a side collision on 26.05.2026 in Riverside. The Skoda Octavia sustained damage to the left door and mirror.");
        Add("CLM-1007-photo-front#0", "CLM-1007", "CLM-1007-photo-front", "photo-caption", 0,
            "Photo of the Skoda Octavia front wing: scratches and a minor dent. Recognition confidence 75 percent.");
        Add("CLM-1007-invoice#0", "CLM-1007", "CLM-1007-invoice", "invoice", 0,
            "The repair invoice is missing. Without the invoice the repair amount cannot be assessed. The document must be requested from the customer.");
        Add("CLM-1007-police#0", "CLM-1007", "CLM-1007-police", "police", 0,
            "Police report RIVERSIDE-2026/05/887: the collision is confirmed and an accident diagram was drawn. There were no injuries and the second driver admitted fault.");
        Add("CLM-1007-statement#0", "CLM-1007", "CLM-1007-application", "statement", 0,
            "Driver statement: the other vehicle failed to give way while changing lanes and struck the left wing of the Skoda Octavia. The policyholder braked but could not avoid the impact.");
        Add("CLM-1007-missing-docs#0", "CLM-1007", "CLM-1007-invoice", "missing-doc-check", 0,
            "Missing document list: repair invoice (required), photo of the right side of the vehicle (recommended). Without the invoice the indemnity amount cannot be determined. A request was sent to the customer.");

        // ════════════════════════════════════════════════════════════════════════════════════
        // CLM-1008 — complete low-risk claim (Riverside)
        //            Scenario: complete claim, all docs present, no anomalies
        // ════════════════════════════════════════════════════════════════════════════════════
        Add("CLM-1008-application#0", "CLM-1008", "CLM-1008-application", "application", 0,
            "Customer statement: road accident on 26.05.2026 in Riverside. The document package is complete.");
        Add("CLM-1008-police#0", "CLM-1008", "CLM-1008-police", "police", 0,
            "Police report RIVERSIDE-12345: the collision is confirmed and was filed under the simplified accident procedure.");
        Add("CLM-1008-invoice#0", "CLM-1008", "CLM-1008-invoice", "invoice", 0,
            "Repair invoice: the repair amount is within the average benchmark and no deviation was found.");
        Add("CLM-1008-coverage-check#0", "CLM-1008", "CLM-1008-policy-terms", "coverage-check", 0,
            "Coverage check: the insured event falls under the Auto Comprehensive policy. No exclusion was established. The documents are complete and the repair amount is normal.");
        Add("CLM-1008-photo#0", "CLM-1008", "CLM-1008-photo", "photo-caption", 0,
            "The photographs confirm the damage: the rear bumper photo shows body deformation consistent with the collision mechanism. Confidence 88 percent.");
        Add("CLM-1008-approval-summary#0", "CLM-1008", "CLM-1008-application", "approval-summary", 0,
            "Recommendation: the claim is fully documented and no anomalies were found. The payout may proceed after the standard signature check. Low risk.");

        // ════════════════════════════════════════════════════════════════════════════════════
        // CLM-1009 — exclusion: driving under influence (DUI)
        //            Scenario: NOT covered due to alcohol exclusion
        // ════════════════════════════════════════════════════════════════════════════════════
        Add("CLM-1009-application#0", "CLM-1009", "CLM-1009-application", "application", 0,
            "Customer statement: road accident on 02.06.2026 in Madison. The Honda Accord sustained damage to the right side of the body. The policyholder requests reimbursement of the repair.");
        Add("CLM-1009-police#0", "CLM-1009", "CLM-1009-police", "police", 0,
            "Police report MADISON-2026/06/102: an administrative offence report was filed. The Honda Accord driver was sent for a medical examination and alcohol intoxication of 1.2 promille was established.");
        Add("CLM-1009-police#1", "CLM-1009", "CLM-1009-police", "police", 1,
            "Medical report No. MD-2026/0612: alcohol intoxication of the policyholder at the time of the accident was established. The document is attached to the claim file.");
        Add("CLM-1009-exclusion-check#0", "CLM-1009", "CLM-1009-policy-terms", "coverage-check", 0,
            "Exclusion check: exclusion CLA-AC-EXCL-001 — losses caused while driving under alcohol intoxication are not covered. The medical report confirms the policyholder was intoxicated. The insured event is excluded from coverage.");
        Add("CLM-1009-denial-summary#0", "CLM-1009", "CLM-1009-application", "denial-summary", 0,
            "Preliminary conclusion: denial of the payout. Basis — clause CLA-AC-EXCL-001 of the Auto Comprehensive policy: alcohol intoxication is documented. Send the customer an official denial notice citing the exclusion clause.");
        Add("CLM-1009-invoice#0", "CLM-1009", "CLM-1009-invoice", "invoice", 0,
            "Repair invoice MADISON: repair of the right side of the Honda Accord — door replacement 1200 dollars, bodywork 600 dollars, painting 400 dollars. Total amount 2200 dollars. The invoice is filed with the claim, however no payout is made because of the exclusion.");
        Add("CLM-1009-coverage-final#0", "CLM-1009", "CLM-1009-policy-terms", "coverage-final", 0,
            "Coverage check summary for CLM-1009: the insured event is not covered. The intoxication exclusion applies based on the police report and the medical report. The payout is denied.");

        // ════════════════════════════════════════════════════════════════════════════════════
        // CLM-1010 — high-risk advisory: inflated invoice + photo mismatch
        //            Scenario: road accident collision (similar to CLM-1006 — cross-claim similarity signal)
        // ════════════════════════════════════════════════════════════════════════════════════
        Add("CLM-1010-application#0", "CLM-1010", "CLM-1010-application", "application", 0,
            "Customer statement: a road traffic accident on 28.05.2026 in Greenville. Volkswagen Passat 2020, a collision at an intersection, with damage to the front bumper and bonnet.");
        Add("CLM-1010-police#0", "CLM-1010", "CLM-1010-police", "police", 0,
            "Police report GREENVILLE-2026/05/339: a collision of vehicles at an intersection on 28.05.2026 is confirmed. Violation of intersection right-of-way rules. An administrative report was filed.");
        Add("CLM-1010-invoice#0", "CLM-1010", "CLM-1010-invoice", "invoice", 0,
            "Repair invoice Greenville: front bumper replacement for the Volkswagen Passat 1800 dollars, bonnet repair 2100 dollars, paint work 1400 dollars. Total repair amount 5300 US dollars.");
        Add("CLM-1010-invoice#1", "CLM-1010", "CLM-1010-invoice", "invoice", 1,
            "The repair estimate of 5300 dollars exceeds the average benchmark for the Volkswagen Passat (2800 dollars) by 89 percent. This significant benchmark excess requires review by a human adjuster.");
        Add("CLM-1010-photo-front#0", "CLM-1010", "CLM-1010-photo-front", "photo-caption", 0,
            "Photo of the Volkswagen Passat front bumper: visible scratches and slight deformation. The extent of damage is consistent with a low-speed collision.");
        Add("CLM-1010-photo-mismatch#0", "CLM-1010", "CLM-1010-photo-front", "photo-mismatch", 0,
            "Mismatch between the photo and the invoice: the invoice bills for bumper replacement and bonnet repair, yet the photo shows only bumper scratches with no bonnet damage. This mismatch between documents requires human review.");
        Add("CLM-1010-statement#0", "CLM-1010", "CLM-1010-application", "statement", 0,
            "Driver statement: the other car entered on a red light and struck the front of the Volkswagen Passat. The policyholder provided the contact details of a witness.");
        Add("CLM-1010-risk-summary#0", "CLM-1010", "CLM-1010-invoice", "risk-summary", 0,
            "Risk summary for CLM-1010: two anomalies were found — a significant benchmark excess of 89 percent and a mismatch between the photo and the list of works on the invoice. The claim requires human review before a decision. A payout is not recommended until the review is complete.");
        Add("CLM-1010-coverage-check#0", "CLM-1010", "CLM-1010-policy-terms", "coverage-check", 0,
            "Coverage check: the road accident at the intersection falls under Auto Comprehensive coverage. No exclusion (intoxication, racing) was established. However the payout is suspended because of the invoice and photo anomalies.");
        Add("CLM-1010-similar-signal#0", "CLM-1010", "CLM-1010-application", "similar-claim", 0,
            "Similar precedent: claim CLM-1006 also involved front bumper damage to a Toyota Camry from a collision together with a benchmark excess. The comparative analysis shows a recurring pattern of questionable invoices for bumper work.");

        // ════════════════════════════════════════════════════════════════════════════════════
        // CLM-1011 — missing police report
        //            Scenario: claim with absent police documentation
        // ════════════════════════════════════════════════════════════════════════════════════
        Add("CLM-1011-application#0", "CLM-1011", "CLM-1011-application", "application", 0,
            "Customer statement: road accident on 30.05.2026 in Salem. Hyundai Tucson, the rear bumper was damaged in a rear-end impact. The customer did not call the police and only signed a handwritten receipt with the other driver.");
        Add("CLM-1011-invoice#0", "CLM-1011", "CLM-1011-invoice", "invoice", 0,
            "Repair invoice Salem: rear bumper replacement for the Hyundai Tucson 850 dollars, bodywork 400 dollars. Total amount 1250 dollars. The invoice was provided, but the police report is missing.");
        Add("CLM-1011-missing-police#0", "CLM-1011", "CLM-1011-police", "missing-doc-check", 0,
            "The police report is missing. The customer stated that the police were not called. Only a handwritten receipt from the other driver was provided. Without an official police report the fact and circumstances of the accident cannot be confirmed.");
        Add("CLM-1011-missing-police#1", "CLM-1011", "CLM-1011-police", "missing-doc-check", 1,
            "Policy documentation requirements: an official police report or an accident certificate is required for road accident indemnity. A handwritten receipt is not an official document and cannot replace the police report.");
        Add("CLM-1011-statement#0", "CLM-1011", "CLM-1011-application", "statement", 0,
            "Driver statement: the customer states that at the time of the accident they settled with the other driver without calling the police. The other driver signed a receipt accepting responsibility. The customer asks that the claim be considered without a police report.");
        Add("CLM-1011-missing-docs-summary#0", "CLM-1011", "CLM-1011-police", "missing-doc-summary", 0,
            "Document summary for CLM-1011: the mandatory police report is missing. The repair invoice is present. A request must be sent to the customer to obtain an official accident document (police report or form 6 certificate).");
        Add("CLM-1011-risk-advisory#0", "CLM-1011", "CLM-1011-application", "risk-advisory", 0,
            "Advisory note: claim CLM-1011 cannot be resolved without the police report. Until an official document is provided the payout is suspended. The claim requires further documentation by the customer.");
        Add("CLM-1011-coverage-check#0", "CLM-1011", "CLM-1011-policy-terms", "coverage-check", 0,
            "Coverage check for CLM-1011: the rear-end impact on the Hyundai Tucson falls under Auto Comprehensive coverage provided official confirmation of the accident is supplied. Without a police report or certificate the insured event cannot be confirmed.");

        if (newChunks.Count > 0)
            await db.EvidenceChunks.AddRangeAsync(newChunks, ct);

        // ---- Gold evaluation questions ----
        var allQuestions = new List<RagEvaluationQuestion>
        {
            // ── CLM-1006 (4 questions — kept exactly as before for RagServiceTests count) ──
            new() { QuestionId = "Q-COVER-1006-1", ClaimId = "CLM-1006", UseCase = RagUseCases.Coverage, Language = "en",
                Text = "Does the policy cover road accident damage and what is the deductible?",
                ExpectedSourceChunkIdsCsv = "CLM-1006-policy-terms#0",
                ExpectedAnswerKeywordsCsv = "cover,deductible,500",
                MustNotCiteChunkIdsCsv = "CLM-1007-application#0,CLM-1007-invoice#0" },

            new() { QuestionId = "Q-MISS-1006-1", ClaimId = "CLM-1006", UseCase = RagUseCases.MissingDocs, Language = "en",
                Text = "Which documents are missing in this claim — is the rear bumper photo absent?",
                ExpectedSourceChunkIdsCsv = "CLM-1006-photo-rear#0",
                ExpectedAnswerKeywordsCsv = "photo,rear,missing",
                MustNotCiteChunkIdsCsv = "CLM-1007-photo-front#0" },

            new() { QuestionId = "Q-RISK-1006-1", ClaimId = "CLM-1006", UseCase = RagUseCases.Risk, Language = "en",
                Text = "Why does this claim carry elevated risk — does the repair estimate exceed the benchmark?",
                ExpectedSourceChunkIdsCsv = "CLM-1006-invoice#1",
                ExpectedAnswerKeywordsCsv = "exceeds,benchmark",
                MustNotCiteChunkIdsCsv = "CLM-1008-invoice#0" },

            new() { QuestionId = "Q-SUMM-1006-1", ClaimId = "CLM-1006", UseCase = RagUseCases.Summary, Language = "en",
                Text = "Prepare an evidence summary for the decision — police report, accident, repair invoice.",
                ExpectedSourceChunkIdsCsv = "CLM-1006-police#0,CLM-1006-invoice#0",
                ExpectedAnswerKeywordsCsv = "accident,repair",
                MustNotCiteChunkIdsCsv = "CLM-1007-application#0" },

            // ── CLM-1007 (2 questions) ──
            new() { QuestionId = "Q-COVER-1007-1", ClaimId = "CLM-1007", UseCase = RagUseCases.Coverage, Language = "en",
                Text = "What is known about the damage — side collision, door, mirror in this claim?",
                ExpectedSourceChunkIdsCsv = "CLM-1007-application#0",
                ExpectedAnswerKeywordsCsv = "side,door",
                MustNotCiteChunkIdsCsv = "CLM-1006-police#0,CLM-1006-invoice#0" },

            new() { QuestionId = "Q-MISS-1007-1", ClaimId = "CLM-1007", UseCase = RagUseCases.MissingDocs, Language = "en",
                Text = "Which documents are missing — is the repair invoice absent?",
                ExpectedSourceChunkIdsCsv = "CLM-1007-invoice#0",
                ExpectedAnswerKeywordsCsv = "invoice,missing",
                MustNotCiteChunkIdsCsv = "CLM-1006-invoice#0" },

            // ── CLM-1008 (3 questions) ──
            new() { QuestionId = "Q-SUMM-1008-1", ClaimId = "CLM-1008", UseCase = RagUseCases.Summary, Language = "en",
                Text = "Prepare an evidence summary — complete claim, low risk, payout recommendation.",
                ExpectedSourceChunkIdsCsv = "CLM-1008-approval-summary#0",
                ExpectedAnswerKeywordsCsv = "Low risk,anomalies,payout",
                MustNotCiteChunkIdsCsv = "CLM-1009-denial-summary#0" },

            new() { QuestionId = "Q-COVER-1008-1", ClaimId = "CLM-1008", UseCase = RagUseCases.Coverage, Language = "en",
                Text = "Is the insured event covered by the policy? Exclusion check, documents complete.",
                ExpectedSourceChunkIdsCsv = "CLM-1008-coverage-check#0",
                ExpectedAnswerKeywordsCsv = "covered,exclusion,complete",
                MustNotCiteChunkIdsCsv = "CLM-1009-exclusion-check#0" },

            new() { QuestionId = "Q-RISK-1008-1", ClaimId = "CLM-1008", UseCase = RagUseCases.Risk, Language = "en",
                Text = "What is the risk level of the claim — no deviation found, benchmark normal?",
                ExpectedSourceChunkIdsCsv = "CLM-1008-invoice#0",
                ExpectedAnswerKeywordsCsv = "benchmark,deviation",
                MustNotCiteChunkIdsCsv = "CLM-1010-invoice#1" },

            // ── CLM-1009 (4 questions) ──
            new() { QuestionId = "Q-COVER-1009-1", ClaimId = "CLM-1009", UseCase = RagUseCases.Coverage, Language = "en",
                Text = "Is this claim covered by the policy? Alcohol intoxication exclusion check.",
                ExpectedSourceChunkIdsCsv = "CLM-1009-exclusion-check#0",
                ExpectedAnswerKeywordsCsv = "exclusion,intoxication,alcohol",
                MustNotCiteChunkIdsCsv = "CLM-1006-policy-terms#0" },

            new() { QuestionId = "Q-DENY-1009-1", ClaimId = "CLM-1009", UseCase = RagUseCases.Summary, Language = "en",
                Text = "Prepare a denial summary — intoxication, denial, notice to the customer.",
                ExpectedSourceChunkIdsCsv = "CLM-1009-denial-summary#0",
                ExpectedAnswerKeywordsCsv = "denial,intoxication,payout",
                MustNotCiteChunkIdsCsv = "CLM-1008-approval-summary#0" },

            new() { QuestionId = "Q-RISK-1009-1", ClaimId = "CLM-1009", UseCase = RagUseCases.Risk, Language = "en",
                Text = "What is the basis for denial — medical report, alcohol, police report?",
                ExpectedSourceChunkIdsCsv = "CLM-1009-police#1",
                ExpectedAnswerKeywordsCsv = "Medical report,alcohol,intoxication",
                MustNotCiteChunkIdsCsv = "CLM-1006-police#0" },

            new() { QuestionId = "Q-MISS-1009-1", ClaimId = "CLM-1009", UseCase = RagUseCases.MissingDocs, Language = "en",
                Text = "Which documents are present in the claim — repair invoice, police report?",
                ExpectedSourceChunkIdsCsv = "CLM-1009-invoice#0",
                ExpectedAnswerKeywordsCsv = "invoice,repair,amount",
                MustNotCiteChunkIdsCsv = "CLM-1011-missing-police#0" },

            // ── CLM-1010 (4 questions) ──
            new() { QuestionId = "Q-RISK-1010-1", ClaimId = "CLM-1010", UseCase = RagUseCases.Risk, Language = "en",
                Text = "Why does the claim carry elevated risk — benchmark excess and photo/invoice mismatch?",
                ExpectedSourceChunkIdsCsv = "CLM-1010-invoice#1,CLM-1010-photo-mismatch#0",
                ExpectedAnswerKeywordsCsv = "excess,benchmark,mismatch",
                MustNotCiteChunkIdsCsv = "CLM-1006-invoice#1" },

            new() { QuestionId = "Q-RISK-1010-2", ClaimId = "CLM-1010", UseCase = RagUseCases.Risk, Language = "en",
                Text = "What decision is recommended for a risky claim — human review, payout suspension?",
                ExpectedSourceChunkIdsCsv = "CLM-1010-risk-summary#0",
                ExpectedAnswerKeywordsCsv = "anomalies,requires,review",
                MustNotCiteChunkIdsCsv = "CLM-1008-approval-summary#0" },

            new() { QuestionId = "Q-COVER-1010-1", ClaimId = "CLM-1010", UseCase = RagUseCases.Coverage, Language = "en",
                Text = "Does the intersection road accident fall under Auto Comprehensive coverage with no exclusion established?",
                ExpectedSourceChunkIdsCsv = "CLM-1010-coverage-check#0",
                ExpectedAnswerKeywordsCsv = "coverage,exclusion,intersection",
                MustNotCiteChunkIdsCsv = "CLM-1009-exclusion-check#0" },

            new() { QuestionId = "Q-SUMM-1010-1", ClaimId = "CLM-1010", UseCase = RagUseCases.Summary, Language = "en",
                Text = "Evidence summary for the claim — invoice, repair, bumper, bonnet, police report Greenville?",
                ExpectedSourceChunkIdsCsv = "CLM-1010-police#0,CLM-1010-invoice#0",
                ExpectedAnswerKeywordsCsv = "collision,bumper,repair",
                MustNotCiteChunkIdsCsv = "CLM-1006-police#0" },

            // ── CLM-1011 (4 questions) ──
            new() { QuestionId = "Q-MISS-1011-1", ClaimId = "CLM-1011", UseCase = RagUseCases.MissingDocs, Language = "en",
                Text = "Which documents are missing — police report absent, handwritten receipt does not replace it?",
                ExpectedSourceChunkIdsCsv = "CLM-1011-missing-police#0",
                ExpectedAnswerKeywordsCsv = "police report,missing,receipt",
                MustNotCiteChunkIdsCsv = "CLM-1007-invoice#0" },

            new() { QuestionId = "Q-MISS-1011-2", ClaimId = "CLM-1011", UseCase = RagUseCases.MissingDocs, Language = "en",
                Text = "Which documents does the policy require for accident indemnity — official report or certificate?",
                ExpectedSourceChunkIdsCsv = "CLM-1011-missing-police#1",
                ExpectedAnswerKeywordsCsv = "police report,certificate,official",
                MustNotCiteChunkIdsCsv = "CLM-1007-missing-docs#0" },

            new() { QuestionId = "Q-RISK-1011-1", ClaimId = "CLM-1011", UseCase = RagUseCases.Risk, Language = "en",
                Text = "What is the claim risk without a police report — payout suspended, documentation?",
                ExpectedSourceChunkIdsCsv = "CLM-1011-risk-advisory#0",
                ExpectedAnswerKeywordsCsv = "suspended,documentation,police report",
                MustNotCiteChunkIdsCsv = "CLM-1010-risk-summary#0" },

            new() { QuestionId = "Q-SUMM-1011-1", ClaimId = "CLM-1011", UseCase = RagUseCases.Summary, Language = "en",
                Text = "Prepare the list of missing documents for the decision — request police report, form 6?",
                ExpectedSourceChunkIdsCsv = "CLM-1011-missing-docs-summary#0",
                ExpectedAnswerKeywordsCsv = "police report,form 6,request",
                MustNotCiteChunkIdsCsv = "CLM-1009-denial-summary#0" },
        };

        var newQuestions = allQuestions.Where(q => !existingQIds.Contains(q.QuestionId)).ToList();
        if (newQuestions.Count > 0)
            await db.RagEvaluationQuestions.AddRangeAsync(newQuestions, ct);

        await db.SaveChangesAsync(ct);
    }

    private static EvidenceChunk BuildChunk(
        IEmbeddingProvider embed, string id, string claimId, string docId, string kind, int ordinal, string text)
    {
        var vector = embed.Embed(text);
        return new EvidenceChunk
        {
            ChunkId = id,
            ClaimId = claimId,
            DocumentId = docId,
            Kind = kind,
            Ordinal = ordinal,
            Text = text,
            TokenCount = Math.Max(1, text.Length / 4),
            ChunkHash = Hash(text),
            Language = "en",
            SourceVersion = "v0.1",
            EmbeddingModel = embed.ModelName,
            EmbeddingDim = embed.Dimensions,
            EmbeddingJson = EmbeddingCodec.ToJson(vector)
        };
    }

    private static string Hash(string text) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)))[..16];
}
