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
            new() { ClauseId = "CLA-AC-COVER-001", ProductCode = ProductAutoComprehensive, PolicyId = "POL-2025-AC-4421", ClauseType = "coverage",   Ordinal = 1, Title = "Покриття ДТП", Text = "Поліс Auto Comprehensive покриває збитки внаслідок дорожньо-транспортної пригоди та зіткнення транспортних засобів." },
            new() { ClauseId = "CLA-AC-COVER-002", ProductCode = ProductAutoComprehensive, PolicyId = "POL-2025-AC-4421", ClauseType = "coverage",   Ordinal = 2, Title = "Стихійні явища і крадіжка", Text = "Покриваються пошкодження від стихійних явищ, пожежі та крадіжки транспортного засобу." },
            new() { ClauseId = "CLA-AC-EXCL-001",  ProductCode = ProductAutoComprehensive, PolicyId = "POL-2025-AC-4421", ClauseType = "exclusion",  Ordinal = 3, Title = "Виключення: стан спʼяніння", Text = "Не покриваються збитки, завдані під час керування у стані алкогольного або наркотичного спʼяніння." },
            new() { ClauseId = "CLA-AC-EXCL-002",  ProductCode = ProductAutoComprehensive, PolicyId = "POL-2025-AC-4421", ClauseType = "exclusion",  Ordinal = 4, Title = "Виключення: перегони", Text = "Не покриваються збитки під час участі у перегонах або експлуатації поза дорогами загального користування." },
            new() { ClauseId = "CLA-AC-DED-001",   ProductCode = ProductAutoComprehensive, PolicyId = "POL-2025-AC-4421", ClauseType = "deductible", Ordinal = 5, Title = "Франшиза", Text = "До кожного страхового випадку застосовується франшиза у розмірі 500 доларів США." },
            new() { ClauseId = "CLA-AC-LIM-001",   ProductCode = ProductAutoComprehensive, PolicyId = "POL-2025-AC-4421", ClauseType = "limit",      Ordinal = 6, Title = "Ліміт на кузовний ремонт", Text = "Ліміт відшкодування кузовного ремонту визначається ринковою вартістю аналогічних робіт у регіоні." },
            // Auto Third-Party product (two extra clauses)
            new() { ClauseId = "CLA-TP-COVER-001", ProductCode = ProductAutoThirdParty,    PolicyId = "POL-2025-TP-0091", ClauseType = "coverage",   Ordinal = 1, Title = "Покриття ОСЦПВ", Text = "Поліс Auto Third-Party покриває збитки, заподіяні третім особам внаслідок дорожньо-транспортної пригоди з вини страхувальника." },
            new() { ClauseId = "CLA-TP-LIM-001",   ProductCode = ProductAutoThirdParty,    PolicyId = "POL-2025-TP-0091", ClauseType = "limit",      Ordinal = 2, Title = "Ліміт відповідальності ОСЦПВ", Text = "Максимальне відшкодування шкоди майну третіх осіб — 130 000 гривень; шкоди здоров'ю — 260 000 гривень на одну подію." },
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
        //            Scenario: covered ДТП collision; inflated invoice flagged for review
        // ════════════════════════════════════════════════════════════════════════════════════
        Add("CLM-1006-application#0", "CLM-1006", "CLM-1006-application", "application", 0,
            "Заява клієнта: дорожньо-транспортна пригода сталася 18.05.2026 у місті Бориспіль. Автомобіль Toyota Camry 2021 отримав пошкодження переднього бампера та крила.");
        Add("CLM-1006-police#0", "CLM-1006", "CLM-1006-police", "police", 0,
            "Поліцейський звіт No БРС-2026/05/441: підтверджено факт зіткнення двох транспортних засобів 18.05.2026. Винуватцем визнано іншого водія, складено адміністративний протокол.");
        Add("CLM-1006-police#1", "CLM-1006", "CLM-1006-police", "police", 1,
            "Погодні умови на момент ДТП: дощ, мокре дорожнє покриття. Постраждалих немає. Обидва транспортні засоби залишалися на місці події до приїзду патруля.");
        Add("CLM-1006-invoice#0", "CLM-1006", "CLM-1006-invoice", "invoice", 0,
            "Рахунок СТО: заміна переднього бампера 980 доларів, лакування 740 доларів, кузовні роботи 1000 доларів. Загальна сума ремонту 2720 доларів США.");
        Add("CLM-1006-invoice#1", "CLM-1006", "CLM-1006-invoice", "invoice", 1,
            "Оцінка ремонту 2720 доларів перевищує середній бенчмарк 1970 доларів на 38 відсотків. Розбіжність потребує перевірки людиною-ад'юстером.");
        Add("CLM-1006-policy-terms#0", "CLM-1006", "CLM-1006-policy-terms", "policy-clause", 0,
            "Поліс Auto Comprehensive POL-2025-AC-4421 покриває збитки від дорожньо-транспортної пригоди. До страхового випадку застосовується франшиза 500 доларів.");
        Add("CLM-1006-statement#0", "CLM-1006", "CLM-1006-application", "statement", 0,
            "Пояснення водія: автомобіль попереду різко загальмував, через мокре покриття уникнути зіткнення не вдалося. Швидкість була в межах дозволеної.");
        Add("CLM-1006-photo-front#0", "CLM-1006", "CLM-1006-photo-front", "photo-caption", 0,
            "Фото переднього бампера: видимі тріщини та деформація, відповідає опису пригоди. Достовірність розпізнавання 92 відсотки.");
        Add("CLM-1006-photo-rear#0", "CLM-1006", "CLM-1006-photo-rear", "photo-caption", 0,
            "Фото заднього бампера відсутнє. Пакет документів неповний: надано 6 із 7 обов'язкових позицій. Потрібно дозапросити фото заднього бампера.");
        // deepening chunks
        Add("CLM-1006-repair-detail#0", "CLM-1006", "CLM-1006-invoice", "invoice-detail", 0,
            "Деталізація СТО: замінено передній бампер Toyota Camry 2021 (оригінальна запчастина, артикул 521190X910). Норма часу кузовних робіт за каталогом — 8 годин, виставлено 14 годин.");
        Add("CLM-1006-repair-detail#1", "CLM-1006", "CLM-1006-invoice", "invoice-detail", 1,
            "Порівняльна оцінка: середні ставки СТО у Бориспільському районі складають 85–95 доларів за нормо-годину. Виставлена ставка 120 доларів. Перевищення ставки потребує перевірки людиною.");
        Add("CLM-1006-coverage-check#0", "CLM-1006", "CLM-1006-policy-terms", "coverage-check", 0,
            "Перевірка виключень: водій на момент ДТП тверезий, перегони не проводилися, дорога загального користування. Жодного виключення за полісом POL-2025-AC-4421 не виявлено. Страховий випадок покривається.");
        Add("CLM-1006-approval-summary#0", "CLM-1006", "CLM-1006-application", "approval-summary", 0,
            "Попереднє рішення: страховий випадок підпадає під покриття поліса Auto Comprehensive. Сума до відшкодування після франшизи 500 доларів підлягає уточненню після перевірки рахунку ад'юстером. Документи 6/7 — запитати фото заднього бампера.");

        // ════════════════════════════════════════════════════════════════════════════════════
        // CLM-1007 — separate claim (Skoda Octavia) — powers the cross-claim leakage guard
        //            Scenario: missing repair invoice
        // ════════════════════════════════════════════════════════════════════════════════════
        Add("CLM-1007-application#0", "CLM-1007", "CLM-1007-application", "application", 0,
            "Заява клієнта: бокове зіткнення 26.05.2026 у місті Київ. Автомобіль Skoda Octavia, пошкоджено ліві двері та дзеркало.");
        Add("CLM-1007-photo-front#0", "CLM-1007", "CLM-1007-photo-front", "photo-caption", 0,
            "Фото переднього крила Skoda Octavia: подряпини та незначна вмʼятина. Достовірність розпізнавання 75 відсотків.");
        Add("CLM-1007-invoice#0", "CLM-1007", "CLM-1007-invoice", "invoice", 0,
            "Рахунок СТО відсутній. Без рахунку неможливо оцінити суму ремонту. Документ потрібно дозапросити у клієнта.");
        Add("CLM-1007-police#0", "CLM-1007", "CLM-1007-police", "police", 0,
            "Поліцейський звіт КИЇВ-2026/05/887: зіткнення підтверджено, складено схему ДТП. Постраждалих немає, другий водій визнав провину.");
        Add("CLM-1007-statement#0", "CLM-1007", "CLM-1007-application", "statement", 0,
            "Пояснення водія: інший транспортний засіб не надав перевагу під час перестроювання, вдарив у ліве крило Skoda Octavia. Страхувальник гальмував але уникнути удару не встиг.");
        Add("CLM-1007-missing-docs#0", "CLM-1007", "CLM-1007-invoice", "missing-doc-check", 0,
            "Перелік відсутніх документів: рахунок СТО (обов'язковий), фото правого боку автомобіля (рекомендований). Без рахунку сума відшкодування не може бути визначена. Направлено запит клієнту.");

        // ════════════════════════════════════════════════════════════════════════════════════
        // CLM-1008 — complete low-risk claim (Харків)
        //            Scenario: complete claim, all docs present, no anomalies
        // ════════════════════════════════════════════════════════════════════════════════════
        Add("CLM-1008-application#0", "CLM-1008", "CLM-1008-application", "application", 0,
            "Заява клієнта: ДТП 26.05.2026 у місті Харків. Пакет документів повний.");
        Add("CLM-1008-police#0", "CLM-1008", "CLM-1008-police", "police", 0,
            "Поліцейський звіт ХАРКІВ-12345: зіткнення підтверджено, оформлено за європротоколом.");
        Add("CLM-1008-invoice#0", "CLM-1008", "CLM-1008-invoice", "invoice", 0,
            "Рахунок СТО: сума ремонту в межах середнього бенчмарку, відхилень не виявлено.");
        Add("CLM-1008-coverage-check#0", "CLM-1008", "CLM-1008-policy-terms", "coverage-check", 0,
            "Перевірка покриття: страховий випадок підпадає під дію поліса Auto Comprehensive. Виключень не встановлено. Документи повні, сума ремонту в нормі.");
        Add("CLM-1008-photo#0", "CLM-1008", "CLM-1008-photo", "photo-caption", 0,
            "Фотоматеріали підтверджують пошкодження: фото заднього бампера, деформація кузова відповідає механізму зіткнення. Достовірність 88 відсотків.");
        Add("CLM-1008-approval-summary#0", "CLM-1008", "CLM-1008-application", "approval-summary", 0,
            "Рекомендація: справа повністю задокументована, аномалій не виявлено. Виплата може бути здійснена після стандартної перевірки підписів. Низький ризик.");

        // ════════════════════════════════════════════════════════════════════════════════════
        // CLM-1009 — exclusion: driving under influence (DUI)
        //            Scenario: NOT covered due to alcohol exclusion
        // ════════════════════════════════════════════════════════════════════════════════════
        Add("CLM-1009-application#0", "CLM-1009", "CLM-1009-application", "application", 0,
            "Заява клієнта: ДТП 02.06.2026 у місті Дніпро. Автомобіль Honda Accord, пошкоджено правий бік кузова. Страхувальник просить відшкодування ремонту.");
        Add("CLM-1009-police#0", "CLM-1009", "CLM-1009-police", "police", 0,
            "Поліцейський звіт ДНІПРО-2026/06/102: складено протокол про адміністративне правопорушення. Водія Honda Accord направлено на медичний огляд, встановлено алкогольне спʼяніння 1.2 проміле.");
        Add("CLM-1009-police#1", "CLM-1009", "CLM-1009-police", "police", 1,
            "Медичний висновок No ДН-2026/0612: встановлено стан алкогольного спʼяніння страхувальника на момент ДТП. Документ приєднано до матеріалів справи.");
        Add("CLM-1009-exclusion-check#0", "CLM-1009", "CLM-1009-policy-terms", "coverage-check", 0,
            "Перевірка виключень: виключення CLA-AC-EXCL-001 — збитки завдані під час керування у стані алкогольного спʼяніння не покриваються. Медичний висновок підтверджує спʼяніння страхувальника. Страховий випадок виключений з покриття.");
        Add("CLM-1009-denial-summary#0", "CLM-1009", "CLM-1009-application", "denial-summary", 0,
            "Попереднє рішення: відмова у виплаті. Підстава — пункт CLA-AC-EXCL-001 поліса Auto Comprehensive: стан алкогольного спʼяніння підтверджено документально. Надіслати клієнту офіційне повідомлення про відмову із зазначенням пункту виключення.");
        Add("CLM-1009-invoice#0", "CLM-1009", "CLM-1009-invoice", "invoice", 0,
            "Рахунок СТО ДНІПРО: ремонт правого боку Honda Accord — заміна дверей 1200 доларів, кузовні роботи 600 доларів, лакування 400 доларів. Загальна сума 2200 доларів. Рахунок прийнятий до матеріалів справи, однак виплата не здійснюється через виключення.");
        Add("CLM-1009-coverage-final#0", "CLM-1009", "CLM-1009-policy-terms", "coverage-final", 0,
            "Підсумок перевірки покриття CLM-1009: страховий випадок не покривається. Виключення спʼяніння застосовується на підставі поліцейського протоколу та медичного висновку. Виплата відмовлена.");

        // ════════════════════════════════════════════════════════════════════════════════════
        // CLM-1010 — high-risk advisory: inflated invoice + photo mismatch
        //            Scenario: ДТП collision (similar to CLM-1006 — cross-claim similarity signal)
        // ════════════════════════════════════════════════════════════════════════════════════
        Add("CLM-1010-application#0", "CLM-1010", "CLM-1010-application", "application", 0,
            "Заява клієнта: дорожньо-транспортна пригода 28.05.2026 у місті Одеса. Автомобіль Volkswagen Passat 2020, зіткнення на перехресті, пошкоджено передній бампер та капот.");
        Add("CLM-1010-police#0", "CLM-1010", "CLM-1010-police", "police", 0,
            "Поліцейський звіт ОДЕСА-2026/05/339: підтверджено факт зіткнення транспортних засобів 28.05.2026 на перехресті. Порушення правил проїзду перехрестя. Адміністративний протокол складено.");
        Add("CLM-1010-invoice#0", "CLM-1010", "CLM-1010-invoice", "invoice", 0,
            "Рахунок СТО Одеса: заміна переднього бампера Volkswagen Passat 1800 доларів, ремонт капота 2100 доларів, малярні роботи 1400 доларів. Загальна сума ремонту 5300 доларів США.");
        Add("CLM-1010-invoice#1", "CLM-1010", "CLM-1010-invoice", "invoice", 1,
            "Оцінка ремонту 5300 доларів перевищує середній бенчмарк для Volkswagen Passat (2800 доларів) на 89 відсотків. Значне перевищення бенчмарку потребує перевірки людиною-ад'юстером.");
        Add("CLM-1010-photo-front#0", "CLM-1010", "CLM-1010-photo-front", "photo-caption", 0,
            "Фото переднього бампера Volkswagen Passat: видимі подряпини та незначна деформація. Ступінь пошкодження відповідає низько-швидкісному зіткненню.");
        Add("CLM-1010-photo-mismatch#0", "CLM-1010", "CLM-1010-photo-front", "photo-mismatch", 0,
            "Невідповідність фото та рахунку: рахунок виставлено за заміну бампера та ремонт капота, однак фото демонструє лише подряпини бампера без пошкодження капота. Розбіжність між документами потребує перевірки людиною.");
        Add("CLM-1010-statement#0", "CLM-1010", "CLM-1010-application", "statement", 0,
            "Пояснення водія: інший автомобіль виїхав на червоний сигнал, удар у передню частину Volkswagen Passat. Страхувальник надав контактні дані свідка.");
        Add("CLM-1010-risk-summary#0", "CLM-1010", "CLM-1010-invoice", "risk-summary", 0,
            "Підсумок ризику CLM-1010: виявлено дві аномалії — значне перевищення бенчмарку на 89 відсотків та невідповідність між фото та переліком робіт у рахунку. Справа потребує перевірки людиною перед прийняттям рішення. Виплата не рекомендується до завершення перевірки.");
        Add("CLM-1010-coverage-check#0", "CLM-1010", "CLM-1010-policy-terms", "coverage-check", 0,
            "Перевірка покриття: ДТП на перехресті підпадає під покриття поліса Auto Comprehensive. Виключень (спʼяніння, перегони) не встановлено. Проте виплата призупинена через аномалії рахунку та фото.");
        Add("CLM-1010-similar-signal#0", "CLM-1010", "CLM-1010-application", "similar-claim", 0,
            "Схожий прецедент: справа CLM-1006 також містила пошкодження переднього бампера Toyota Camry внаслідок зіткнення та перевищення бенчмарку. Порівняльний аналіз підтверджує шаблон підозрілих рахунків за бамперні роботи.");

        // ════════════════════════════════════════════════════════════════════════════════════
        // CLM-1011 — missing police report
        //            Scenario: claim with absent police documentation
        // ════════════════════════════════════════════════════════════════════════════════════
        Add("CLM-1011-application#0", "CLM-1011", "CLM-1011-application", "application", 0,
            "Заява клієнта: ДТП 30.05.2026 у місті Запоріжжя. Автомобіль Hyundai Tucson, задній бампер пошкоджено внаслідок наїзду ззаду. Клієнт не викликав поліцію, оформив тільки розписку з іншим водієм.");
        Add("CLM-1011-invoice#0", "CLM-1011", "CLM-1011-invoice", "invoice", 0,
            "Рахунок СТО Запоріжжя: заміна заднього бампера Hyundai Tucson 850 доларів, кузовні роботи 400 доларів. Загальна сума 1250 доларів. Рахунок наданий, але поліцейський звіт відсутній.");
        Add("CLM-1011-missing-police#0", "CLM-1011", "CLM-1011-police", "missing-doc-check", 0,
            "Поліцейський звіт відсутній. Клієнт повідомив, що не викликав поліцію. Надано лише рукописну розписку від іншого водія. Без офіційного поліцейського звіту неможливо підтвердити факт та обставини ДТП.");
        Add("CLM-1011-missing-police#1", "CLM-1011", "CLM-1011-police", "missing-doc-check", 1,
            "Вимоги поліса щодо документування: для відшкодування за ДТП необхідний офіційний поліцейський протокол або довідка про ДТП. Рукописна розписка не є офіційним документом та не може замінити поліцейський звіт.");
        Add("CLM-1011-statement#0", "CLM-1011", "CLM-1011-application", "statement", 0,
            "Пояснення водія: стверджує, що на момент ДТП погодився з іншим водієм без виклику поліції. Інший водій підписав розписку про відповідальність. Клієнт просить розглянути справу без поліцейського звіту.");
        Add("CLM-1011-missing-docs-summary#0", "CLM-1011", "CLM-1011-police", "missing-doc-summary", 0,
            "Підсумок документів CLM-1011: відсутній обов'язковий поліцейський звіт. Рахунок СТО наявний. Потрібно направити клієнту запит на отримання офіційного документа про ДТП (поліцейський протокол або довідка форми 6).");
        Add("CLM-1011-risk-advisory#0", "CLM-1011", "CLM-1011-application", "risk-advisory", 0,
            "Консультативна позначка: справа CLM-1011 не може бути вирішена без поліцейського звіту. До надання офіційного документа виплата призупинена. Справа потребує подальшого документування клієнтом.");
        Add("CLM-1011-coverage-check#0", "CLM-1011", "CLM-1011-policy-terms", "coverage-check", 0,
            "Перевірка покриття CLM-1011: подія наїзду ззаду на Hyundai Tucson підпадає під покриття поліса Auto Comprehensive за умови надання офіційного підтвердження ДТП. Без поліцейського звіту або довідки страховий випадок не може бути підтверджено.");

        if (newChunks.Count > 0)
            await db.EvidenceChunks.AddRangeAsync(newChunks, ct);

        // ---- Gold evaluation questions ----
        var allQuestions = new List<RagEvaluationQuestion>
        {
            // ── CLM-1006 (4 questions — kept exactly as before for RagServiceTests count) ──
            new() { QuestionId = "Q-COVER-1006-1", ClaimId = "CLM-1006", UseCase = RagUseCases.Coverage, Language = "uk",
                Text = "Чи покриває поліс пошкодження від ДТП і яка франшиза?",
                ExpectedSourceChunkIdsCsv = "CLM-1006-policy-terms#0",
                ExpectedAnswerKeywordsCsv = "покрива,франшиз,500",
                MustNotCiteChunkIdsCsv = "CLM-1007-application#0,CLM-1007-invoice#0" },

            new() { QuestionId = "Q-MISS-1006-1", ClaimId = "CLM-1006", UseCase = RagUseCases.MissingDocs, Language = "uk",
                Text = "Яких документів бракує у справі — фото задній бампер відсутнє?",
                ExpectedSourceChunkIdsCsv = "CLM-1006-photo-rear#0",
                ExpectedAnswerKeywordsCsv = "фото,задн,відсут",
                MustNotCiteChunkIdsCsv = "CLM-1007-photo-front#0" },

            new() { QuestionId = "Q-RISK-1006-1", ClaimId = "CLM-1006", UseCase = RagUseCases.Risk, Language = "uk",
                Text = "Чому ця справа має підвищений ризик — оцінка ремонту перевищує бенчмарк?",
                ExpectedSourceChunkIdsCsv = "CLM-1006-invoice#1",
                ExpectedAnswerKeywordsCsv = "перевищує,бенчмарк",
                MustNotCiteChunkIdsCsv = "CLM-1008-invoice#0" },

            new() { QuestionId = "Q-SUMM-1006-1", ClaimId = "CLM-1006", UseCase = RagUseCases.Summary, Language = "uk",
                Text = "Підготуй зведення доказів для рішення — поліцейський звіт ДТП рахунок ремонту.",
                ExpectedSourceChunkIdsCsv = "CLM-1006-police#0,CLM-1006-invoice#0",
                ExpectedAnswerKeywordsCsv = "ДТП,ремонт",
                MustNotCiteChunkIdsCsv = "CLM-1007-application#0" },

            // ── CLM-1007 (2 questions) ──
            new() { QuestionId = "Q-COVER-1007-1", ClaimId = "CLM-1007", UseCase = RagUseCases.Coverage, Language = "uk",
                Text = "Що відомо про пошкодження бокове зіткнення двері дзеркало у цій справі?",
                ExpectedSourceChunkIdsCsv = "CLM-1007-application#0",
                ExpectedAnswerKeywordsCsv = "бокове,двер",
                MustNotCiteChunkIdsCsv = "CLM-1006-police#0,CLM-1006-invoice#0" },

            new() { QuestionId = "Q-MISS-1007-1", ClaimId = "CLM-1007", UseCase = RagUseCases.MissingDocs, Language = "uk",
                Text = "Яких документів бракує — рахунок СТО відсутній?",
                ExpectedSourceChunkIdsCsv = "CLM-1007-invoice#0",
                ExpectedAnswerKeywordsCsv = "рахунок,відсут",
                MustNotCiteChunkIdsCsv = "CLM-1006-invoice#0" },

            // ── CLM-1008 (3 questions) ──
            new() { QuestionId = "Q-SUMM-1008-1", ClaimId = "CLM-1008", UseCase = RagUseCases.Summary, Language = "uk",
                Text = "Підготуй зведення доказів повна справа низький ризик рекомендація виплата.",
                ExpectedSourceChunkIdsCsv = "CLM-1008-approval-summary#0",
                ExpectedAnswerKeywordsCsv = "низький,аномалій,виплата",
                MustNotCiteChunkIdsCsv = "CLM-1009-denial-summary#0" },

            new() { QuestionId = "Q-COVER-1008-1", ClaimId = "CLM-1008", UseCase = RagUseCases.Coverage, Language = "uk",
                Text = "Чи покривається страховий випадок полісом? Перевірка виключень документи повні.",
                ExpectedSourceChunkIdsCsv = "CLM-1008-coverage-check#0",
                ExpectedAnswerKeywordsCsv = "покрива,виключень,повн",
                MustNotCiteChunkIdsCsv = "CLM-1009-exclusion-check#0" },

            new() { QuestionId = "Q-RISK-1008-1", ClaimId = "CLM-1008", UseCase = RagUseCases.Risk, Language = "uk",
                Text = "Яким є рівень ризику справи відхилень не виявлено бенчмарк норма?",
                ExpectedSourceChunkIdsCsv = "CLM-1008-invoice#0",
                ExpectedAnswerKeywordsCsv = "бенчмарк,відхилень",
                MustNotCiteChunkIdsCsv = "CLM-1010-invoice#1" },

            // ── CLM-1009 (4 questions) ──
            new() { QuestionId = "Q-COVER-1009-1", ClaimId = "CLM-1009", UseCase = RagUseCases.Coverage, Language = "uk",
                Text = "Чи покривається ця справа полісом? Алкогольне спʼяніння виключення перевірка.",
                ExpectedSourceChunkIdsCsv = "CLM-1009-exclusion-check#0",
                ExpectedAnswerKeywordsCsv = "виключен,спʼяніння,алкогол",
                MustNotCiteChunkIdsCsv = "CLM-1006-policy-terms#0" },

            new() { QuestionId = "Q-DENY-1009-1", ClaimId = "CLM-1009", UseCase = RagUseCases.Summary, Language = "uk",
                Text = "Підготуй підсумок відмови у виплаті спʼяніння відмова повідомлення клієнту.",
                ExpectedSourceChunkIdsCsv = "CLM-1009-denial-summary#0",
                ExpectedAnswerKeywordsCsv = "відмова,спʼяніння,виплат",
                MustNotCiteChunkIdsCsv = "CLM-1008-approval-summary#0" },

            new() { QuestionId = "Q-RISK-1009-1", ClaimId = "CLM-1009", UseCase = RagUseCases.Risk, Language = "uk",
                Text = "Яка підстава для відмови медичний висновок алкоголь протокол поліції?",
                ExpectedSourceChunkIdsCsv = "CLM-1009-police#1",
                ExpectedAnswerKeywordsCsv = "медичний,алкогол,спʼяніння",
                MustNotCiteChunkIdsCsv = "CLM-1006-police#0" },

            new() { QuestionId = "Q-MISS-1009-1", ClaimId = "CLM-1009", UseCase = RagUseCases.MissingDocs, Language = "uk",
                Text = "Які документи наявні у справі рахунок СТО поліцейський протокол?",
                ExpectedSourceChunkIdsCsv = "CLM-1009-invoice#0",
                ExpectedAnswerKeywordsCsv = "рахунок,СТО,сума",
                MustNotCiteChunkIdsCsv = "CLM-1011-missing-police#0" },

            // ── CLM-1010 (4 questions) ──
            new() { QuestionId = "Q-RISK-1010-1", ClaimId = "CLM-1010", UseCase = RagUseCases.Risk, Language = "uk",
                Text = "Чому справа має підвищений ризик — перевищення бенчмарку та невідповідність фото рахунку?",
                ExpectedSourceChunkIdsCsv = "CLM-1010-invoice#1,CLM-1010-photo-mismatch#0",
                ExpectedAnswerKeywordsCsv = "перевищення,бенчмарк,невідповідність",
                MustNotCiteChunkIdsCsv = "CLM-1006-invoice#1" },

            new() { QuestionId = "Q-RISK-1010-2", ClaimId = "CLM-1010", UseCase = RagUseCases.Risk, Language = "uk",
                Text = "Яке рішення рекомендується для ризикової справи перевірка людиною призупинення виплати?",
                ExpectedSourceChunkIdsCsv = "CLM-1010-risk-summary#0",
                ExpectedAnswerKeywordsCsv = "аномалії,потребує,перевірки",
                MustNotCiteChunkIdsCsv = "CLM-1008-approval-summary#0" },

            new() { QuestionId = "Q-COVER-1010-1", ClaimId = "CLM-1010", UseCase = RagUseCases.Coverage, Language = "uk",
                Text = "Чи підпадає ДТП на перехресті під покриття поліса Auto Comprehensive виключень не встановлено?",
                ExpectedSourceChunkIdsCsv = "CLM-1010-coverage-check#0",
                ExpectedAnswerKeywordsCsv = "покриття,виключень,перехресті",
                MustNotCiteChunkIdsCsv = "CLM-1009-exclusion-check#0" },

            new() { QuestionId = "Q-SUMM-1010-1", ClaimId = "CLM-1010", UseCase = RagUseCases.Summary, Language = "uk",
                Text = "Зведення доказів справи — рахунок ремонт бампер капот поліцейський звіт Одеса?",
                ExpectedSourceChunkIdsCsv = "CLM-1010-police#0,CLM-1010-invoice#0",
                ExpectedAnswerKeywordsCsv = "зіткнення,бампер,ремонту",
                MustNotCiteChunkIdsCsv = "CLM-1006-police#0" },

            // ── CLM-1011 (4 questions) ──
            new() { QuestionId = "Q-MISS-1011-1", ClaimId = "CLM-1011", UseCase = RagUseCases.MissingDocs, Language = "uk",
                Text = "Яких документів бракує — поліцейський звіт відсутній рукописна розписка не замінює?",
                ExpectedSourceChunkIdsCsv = "CLM-1011-missing-police#0",
                ExpectedAnswerKeywordsCsv = "поліцейський,відсутній,розписка",
                MustNotCiteChunkIdsCsv = "CLM-1007-invoice#0" },

            new() { QuestionId = "Q-MISS-1011-2", ClaimId = "CLM-1011", UseCase = RagUseCases.MissingDocs, Language = "uk",
                Text = "Які документи вимагає поліс для відшкодування ДТП офіційний протокол довідка?",
                ExpectedSourceChunkIdsCsv = "CLM-1011-missing-police#1",
                ExpectedAnswerKeywordsCsv = "поліцейський,протокол,офіційний",
                MustNotCiteChunkIdsCsv = "CLM-1007-missing-docs#0" },

            new() { QuestionId = "Q-RISK-1011-1", ClaimId = "CLM-1011", UseCase = RagUseCases.Risk, Language = "uk",
                Text = "Яким є ризик справи без поліцейського звіту виплата призупинена документування?",
                ExpectedSourceChunkIdsCsv = "CLM-1011-risk-advisory#0",
                ExpectedAnswerKeywordsCsv = "призупинена,документування,поліцейського",
                MustNotCiteChunkIdsCsv = "CLM-1010-risk-summary#0" },

            new() { QuestionId = "Q-SUMM-1011-1", ClaimId = "CLM-1011", UseCase = RagUseCases.Summary, Language = "uk",
                Text = "Підготуй перелік відсутніх документів для рішення — запит поліцейський протокол форма 6?",
                ExpectedSourceChunkIdsCsv = "CLM-1011-missing-docs-summary#0",
                ExpectedAnswerKeywordsCsv = "поліцейський,форма,запит",
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
            Language = "uk",
            SourceVersion = "v0.1",
            EmbeddingModel = embed.ModelName,
            EmbeddingDim = embed.Dimensions,
            EmbeddingJson = EmbeddingCodec.ToJson(vector)
        };
    }

    private static string Hash(string text) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)))[..16];
}
