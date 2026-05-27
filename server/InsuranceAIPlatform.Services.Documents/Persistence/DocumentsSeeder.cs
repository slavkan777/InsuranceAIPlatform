using InsuranceAIPlatform.BuildingBlocks;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAIPlatform.Services.Documents.Persistence;

/// <summary>
/// Idempotent seed for the Documents context.
/// CLM-1006: 7 documents exactly as in InMemoryClaimReadService.
/// Plus a few docs for CLM-1007 and CLM-1008.
/// </summary>
public static class DocumentsSeeder
{
    public static async Task SeedAsync(DocumentsDbContext db, CancellationToken ct = default)
    {
        if (await db.ClaimDocuments.AnyAsync(ct))
            return;

        var docs = new List<ClaimDocument>
        {
            // CLM-1006 — 7 documents exactly mirroring InMemoryClaimReadService
            new() { Id = "CLM-1006-application",  ClaimId = "CLM-1006", Kind = "application",  Title = "Заява клієнта",        Meta = "19.05.2026",            Status = "ok",      DocType = "document", AiConfidence = null },
            new() { Id = "CLM-1006-police",        ClaimId = "CLM-1006", Kind = "police",        Title = "Поліцейський звіт",    Meta = "NoБРС-2026/05/441",    Status = "ok",      DocType = "document", AiConfidence = null },
            new() { Id = "CLM-1006-photo-front",   ClaimId = "CLM-1006", Kind = "photo-front",   Title = "Фото — переднє",       Meta = "AI conf 92%",           Status = "ok",      DocType = "photo",    AiConfidence = 92 },
            new() { Id = "CLM-1006-photo-side",    ClaimId = "CLM-1006", Kind = "photo-side",    Title = "Фото — бокове",        Meta = "AI conf 87%",           Status = "ok",      DocType = "photo",    AiConfidence = 87 },
            new() { Id = "CLM-1006-invoice",       ClaimId = "CLM-1006", Kind = "invoice",       Title = "Рахунок СТО",          Meta = "Сума +38%",             Status = "warn",    DocType = "document", AiConfidence = null },
            new() { Id = "CLM-1006-policy-terms",  ClaimId = "CLM-1006", Kind = "policy-terms",  Title = "Умови полісу",         Meta = "Auto Comprehensive",    Status = "ok",      DocType = "document", AiConfidence = null },
            new() { Id = "CLM-1006-photo-rear",    ClaimId = "CLM-1006", Kind = "photo-rear",    Title = "Фото — задній бампер", Meta = "ВІДСУТНЄ",              Status = "missing", DocType = "photo",    AiConfidence = null },

            // CLM-1007 — partial docs
            new() { Id = "CLM-1007-application",   ClaimId = "CLM-1007", Kind = "application", Title = "Заява клієнта",       Meta = "26.05.2026", Status = "ok",   DocType = "document", AiConfidence = null },
            new() { Id = "CLM-1007-photo-front",   ClaimId = "CLM-1007", Kind = "photo-front",  Title = "Фото — переднє",      Meta = "AI conf 75%", Status = "ok",  DocType = "photo",    AiConfidence = 75 },
            new() { Id = "CLM-1007-invoice",       ClaimId = "CLM-1007", Kind = "invoice",      Title = "Рахунок СТО",         Meta = "ВІДСУТНЄ",    Status = "missing", DocType = "document", AiConfidence = null },

            // CLM-1008 — complete docs
            new() { Id = "CLM-1008-application",   ClaimId = "CLM-1008", Kind = "application", Title = "Заява клієнта",       Meta = "26.05.2026",   Status = "ok", DocType = "document", AiConfidence = null },
            new() { Id = "CLM-1008-police",        ClaimId = "CLM-1008", Kind = "police",       Title = "Поліцейський звіт",   Meta = "ХАРКІВ-12345", Status = "ok", DocType = "document", AiConfidence = null },
            new() { Id = "CLM-1008-photo-front",   ClaimId = "CLM-1008", Kind = "photo-front",  Title = "Фото — переднє",      Meta = "AI conf 88%",  Status = "ok", DocType = "photo",    AiConfidence = 88 },
            new() { Id = "CLM-1008-invoice",       ClaimId = "CLM-1008", Kind = "invoice",      Title = "Рахунок СТО",         Meta = "Сума OK",      Status = "ok", DocType = "document", AiConfidence = null },
        };

        await db.ClaimDocuments.AddRangeAsync(docs, ct);
        await db.SaveChangesAsync(ct);
    }
}
