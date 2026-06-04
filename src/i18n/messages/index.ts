// Aggregated message catalog. Each namespace lives in its own file and exports
// `{ en, uk }` with identical key sets (the `type T = typeof en; const uk: T`
// pattern enforces that at compile time). Add new namespaces here.
import { common } from './common';
import { login } from './login';
import { sidebar } from './sidebar';
import { topbar } from './topbar';
import { dashboard } from './dashboard';
import { claimsList } from './claimsList';
import { claimWorkspace } from './claimWorkspace';
import { aiEvidence } from './aiEvidence';
import { documents } from './documents';
import { risks } from './risks';
import { approval } from './approval';
import { audit } from './audit';
import { policy } from './policy';
import { customerVehicle } from './customerVehicle';
import { customers } from './customers';
import { claimShell } from './claimShell';
import { demo } from './demo';
import { ui } from './ui';
import { rag } from './rag';

export const messages = {
  en: {
    common: common.en,
    login: login.en,
    sidebar: sidebar.en,
    topbar: topbar.en,
    dashboard: dashboard.en,
    claimsList: claimsList.en,
    claimWorkspace: claimWorkspace.en,
    aiEvidence: aiEvidence.en,
    documents: documents.en,
    risks: risks.en,
    approval: approval.en,
    audit: audit.en,
    policy: policy.en,
    customerVehicle: customerVehicle.en,
    customers: customers.en,
    claimShell: claimShell.en,
    demo: demo.en,
    ui: ui.en,
    rag: rag.en,
  },
  uk: {
    common: common.uk,
    login: login.uk,
    sidebar: sidebar.uk,
    topbar: topbar.uk,
    dashboard: dashboard.uk,
    claimsList: claimsList.uk,
    claimWorkspace: claimWorkspace.uk,
    aiEvidence: aiEvidence.uk,
    documents: documents.uk,
    risks: risks.uk,
    approval: approval.uk,
    audit: audit.uk,
    policy: policy.uk,
    customerVehicle: customerVehicle.uk,
    customers: customers.uk,
    claimShell: claimShell.uk,
    demo: demo.uk,
    ui: ui.uk,
    rag: rag.uk,
  },
};

export type Messages = (typeof messages)['en'];
