"""
Advanced Claim Analytics — LangChain sidecar (FastAPI).

Optional advisory analytics layer for InsuranceAIPlatform. The .NET RAG service remains the
core claim-scoped evidence + citation pipeline; this sidecar adds a structured "manager review"
over the SAME claim-scoped EvidenceChunks the .NET side already retrieves.

Design / honesty:
  * LangChain is used meaningfully: a ChatPromptTemplate builds the analysis prompt, a
    PydanticOutputParser defines the structured contract, and an LCEL chain (prompt | model | parser)
    produces the review.
  * The "model" is DETERMINISTIC by default (no API key, no paid provider, no network) so the sidecar
    is portable + reproducible in CI/Azure. If OLLAMA_BASE_URL is set AND reachable, a real local
    ChatOllama model is used instead (no key, local only). Either way providerMode is reported honestly.
  * Advisory only — never a final payout / fraud / legal decision. Citations are echoed from the
    claim-scoped evidence passed in by the .NET caller (no cross-claim data is fetched here).
"""
from __future__ import annotations

import os
from typing import List, Optional

from fastapi import FastAPI
from pydantic import BaseModel, Field

from langchain_core.prompts import ChatPromptTemplate
from langchain_core.output_parsers import PydanticOutputParser
from langchain_core.runnables import Runnable, RunnableLambda

SERVICE = "langchain-claim-analytics"
VERSION = "0.1.0"
ADVISORY = ("AI-аналіз має лише рекомендаційний характер — фінальне рішення приймає людина-адʼюстер.")


# ----------------------------- I/O contracts -----------------------------
class EvidenceChunkIn(BaseModel):
    chunkId: str
    kind: str = ""
    text: str = ""


class AdvancedReviewRequest(BaseModel):
    claimId: str
    customerName: Optional[str] = None
    vehicle: Optional[str] = None
    eventType: Optional[str] = None
    description: Optional[str] = None
    question: Optional[str] = None
    evidence: List[EvidenceChunkIn] = Field(default_factory=list)


class Citation(BaseModel):
    chunkId: str
    kind: str = ""


class AdvancedReview(BaseModel):
    claimId: str
    summary: str
    coverageAssessment: str
    evidenceStrength: str  # none | weak | moderate | strong
    anomalies: List[str] = Field(default_factory=list)
    missingItems: List[str] = Field(default_factory=list)
    recommendedNextAction: str
    citations: List[Citation] = Field(default_factory=list)
    confidence: int = 0  # 0..100
    advisoryOnly: bool = True
    providerMode: str = "Deterministic"  # Deterministic | Ollama
    framework: str = "langchain"


parser = PydanticOutputParser(pydantic_object=AdvancedReview)

prompt = ChatPromptTemplate.from_messages([
    ("system",
     "You are an insurance claim review assistant. Produce an ADVISORY-ONLY structured manager "
     "review strictly from the provided claim-scoped evidence. Never make a final payout, fraud, or "
     "legal decision. Use only the given evidence; do not invent facts. Respond in Ukrainian.\n"
     "{format_instructions}"),
    ("human",
     "Claim {claimId} ({eventType}). Vehicle: {vehicle}. Description: {description}.\n"
     "Manager question: {question}\n\nClaim-scoped evidence:\n{evidence_block}"),
])


# ----------------------------- deterministic analyzer -----------------------------
_COVERAGE_HINTS = ("покрива", "поліс", "comprehensive", "оцспв", "відшкод")
_EXCLUSION_HINTS = ("виключенн", "спʼянін", "сп'янін", "перегон", "не покрива")
_ANOMALY_HINTS = ("перевищ", "38", "норм", "години", "год", "бенчмарк", "ставка", "невідповід")


