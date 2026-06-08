# Advanced Claim Analytics — LangChain sidecar

Optional advisory analytics layer for InsuranceAIPlatform. The .NET RAG service remains the core
claim-scoped evidence + citation pipeline; this sidecar adds a structured **manager review** over the
SAME claim-scoped evidence the .NET side already retrieves.

- **Framework:** FastAPI + LangChain (`ChatPromptTemplate` + `PydanticOutputParser` + LCEL chain).
- **Model:** deterministic by default (no API key, no paid provider, no network) so it is portable +
  reproducible. If `OLLAMA_BASE_URL` is set and reachable, a real local `ChatOllama` model is used
  instead (still no key, local only). `providerMode` is reported honestly either way.
- **Advisory only.** Never a final payout / fraud / legal decision. Citations are echoed and re-scoped
  to the claim-scoped evidence passed in by the .NET caller — no cross-claim data is fetched here.

## Run
```
python -m venv .venv && .venv/Scripts/python -m pip install -r requirements.txt
.venv/Scripts/python -m uvicorn app:app --host 127.0.0.1 --port 8090
```

## Endpoints
- `GET /health` → `{status, framework, providerMode, advisoryOnly}`
- `POST /advanced-claim-analytics` → structured `AdvancedReview` (summary, coverageAssessment,
  evidenceStrength, anomalies[], missingItems[], recommendedNextAction, citations[], confidence,
  advisoryOnly, providerMode, framework)

## Wiring (.NET)
`AdvancedAiReview:Enabled` (default **false**) + `AdvancedAiReview:SidecarBaseUrl`. The .NET endpoint
`POST /api/claims/{claimId}/advanced-ai-review` collects the claim + its claim-scoped EvidenceChunks
and calls this sidecar. Flag off / sidecar unreachable → safe fallback; the core RAG flow is never replaced.
