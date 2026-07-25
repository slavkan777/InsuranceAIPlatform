// Aggregated message catalog. The product is English-only at runtime: each
// namespace file exports `{ en }` and this module resolves to English only.
// Add new namespaces here.
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
};

export type Messages = (typeof messages)['en'];