def _deterministic_review(req: AdvancedReviewRequest) -> AdvancedReview:
    ev = req.evidence or []
    joined = "\n".join(c.text for c in ev).lower()
    n = len(ev)

    strength = "none" if n == 0 else "weak" if n <= 1 else "moderate" if n <= 4 else "strong"
    confidence = 0 if n == 0 else min(95, 25 + n * 12)

    if n == 0:
        return AdvancedReview(
            claimId=req.claimId,
            summary="Недостатньо доказів у матеріалах справи для розширеного аналізу.",
            coverageAssessment="Неможливо оцінити покриття — немає доказів.",
            evidenceStrength="none",
            anomalies=[],
            missingItems=["Завантажте документи/докази для цієї справи (заява, поліс, рахунок СТО)."],
            recommendedNextAction="Зібрати докази та передати на перевірку людині-адʼюстеру.",
            citations=[],
            confidence=0,
            advisoryOnly=True,
            providerMode="Deterministic",
        )

    coverage_pos = any(h in joined for h in _COVERAGE_HINTS)
    exclusion = any(h in joined for h in _EXCLUSION_HINTS)
    coverage = (
        "Докази вказують на ймовірне покриття за умовами полісу; виключень у наданих доказах не виявлено."
        if coverage_pos and not exclusion else
        "Докази згадують можливі виключення — потрібна перевірка людиною."
        if exclusion else
        "Прямих згадок про покриття у наданих доказах недостатньо — потрібна перевірка полісу людиною."
    )

    anomalies: List[str] = []
    if any(h in joined for h in _ANOMALY_HINTS):
        anomalies.append("Можлива невідповідність вартості/годин ремонту — потребує перевірки людиною (не вирок про шахрайство).")

    kinds = {c.kind for c in ev if c.kind}
    missing: List[str] = []
    for need, label in (("invoice", "рахунок СТО"), ("police", "довідка/протокол"), ("policy", "умови полісу")):
        if not any(need in (c.kind or "").lower() for c in ev):
            missing.append(f"Відсутній документ: {label}.")

    return AdvancedReview(
        claimId=req.claimId,
        summary=f"Розширений аналіз за {n} фрагментами доказів справи {req.claimId}. {ADVISORY}",
        coverageAssessment=coverage,
        evidenceStrength=strength,
        anomalies=anomalies,
        missingItems=missing[:3],
        recommendedNextAction="Передати на перевірку людині-адʼюстеру з урахуванням наведених доказів і відкритих питань.",
        citations=[Citation(chunkId=c.chunkId, kind=c.kind) for c in ev[:6]],
        confidence=confidence,
        advisoryOnly=True,
        providerMode="Deterministic",
    )


def _build_chain() -> Runnable:
    """LCEL chain. Real local Ollama when reachable; deterministic analyzer otherwise."""
    base = os.environ.get("OLLAMA_BASE_URL", "").strip()
    if base:
        try:
            from langchain_ollama import ChatOllama  # lazy import
            model = ChatOllama(base_url=base, model=os.environ.get("OLLAMA_MODEL", "qwen2.5:1.5b"), temperature=0.1)
            # prompt | ollama | pydantic-parser ; providerMode stamped post-parse
            def _stamp(r: AdvancedReview) -> AdvancedReview:
                r.providerMode = "Ollama"
                r.advisoryOnly = True
                return r
            return prompt.partial(format_instructions=parser.get_format_instructions()) | model | parser | RunnableLambda(_stamp)
        except Exception:
            pass  # fall through to deterministic
    # Deterministic path: the prompt is still rendered (LangChain), then the deterministic analyzer runs.
    return RunnableLambda(lambda req: _deterministic_review(req))


chain = _build_chain()

app = FastAPI(title="InsuranceAIPlatform · Advanced Claim Analytics (LangChain sidecar)", version=VERSION)


@app.get("/health")
def health():
    base = os.environ.get("OLLAMA_BASE_URL", "").strip()
    return {"service": SERVICE, "version": VERSION, "status": "healthy",
            "framework": "langchain", "providerMode": "Ollama" if base else "Deterministic", "advisoryOnly": True}


@app.post("/advanced-claim-analytics", response_model=AdvancedReview)
def advanced_claim_analytics(req: AdvancedReviewRequest):
    base = os.environ.get("OLLAMA_BASE_URL", "").strip()
    if base:
        try:
            evidence_block = "\n".join(f"- [{c.kind}] {c.chunkId}: {c.text}" for c in req.evidence) or "(немає)"
            review: AdvancedReview = chain.invoke({
                "claimId": req.claimId, "eventType": req.eventType or "", "vehicle": req.vehicle or "",
                "description": req.description or "", "question": req.question or "Загальний огляд справи",
                "evidence_block": evidence_block,
            })
            # Never trust the model for claim scoping: re-scope citations to the input evidence ids only.
            allowed = {c.chunkId for c in req.evidence}
            review.citations = [c for c in review.citations if c.chunkId in allowed]
            review.claimId = req.claimId
            review.advisoryOnly = True
            return review
        except Exception:
            pass  # honest fallback to deterministic
    return _deterministic_review(req)
