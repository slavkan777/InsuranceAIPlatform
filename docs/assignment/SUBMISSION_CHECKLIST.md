# Submission Checklist

## Repository

- [x] Public GitHub repository available.
- [x] README updated for assignment review.
- [x] Quick setup instructions included.
- [x] Mock mode documented.
- [x] Backend mode documented.
- [x] `.env.example` added.

## Assignment mapping

- [x] Assignment option explicitly stated.
- [x] Domain adaptation explained.
- [x] RAG / chat-with-docs use case described.
- [x] Example user questions listed.

## Core functionality

- [x] Frontend claim workspace available.
- [x] Synthetic golden claim available.
- [x] API facade supports mock/backend modes.
- [x] Mock RAG answer contract available.
- [x] Citations represented in the DTO contract.
- [x] Confidence/cost/retrieval trace represented in the DTO contract.
- [x] Audit trail represented in the DTO contract.

## Engineering documentation

- [x] Architecture overview documented.
- [x] RAG decisions documented.
- [x] Productionization plan documented.
- [x] AI-assisted development workflow documented.
- [x] Limitations documented.

## Safety and guardrails

- [x] AI is advisory-only.
- [x] Human decision remains final.
- [x] No autonomous payout.
- [x] No autonomous rejection.
- [x] No final fraud accusation by AI.
- [x] No real PII.
- [x] No committed secrets.

## Recommended final checks before sending

Run locally:

```bash
npm install
npm run build
npm run dev
```

Optional:

```bash
npm run lint
npm run test:e2e
```

Manual reviewer flow:

1. Open dashboard.
2. Open claims list.
3. Open `CLM-1006`.
4. Open documents/evidence page.
5. Open AI/evidence/RAG-related panels.
6. Check human approval page.
7. Check audit/cost page.
8. Verify that the README does not overclaim production completeness.

## Still recommended before final delivery

- [ ] Add screenshots to `docs/screenshots/`.
- [ ] Record a short demo video.
- [ ] Confirm whether a local .NET backend implementation is included in the final review bundle.
- [ ] If backend is included, add exact `dotnet run` instructions and solution/project paths.
- [ ] If backend is not included, state that backend mode is contract-ready and mock mode is the reviewer path.
- [ ] Run `npm run build` after the documentation changes.
