"""Detection-quality tests for the deterministic advanced-review analyzer.

These exist because the coverage / exclusion / anomaly detection is driven by
keyword hints. When the RAG corpus was translated to English (Lease 5) the
Ukrainian hints would have stopped matching and detection would have degraded
*silently* — the endpoint would still answer 200 with plausible prose.

Each test feeds evidence phrased the way the translated corpus phrases it and
asserts the analyzer actually reacts. Run with:  python -m pytest test_app.py
"""
import app as sidecar
from app import AdvancedReviewRequest, EvidenceChunkIn


def _req(texts, kinds=None, claim_id="CLM-1006"):
    kinds = kinds or ["evidence"] * len(texts)
    return AdvancedReviewRequest(
        claimId=claim_id,
        eventType="RoadAccident",
        vehicle="Toyota Camry 2021",
        description="Collision at an intersection.",
        question="Review the claim.",
        evidence=[
            EvidenceChunkIn(chunkId=f"{claim_id}-c{i}", kind=k, text=t)
            for i, (t, k) in enumerate(zip(texts, kinds))
        ],
    )


# --------------------------- hint sets are English ---------------------------

def test_hint_sets_contain_no_cyrillic():
    for name in ("_COVERAGE_HINTS", "_EXCLUSION_HINTS", "_ANOMALY_HINTS"):
        for hint in getattr(sidecar, name):
            assert not any("Ѐ" <= ch <= "ӿ" for ch in hint), f"{name} still has Cyrillic: {hint}"


# --------------------------- coverage detection ------------------------------

def test_positive_coverage_is_detected_from_english_corpus_wording():
    r = sidecar._deterministic_review(_req([
        # Coverage wording only. An "Exclusion check: ..." fragment must NOT be added
        # here: the analyzer keys on the word "exclusion" itself, so such a chunk
        # legitimately routes to the exclusion branch even when it concludes that no
        # exclusion applies. That behaviour is unchanged from the Ukrainian hints
        # (which matched "виключенн" the same way) and is asserted separately by
        # test_exclusion_wording_triggers_human_review.
        "Policy Auto Comprehensive POL-2025-AC-4421 covers losses from a road traffic accident. "
        "A deductible of 500 dollars applies to the insured event.",
        "The insured event is covered; the deductible of 500 dollars applies.",
    ]))
    assert "likely coverage" in r.coverageAssessment
    assert r.advisoryOnly is True


def test_no_coverage_reference_is_reported_as_insufficient():
    r = sidecar._deterministic_review(_req([
        "Photo of the front bumper: visible cracks and deformation.",
    ]))
    assert "not enough direct references" in r.coverageAssessment


# --------------------------- exclusion detection -----------------------------

def test_exclusion_wording_triggers_human_review():
    r = sidecar._deterministic_review(_req([
        "Exclusion check: exclusion CLA-AC-EXCL-001 — losses caused while driving under "
        "alcohol intoxication are not covered.",
    ]))
    assert "possible exclusions" in r.coverageAssessment


# --------------------------- anomaly detection -------------------------------

def test_benchmark_excess_is_flagged_as_anomaly():
    r = sidecar._deterministic_review(_req([
        "The repair estimate of 2720 dollars exceeds the average benchmark of 1970 dollars by 38 percent.",
    ]))
    assert r.anomalies, "benchmark excess must raise an anomaly"
    assert "fraud" in r.anomalies[0].lower(), "anomaly must stay advisory, not a fraud verdict"


def test_photo_invoice_mismatch_is_flagged_as_anomaly():
    r = sidecar._deterministic_review(_req([
        "Mismatch between the photo and the invoice: the invoice bills for bumper replacement, "
        "yet the photo shows only scratches.",
    ]))
    assert r.anomalies


def test_clean_evidence_raises_no_anomaly():
    r = sidecar._deterministic_review(_req([
        "Customer statement: the document package is complete.",
    ]))
    assert r.anomalies == []


# --------------------------- missing documents -------------------------------

def test_missing_documents_are_listed_in_english():
    r = sidecar._deterministic_review(_req(["Customer statement only."], kinds=["application"]))
    joined = " ".join(r.missingItems)
    assert "repair invoice" in joined and "police report" in joined
    assert not any("Ѐ" <= ch <= "ӿ" for ch in joined)


def test_present_document_kind_is_not_reported_missing():
    r = sidecar._deterministic_review(
        _req(["Repair invoice: total 2720 dollars.", "Police report: collision confirmed."],
             kinds=["invoice", "police"])
    )
    joined = " ".join(r.missingItems)
    assert "repair invoice" not in joined
    assert "police report" not in joined


# --------------------------- safety / scoping --------------------------------

def test_empty_evidence_is_safe_and_english():
    r = sidecar._deterministic_review(_req([]))
    assert r.evidenceStrength == "none"
    assert r.confidence == 0
    assert r.citations == []
    assert r.advisoryOnly is True
    assert not any("Ѐ" <= ch <= "ӿ" for ch in r.summary + r.coverageAssessment)


def test_citations_stay_scoped_to_the_requested_claim():
    r = sidecar._deterministic_review(_req(["Repair invoice: total 2720 dollars."], claim_id="CLM-1009"))
    assert r.claimId == "CLM-1009"
    assert all(c.chunkId.startswith("CLM-1009") for c in r.citations)


def test_response_text_has_no_cyrillic():
    r = sidecar._deterministic_review(_req([
        "Policy Auto Comprehensive covers losses; a deductible of 500 dollars applies.",
        "The repair estimate exceeds the benchmark by 38 percent.",
    ]))
    blob = " ".join([r.summary, r.coverageAssessment, r.recommendedNextAction, *r.anomalies, *r.missingItems])
    assert not any("Ѐ" <= ch <= "ӿ" for ch in blob)


# --------------------------- prompt-language contract ------------------------
# Regression guard for the Lease 6 closure finding: the LangChain system prompt
# used to instruct "Respond in Ukrainian", which silently produced Ukrainian
# output on the Ollama path even though every other surface was English.

def _system_prompt_text() -> str:
    parts = []
    for msg in sidecar.prompt.messages:
        tmpl = getattr(msg, "prompt", None)
        if tmpl is not None and hasattr(tmpl, "template"):
            parts.append(str(tmpl.template))
    return "\n".join(parts)


def test_active_prompt_requires_english_output():
    text = _system_prompt_text()
    assert "Respond in English" in text, "system prompt must explicitly require English output"


def test_no_active_prompt_asks_for_ukrainian():
    text = _system_prompt_text().lower()
    for banned in ("ukrainian", "українськ", "uk-ua", "respond in uk"):
        assert banned not in text, f"prompt must not request Ukrainian output: found {banned!r}"


def test_prompt_contains_no_cyrillic():
    assert not any("Ѐ" <= ch <= "ӿ" for ch in _system_prompt_text())


def test_advisory_only_contract_survives_the_language_change():
    text = _system_prompt_text().lower()
    assert "advisory-only" in text or "advisory only" in text
    assert "never make a final payout" in text
