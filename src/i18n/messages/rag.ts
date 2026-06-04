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

type T = typeof en;

const uk: T = {
  panelTitle: 'Аналіз доказів кейсу',
  panelSubtitle: 'Запитайте AI про документи та докази цього кейсу',

  // Use-case buttons
  btnCoverage: 'Перевірити покриття полісу',
  btnMissingDocs: 'Знайти відсутні документи',
  btnRisk: 'Пояснити ризик',
  btnSimilar: 'Знайти схожі кейси',
  btnSummary: 'Підготувати зведення для затвердження',
  btnCustom: 'Запитати вільне питання',

  // Custom question input
  customPlaceholder: 'Введіть ваше питання…',
  btnAsk: 'Запитати',

  // Loading / empty / error states
  stateIdle: 'Оберіть тип питання вище, щоб запитати AI про цей кейс.',
  stateLoading: 'Запитую AI…',
  stateError: 'Не вдалося отримати відповідь:',

  // Advisory banner
  advisoryBanner: 'Тільки порадницький AI — фінальне рішення приймає людина.',

  // Answer card
  answerHeading: 'Відповідь AI',
  labelConfidence: 'Впевненість',
  labelTraceId: 'Trace',
  labelUseCase: 'Тип запиту',
  labelTokens: 'Токени',
  labelCost: 'Вартість (µ¢)',
  labelRetrievalMs: 'Пошук',
  labelRetrievedChunks: 'Отримані чанки',
  labelProviderMode: 'Провайдер',

  // Citations section
  citationsHeading: 'Посилання на докази',
  colKind: 'Тип',
  colDocId: 'Документ',
  colChunkId: 'Чанк',
  colScore: 'Score',
  colSnippet: 'Уривок',
  noCitations: 'Посилань не повернуто.',

  // Similar claims panel
  similarClaimsHeading: 'Схожі кейси',
  similarClaimsLoadingState: 'Пошук схожих кейсів…',
  similarClaimsEmptyState: 'Схожих кейсів не знайдено.',
  similarClaimsErrorState: 'Не вдалося завантажити схожі кейси:',
  similarClaimsScoreLabel: 'Схожість',
  similarClaimsReasonLabel: 'Причина',
  similarClaimsCategoriesLabel: 'Категорії збігів',
  similarClaimsOpenBtn: 'Відкрити кейс',

  // Audit history panel
  auditHistoryTitle: 'Історія аудиту',
  auditHistoryLoading: 'Завантаження історії аудиту…',
  auditHistoryEmpty: 'Для цього кейсу ще немає записів аудиту.',
  auditHistoryError: 'Не вдалося завантажити історію аудиту:',
  auditColUseCase: 'Тип запиту',
  auditColQuery: 'Запит',
  auditColAnswer: 'Відповідь',
  auditColConfidence: 'Впевненість',
  auditColCreatedAt: 'Дата',

  // Infrastructure stack panel
  infraStackTitle: 'Стек аналізу доказів',
  infraStackSubtitle: 'Діагностика локального RAG-пайплайну — тільки порадницька',
  infraLayerSql: 'SQL-джерело правди',
  infraLayerIndex: 'Індекс памʼяті доказів',
  infraLayerRuntime: 'Локальний рантайм міркувань',
  infraStatusHealthy: 'healthy',
  infraStatusDegraded: 'degraded',
  infraStatusEmpty: 'empty',
  infraStatusUnavailable: 'unavailable',
  infraStatusDisabled: 'disabled',
  infraFieldPolicyClauses: 'Пункти полісу',
  infraFieldEvidenceChunks: 'Чанки доказів',
  infraFieldEvalQuestions: 'Питання оцінки',
  infraFieldAuditTraces: 'Аудит-записи',
  infraFieldEmbeddedChunks: 'Проіндексовано',
  infraFieldTotalChunks: 'Всього чанків',
  infraFieldEmbeddingModel: 'Модель ембедингу',
  infraFieldDimensions: 'Розмірність',
  infraFieldEnabled: 'Увімкнено',
  infraFieldModel: 'Модель',
  infraFieldEndpoint: 'Endpoint',
  infraRuntimeDisabledNote: 'Локальний рантайм міркувань вимкнено — тільки mock-інференс. Жодна жива або платна модель не активна.',
  infraLayerVector: 'Векторний рантайм (Qdrant)',
  infraFieldBackend: 'Векторний бекенд',
  infraFieldReachable: 'Доступний',
  infraVectorDisabledNote: 'Векторний рантайм (Qdrant) вимкнено або недоступний локально — вектори обслуговує локальний індекс як безпечний фолбек.',
  infraPipelineLabel: 'Пайплайн',
  infraPipelineSql: 'SQL',
  infraPipelineIndex: 'Індекс доказів',
  infraPipelineRetrieval: 'Пошук',
  infraPipelineReasoning: 'Міркування (mock)',
  infraPipelineAudit: 'Аудит',
  infraPipelineHuman: 'Перевірка людиною',
  infraBtnCheck: 'Перевірити',
  infraBtnReindex: 'Переіндексувати',
  infraLoading: 'Перевірка інфраструктури…',
  infraError: 'Не вдалося завантажити статус інфраструктури:',
  infraGeneratedAt: 'Згенеровано',
  infraCorrelationId: 'Кореляція',
};

export const rag = { en, uk };
