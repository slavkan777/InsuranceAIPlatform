# Frontend Interview Answer Pack

Short, honest, ready-to-say answers.

**1. Why Redux Toolkit?**
Many routes share UI/domain-view state — selected claim, queue filters, approval draft, demo progress. RTK gives typed slices, predictable reducers, devtools, and a selector layer. It's the standard, low-boilerplate Redux.

**2. Why Redux-Saga?**
For multi-step / cancellable workflows and the side-effect seam: the AI run, document request, approval draft/send, and the guided demo. Sagas read like workflow event flows and are easy to cancel/retry. Trivial toggles stay in reducers.

**3. Why not just local component state?**
Local state is fine for ephemeral UI, and I use it there. But cross-route shared state and orchestrated workflows would turn into prop-drilling and effect spaghetti — Redux + Saga keep them explicit and testable.

**4. Why not RTK Query yet?**
There's no server. RTK Query shines for server cache; today everything is a local mock. Adding it now would be caching nothing.

**5. Where would RTK Query / TanStack Query fit later?**
The moment the .NET backend exists: claim lists, claim details, AI results become server cache → move them to RTK Query/TanStack Query, leaving slices for true client state (filters, selections, drafts, demo).

**6. How would this connect to a .NET backend?**
Through one seam: `mockInsuranceApi`. Its functions already have the async signatures the UI/sagas call. Swap the implementation for a typed `.NET` client (same shapes) and the UI doesn't change. Endpoint mapping is documented in the backend-contract readiness doc.

**7. What's mocked now?**
Everything server/AI: synthetic claim data, a mocked AI "run" (a saga delay with stepped progress), and local write acknowledgements. No network, no provider, no keys.

**8. How do you prevent AI from making final claim decisions?**
By design: `AiRecommendation.advisoryOnly: true`, a `HumanReviewRequirement` gate, explicit "auto-approval: NO" governance panels, and approval being a human-controlled draft. There is no autonomous payout/rejection path in the code.

**9. Where is audit/cost governance represented?**
A dedicated Audit & Cost page (run id, trace id, model, tokens, cost, latency, audit trail, governance box) plus a dashboard "audit-today" panel. Telemetry is treated as a first-class product feature.

**10. What would you improve next?**
Open the .NET backend gate behind the mock-API seam, migrate remaining pages to selectors + the API, add RTK Query for server cache, then real (cheap) AI + RAG + guardrails — each as its own approved gate. Tests (Vitest + RTL) come in alongside the first commit.
