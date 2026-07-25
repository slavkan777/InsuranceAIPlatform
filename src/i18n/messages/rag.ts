// Claim Evidence Intelligence (RAG) panel messages.
// EN is the product default; UK is the switched locale.
// The `const uk: T` annotation enforces identical keys at compile time.
const en = {
  panelTitle: 'Claim Evidence Intelligence',
  panelSubtitle: 'Ask the AI about this claim\'s documents and evidence',

  // Use-case buttons
  btnCoverage: 'Check policy coverage',
  btnMissingDocs: 'Find missing documents',
  btnRisk: 'Explain risk',
  btnSimilar: 'Find similar claims',
  btnSummary: 'Prepare approval summary',
  btnCustom: 'Ask custom question',

  // Custom question input
  customPlaceholder: 'Type your question…',
  btnAsk: 'Ask',

  // Loading / empty / error states
  stateIdle: 'Select a question type above to ask the AI about this claim.',
  stateLoading: 'Asking the AI…',
  stateError: 'Failed to get an answer:',

  // Advisory banner
  advisoryBanner: 'AI advisory only — human makes the final decision.',

  // Answer card
  answerHeading: 'AI Answer',
  labelConfidence: 'Confidence',
  labelTraceId: 'Trace',
  labelUseCase: 'Use case',
  labelTokens: 'Tokens',
  labelCost: 'Cost (µ¢)',
  labelRetrievalMs: 'Retrieval',
  labelRetrievedChunks: 'Retrieved chunks',
  labelProviderMode: 'Provider',

  // Citations section
  citationsHeading: 'Evidence citations',
  colKind: 'Kind',
  colDocId: 'Document',
  colChunkId: 'Chunk',
  colScore: 'Score',
  colSnippet: 'Snippet',
  noCitations: 'No citations returned.',

  // Similar claims panel
  similarClaimsHeading: 'Similar claims',
  similarClaimsLoadingState: 'Searching for similar claims…',
  similarClaimsEmptyState: 'No similar claims found.',
  similarClaimsErrorState: 'Failed to load similar claims:',
  similarClaimsScoreLabel: 'Similarity',
  similarClaimsReasonLabel: 'Reason',
  similarClaimsCategoriesLabel: 'Matching categories',
  similarClaimsOpenBtn: 'Open claim',

  // Audit history panel
  auditHistoryTitle: 'Audit history',
  auditHistoryLoading: 'Loading audit history…',
  auditHistoryEmpty: 'No audit history for this claim yet.',
  auditHistoryError: 'Failed to load audit history:',
  auditColUseCase: 'Use case',
  auditColQuery: 'Query',
  auditColAnswer: 'Answer',
  auditColConfidence: 'Confidence',
  auditColCreatedAt: 'Date',

  // Infrastructure stack panel
  infraStackTitle: 'Evidence Intelligence Stack',
  infraStackSubtitle: 'Local RAG pipeline diagnostics — advisory only',
  infraLayerSql: 'SQL Source of Truth',
  infraLayerIndex: 'Evidence Memory Index',
  infraLayerRuntime: 'Local Reasoning Runtime',
  infraStatusHealthy: 'healthy',
  infraStatusDegraded: 'degraded',
  infraStatusEmpty: 'empty',
  infraStatusUnavailable: 'unavailable',
  infraStatusDisabled: 'disabled',
  infraFieldPolicyClauses: 'Policy clauses',
  infraFieldEvidenceChunks: 'Evidence chunks',
  infraFieldEvalQuestions: 'Eval questions',
  infraFieldAuditTraces: 'Audit traces',
  infraFieldEmbeddedChunks: 'Embedded',
  infraFieldTotalChunks: 'Total chunks',
  infraFieldEmbeddingModel: 'Embedding model',
  infraFieldDimensions: 'Dimensions',
  infraFieldEnabled: 'Enabled',
  infraFieldModel: 'Model',
  infraFieldEndpoint: 'Endpoint',
  infraRuntimeDisabledNote: 'Local reasoning runtime is disabled — mock inference only. No live or paid model is active.',
  infraLayerVector: 'Vector Runtime (Qdrant)',
  infraFieldBackend: 'Vector backend',
  infraFieldReachable: 'Reachable',
  infraVectorDisabledNote: 'Vector runtime (Qdrant) is disabled or not reachable locally — the in-process index serves vectors as a safe fallback.',
  infraPipelineLabel: 'Pipeline',
  infraPipelineSql: 'SQL',
  infraPipelineIndex: 'Evidence Index',
  infraPipelineRetrieval: 'Retrieval',
  infraPipelineReasoning: 'Reasoning (mock)',
  infraPipelineAudit: 'Audit',
  infraPipelineHuman: 'Human review',
  infraBtnCheck: 'Check',
  infraBtnReindex: 'Reindex',
  infraLoading: 'Checking infrastructure…',
  infraError: 'Failed to load infrastructure status:',
  infraGeneratedAt: 'Generated',
  infraCorrelationId: 'Correlation',
};

export const rag = { en };
