# Signaled MVP Security Design

**Status:** Approved for MVP  
**Date:** September 26, 2026  
**Purpose:** Define the authentication, authorization, data protection, evidence, audit, integration, and configuration policies for the Signaled MVP.

## 1. Authority and security boundary

This document implements the approved [MVP requirements](../requirements/signaled-mvp-requirements.md), [user flows](../design/signaled-user-flows.md), [architecture](../architecture/signaled-mvp-architecture.md), [data model](../architecture/signaled-mvp-data-model.md), and [API contract](../architecture/signaled-mvp-api-contract.md). It supplies security decisions those documents deliberately left open. The UI mockups guide presentation; their sample people, role labels, record counts, settings, and file types cannot grant access or change these rules.

The MVP is a **single organizational workspace** with one configured Microsoft Entra ID tenant. Department and manager fields are informational, not access boundaries. Adding multi-organization tenancy or department isolation would require a later approved requirement and a new authorization design. No real agency data is required to demonstrate the MVP.

Trust boundaries:

1. A browser session represents a human whose Entra identity is bound to an active Signaled account.
2. An integration token represents a registered service identity with explicitly granted app permissions.
3. The worker runs with its own Azure managed identity and a Signaled service identity for business attribution.
4. Signaled checks current permissions and record scope in Azure SQL before reading or changing protected records.
5. Azure SQL is authoritative; Blob bytes, Service Bus messages, telemetry, and the AI Search index have separate limited access.

## 2. Human authentication and sessions

Use **Microsoft Entra ID** as the human identity provider. The ASP.NET Core web host performs OpenID Connect authorization code sign-in with PKCE and maintains a same-origin, server-controlled session cookie. Browser JavaScript does not store long-lived access tokens. The public API routes used by the web UI share this cookie; external integrations use app-only bearer tokens on their separate `/integrations/*` routes. An Entra sign-in alone never provisions access: Signaled requires a pre-created active `UserAccount` bound to the validated tenant ID and immutable Entra object ID. Email, display name, and department are profile data, not identity keys.

At sign-in, validate issuer/tenant, audience/client, nonce, state, token signature, expiration, and the provider subject. Entra handles passwords, MFA, and account recovery; Signaled does not store passwords. Require MFA through the deployed tenant's identity policy for human users and verify it during deployment. The application does not implement a second password/MFA system. Signaled's own account status and current grants are checked on every protected operation, including an existing session. Disabling an account stops new Signaled actions immediately; sign-out clears the local session. A provider-side account disable and Signaled disable are separate administrative actions, with the local check preventing a still-valid browser cookie from continuing access.

The session cookie is `Secure`, `HttpOnly`, and `SameSite=Lax` for normal app traffic, scoped to the Signaled origin and with no JavaScript access. Use an idle timeout of **30 minutes** and absolute lifetime of **8 hours**; on timeout, require sign-in again. Use a CSRF token on every state-changing browser request, validate Origin/Referer as an additional same-origin check, and keep integration bearer endpoints outside cookie authentication. OpenID Connect callback cookies use the provider/library's required secure SameSite behavior. CORS is not opened for arbitrary origins. Apply HTTPS-only redirection, HSTS in deployment, a restrictive Content Security Policy, `frame-ancestors 'none'`, and `X-Content-Type-Options: nosniff`.

Multiple Container Apps replicas must share ASP.NET Core Data Protection keys: store the key ring in a private Azure Blob container and protect it using a Key Vault key, with access limited to the web host managed identity. Avoid ephemeral per-container keys that would invalidate cookies or antiforgery tokens during scale-out. Local development uses documented development keys and identities, never production material.

## 3. Service authentication and Azure workload identities

Approved external systems obtain a Microsoft Entra **app-only access token** for Signaled's integration API. The API validates signature, issuer/tenant, audience, expiration, and client/application identity, then maps it to an active `ServiceIdentity` by its immutable external client ID. It also checks the required app permission and the Signaled service permission. A valid Azure token with no active, explicitly permitted local service identity is denied. The service identity cannot use browser/human routes, impersonate a user, or read investigation records simply because it can submit an event. No client secret is generated, accepted, displayed, or stored by Signaled's Administration UI. Credential provisioning and revocation in Entra are handled by authorized identity administration; disabling/revoking the Signaled record independently blocks local access and preserves historical actions.

Use distinct **Azure managed identities** for the web/API host and ingestion/indexing worker. Give each only the Azure SQL, Blob, Service Bus, Key Vault, App Configuration, AI Search, or embedding permissions its tasks require. The worker's Azure identity and Signaled business `ServiceIdentity` are linked for attribution but are not the same permission system. The worker consumes Service Bus without a public HTTP endpoint, validates message type/schema, correlation and source operation, and cannot use human permission shortcuts. Do not put Azure connection strings or shared account keys in source control, database settings, service identity records, messages, or telemetry.

## 4. Permission model

The application checks **capability + record scope + lifecycle state**, rather than scattering literal role checks across controllers. Seed the five human roles as collections of the capabilities below. The permission catalog is explicit, versioned with migrations/seed data, and changes to a user's role or direct grant are stored and audited without changing application source. One active primary human role per account in the MVP prevents accidental combination of Administrator/Auditor with investigative powers; the data model can support more later, but the administrative API validates this MVP constraint. The UI may show role permissions and edit access, while the server remains authoritative.

### 4.1 Human capability catalog and default grants

| Capability or group | Contributor | Investigator | Supervisor | Administrator | Auditor |
| --- | :---: | :---: | :---: | :---: | :---: |
| `event.create`, `event.editDraft`, `event.submit`, `event.readOwn` | ✓ |  |  |  |  |
| `event.respondFollowUpOwn`, `evidence.uploadOwnEvent` | ✓ |  |  |  |  |
| `event.requestFollowUp`, `event.triage`, `evidence.uploadTriageEvent` |  | ✓ |  |  |  |
| `event.readSubmitted` for oversight |  | ✓ | ✓ |  | ✓ |
| `case.create`, `case.editAssigned`, `case.manageAssigned`, `case.linkAssigned`, `case.submitReview` |  | ✓ |  |  |  |
| `case.noteAssigned`, `case.findingAssigned`, `evidence.uploadAssignedCase` |  | ✓ |  |  |  |
| `case.readAll`, `evidence.readPermitted` |  | ✓ | ✓ |  | ✓ |
| `case.assign`, `case.manageAll`, `case.review`, `case.reopen` |  |  | ✓ |  |  |
| `summary.readTeam`, `summary.readTrends` |  |  | ✓ |  |  |
| `search.structured` |  | ✓ | ✓ |  | ✓ |
| `search.similar` |  | ✓ |  |  |  |
| `audit.readInvestigation` |  |  |  |  | ✓ |
| `admin.userManage`, `admin.serviceManage`, `admin.settingManage`, `admin.opsRead` |  |  |  | ✓ |  |
| `audit.readAdministration` |  |  |  | ✓ |  |

`event.amendSubmitted`, `event.triageCorrect`, `case.noteCorrect`, `case.findingCorrect`, `evidence.correct`, `evidence.remove`, and `admin.ingestionReplay` are restricted capabilities. They can be granted directly by an authorized Administrator only to the eligible role family: Investigator for investigative corrections and Administrator for ingestion replay. `case.reopen` is part of the Supervisor default because reopening is an approved Supervisor story. Direct grants cannot confer another role's primary capabilities, override record scope, grant service permissions to humans, or modify audit records. `PUT /admin/users/{id}/access` rejects an ineligible grant. Access changes are audited and effective on the next protected operation. Keep at least one active Administrator; reject self-disable or self-removal of the last Administrator role. A user cannot grant a restricted capability to their own account; bootstrap grants such as the first replay operator are provisioned through reviewed deployment seed data.

`evidence.readPermitted` means the caller may open file bytes only when they may read the **parent event or case**. Event evidence visible in a linked case still uses the event's authorization, including for Auditors and Supervisors. The Administrator role alone does not confer investigation/evidence read access. A Contributor's own-event access does not give the Contributor access to an investigation case created from that event.

An Investigator with `event.amendSubmitted` may append a documented correction only for `title`, `description`, `occurredAt`, `location`, or `category` on a submitted event, with a required reason. The original submission remains readable in history. Source identity, creator, submission time, and ingestion key are never editable through this operation. `evidence.uploadTriageEvent` permits an Investigator to attach supporting material to a submitted event during active triage; it does not change the original submitted payload.

Service permissions are a separate catalog: `integration.eventIngest`, `integration.operationReadOwn`, `integration.operationCorrectOwn` for an approved external source; `worker.ingestionProcess` and `worker.searchIndex` for the worker. Each service identity receives only its approved subset. An integration cannot receive human roles or investigator/admin capabilities.

### 4.2 Record scope and lifecycle checks

| Record/action | MVP scope |
| --- | --- |
| Contributor draft/event and follow-up | Own created events, including submitted status and requested follow-up. May change original event only as a draft. May respond to an open request on own submitted event. Cannot list others' events or see the resulting case solely through the originating event. |
| Investigator triage | All **submitted** events in this single workspace, including supporting files and relevant follow-up. Other Contributors' drafts remain private. May record a decision and create a case from an eligible event. |
| Investigator case read/search | All cases in the workspace for historical investigation and similarity research. Notes/evidence content follows the case or linked-event scope. Workload and current owner are visible as needed. |
| Investigator case mutation | Only a case currently assigned to that active Investigator, or the atomic creation/origin-link operation. Can edit synopsis, priority, and due date on an assigned editable case. Cannot edit `AwaitingReview` or `Closed`; returned work resumes under the current eligible assignee. Linking also requires permission to read the event. |
| Supervisor oversight | All submitted events and cases in this workspace for assignment, review, status and trends. May set case priority/due date while editable, assign only active Investigators, make review/closure/reopening decisions, and read relevant evidence. No general Investigator note/finding edits. |
| Auditor review | All submitted events, cases, permitted evidence, and investigation audit history in the workspace, strictly read-only. Other Contributors' drafts are excluded. Audit viewing itself is logged. |
| Administrator | Account, service, settings, safe operational status, and administration audit metadata. No general case/event/evidence body access or global search through this role. The operator may inspect safe ingestion status/correlation without raw incident text. |
| External integration | Its own ingestion requests/operation outcomes; no other source's data or human investigation records. `sourceSystem` must be bound to registered identity. |

Record filters are applied in SQL **before** pagination, counts, dashboards, reports, and structured search. Direct reads, nested file access, and the final semantic-search candidates are checked again against current authoritative grants. Return `404` for an absent or inaccessible record, so existence is not exposed. UI action hiding is guidance only. Server guards also enforce event draft/submitted state, case assignment, review lock, closure, and valid transitions.

For the MVP's single workspace, `case.readAll` is broad by design for Investigator/Supervisor/Auditor; it does not imply a future agency may deploy without department/team restrictions. That future deployment needs a separate policy and query/index change, not an undocumented assumption based on department fields.

## 5. Evidence and file policy

The MVP accepts **PDF, JPEG, PNG, and DOCX** attachments, up to **25 MiB per file**. The allowed set and size are security ceilings, not promises to support every file type in generated mockups. An Administrator may lower the effective maximum or restrict the allowed set through the approved application settings in §8; they cannot raise the ceiling or add a new type without a reviewed deployment/security change. Validate extension, declared MIME type, recognized file signature/container structure, actual byte length, and request limits; reject mismatches or malformed/oversized uploads with a field-safe error. Restrict decompression of archive-like formats; never execute uploaded content or trust the original file name as a storage path.

Files go to a **private Blob container** under an opaque server-generated reference. The API stages bytes, verifies storage and metadata, then marks the SQL evidence record `Available`; failures remain pending/unavailable and are observable. Only `Available` files may be downloaded. The API streams downloads after a fresh parent-record and file authorization check, with `Content-Disposition: attachment`, a safe content type, and `nosniff`. It does not issue direct public Blob URLs, render untrusted Office/PDF content inline, or index file bytes for MVP similarity search. A missing expected Blob marks evidence unavailable and triggers telemetry; it is not represented as valid evidence. A correction or removal preserves metadata, actor, timestamp, and reason; ordinary users cannot physically dispose of evidence/history.

Every evidence upload, metadata correction, removal, and download is recorded with actor, record, timestamp, and correlation ID where present. Do not log file bytes or signed storage access. Follow-up files attach to their response and event. Blob storage encryption at rest and HTTPS transport are required. The MVP does **not** include an automatic malware scanning service; its protective boundary is strict type/size validation, private storage, attachment-only download, and no server rendering. A deployment that requires scanned evidence must add a reviewed scanning-and-quarantine workflow before accepting such data; the MVP does not claim malware clearance.

## 6. Investigative data, search, and privacy

Structured search and operational summaries read scoped Azure SQL data. Azure AI Search stores a **derived** index of approved submitted event narratives and case synopses, stable IDs, type, version, and minimum filter metadata. No evidence bytes, file contents, credentials, user profile data, or full audit narratives are indexed. Use an Azure-hosted embedding deployment for that approved text, under managed identity where supported; select a supported model/region during deployment and document cost and data-region configuration. Any provider outside the approved Azure environment requires a new security decision.

Apply permitted-record filters to candidate retrieval where possible, then recheck every candidate against current Azure SQL permissions before returning identifiers, snippets, counts, or relevance. Do not infer authorization from stale index metadata. Permission/account changes must affect results on the next request even if indexing lags. Search queries and incident narratives are excluded from routine logs/traces. Semantic search is an investigative aid and cannot make a triage, case, assignment, finding, or closure decision. Its failure leaves structured search and case work available.

Encrypt deployed Azure SQL and Blob data at rest and use current supported TLS for all transport. Minimize PII stored for accounts; phone, department, and manager are held only for approved admin context, and their visibility follows admin permission. Escape user-authored text in the UI, validate lengths and schemas server-side, use parameterized EF Core queries, and avoid dynamic SQL assembled from filter input. Consistent errors must not leak unauthorized existence or internal connection details.

## 7. Audit, operational telemetry, and incident handling

Write an `AuditRecord` in the same SQL transaction as each business-significant change: event submission, follow-up, triage/correction, case creation/linking/assignment/status, note/finding revision, review/closure/reopening, evidence action, access/admin/settings change, ingestion processing/replay. Store actor or service identity, UTC time, action, target, safe prior/new values when useful, and correlation. Audit entries are append-only through the application. Use a runtime SQL identity that can insert/select audit records but cannot update/delete them; migrations and privileged maintenance use separate identities. Corrections create new entries. Neither Administrator nor Auditor gets an API to change audit history.

Also audit evidence downloads and Auditor access to audit history in the MVP. Denied sensitive actions produce safe security telemetry and may produce an audit row when a trustworthy actor/target is known. OpenTelemetry/Application Insights/Azure Monitor record structured operational events with correlation across API → outbox → Service Bus → worker → SQL/Blob/search. Do not include passwords, tokens, secrets, full event narratives, note bodies, file content, or unredacted sensitive field changes in logs. Audit is durable business history; telemetry is for diagnosis and may have a different lifecycle.

An authorized Administrator can inspect safe operation status, failure category, timestamps, and correlation IDs. A dead-lettered ingestion is not silently discarded: it is inspected, corrected by its owning integration when the payload was invalid, then replayed by an Administrator holding the restricted `admin.ingestionReplay` capability. Replay records reason/actor, reuses the source idempotency key, and prevents a second event. An invalid request is never displayed as a successfully submitted event. Alert on failed processing, outbox backlog, dead letters, missing evidence Blob, repeated auth denial, and dependency errors; exact alert thresholds and KQL examples belong to operations documentation.

There is no organization-specific legal retention or permanent-deletion workflow in the MVP. Ordinary users cannot delete audit, event, case, or evidence history. Backup, restore, access to privileged database maintenance, and retention duration are set in deployment/operations; do not silently treat the lack of a legal schedule as permission to purge history.

## 8. Configuration ownership and final UI boundary

Only **two application-managed runtime settings** are editable in the MVP Configuration tab:

| Setting key | Default | Allowed change | Validation and effect |
| --- | --- | --- | --- |
| `evidence.maxAttachmentMiB` | `25` | Integer `1`–`25` | Applies to new uploads after committed change. Cannot exceed deployment hard ceiling of 25 MiB; existing evidence is unaffected. |
| `evidence.allowedTypes` | `PDF`, `JPEG`, `PNG`, `DOCX` | Nonempty subset of these four | Applies to new uploads after committed change. A type cannot be added outside the deployment security allowlist; existing evidence remains readable. |

These typed values live in the SQL `AppSetting`/`AppSettingChange` model, with optimistic concurrency, Administrator authorization, validation, audit, and a version/effective timestamp. The application reads the current committed setting for each new upload; it must not rely on a stale in-process cache that permits a now-disallowed type or size. If SQL settings are unavailable, uploads fail safely rather than applying a weaker default. Admin changes cannot alter secrets, identity/session policy, role templates, audit controls, Blob container settings, Azure service endpoints, embedding deployment, or deployment security ceilings.

Secrets and sensitive connection material are deployment-managed in **Azure Key Vault**. Non-secret environment/deployment values may use **Azure App Configuration**; they are not editable through Signaled. The application may show read-only environment, build/version, and dependency health where reliable authorized signals exist, but cannot become an Azure management console. Configuration history shows actor, time, safe old/new values. This section supplies the final approved controls for `administration-configuration.png`; the visual design can now be finished using these two settings and truthful read-only operational information.

## 9. Administrative protections and failure behavior

- The Administrator can create local account/service-identity records bound to already provisioned Entra subjects/client IDs, assign a primary role or eligible direct permission, disable/re-enable access, and review administrative history. Signaled does not create Entra accounts, reset credentials, or display client secrets.
- Self-disable and removal of the last active Administrator are rejected. Changes to another user's grants require an Administrator, produce `AccessChange` plus audit, and take effect without a code deployment. Direct permissions are constrained by §4 and cannot override the role/record-scope boundary.
- Invalid role/permission changes, unauthorized settings, stale ETags, case review locks, and repeated conflicting ingestion keys return stable safe errors from the API contract. Do not claim success after a partial SQL/Blob/Service Bus failure.
- Apply bounded pagination and upload/request limits; API rate limiting protects sign-in-adjacent, ingestion, search, and expensive query paths. Exact thresholds are measured and recorded in testing/deployment so legitimate MVP load remains usable.
- Use separate identities and access policy for CI/CD, migrations, runtime web host, worker, and human operators. Review role assignments and Key Vault access as part of deployment.

## 10. Verification and traceability

| Control | Requirement basis | Minimum acceptance evidence |
| --- | --- | --- |
| Entra authentication and local disable | `REQ-AUTH-001–005`, `REQ-ADM-003–005`, `REQ-SEC-001–005` | Valid/invalid token, wrong tenant/audience, disabled account, revoked service, expired session, CSRF tests. |
| Permission plus record scope | `REQ-AUTH-003–009`, `REQ-SRC-009`, `REQ-RPT-003`, `REQ-UI-002` | Allowed/denied tests for all five roles; direct IDs, list/count, linked events, files, structured and stale-index similarity results. |
| Workflow guards | `REQ-TRI-*`, `REQ-WFL-*`, `REQ-RES-*`, `REQ-REL-*` | No triage before submission, only assigned Investigator edits, review lock, Supervisor decision/reopen, stale ETag conflicts. |
| Evidence handling | `REQ-EVD-*`, `REQ-SEC-006/009` | Type/signature/size mismatch, storage failure, missing Blob, unauthorized download, access audit, removal history. |
| Service ingestion/replay | `REQ-ING-*`, `REQ-ASY-*`, `REQ-OBS-*` | Owning identity, source spoofing, duplicate/conflicting key, correction, retry/dead-letter, controlled replay, correlation. |
| Audit and settings | `REQ-AUD-*`, `REQ-ADM-007`, `REQ-CFG-001–008` | Atomic audit writes, no update/delete path, setting ceiling/type validation, unauthorized change, immediate application, history. |

The implementation must verify these controls in CI and deployed smoke/security tests. Performance goals, retry counts, deployment network restrictions, backup recovery objectives, and operational alert thresholds are owned by testing/deployment documentation. Those values do not change the access or data-protection rules approved here.

## 11. Technology references

- [ASP.NET Core OpenID Connect web authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-oidc-web-authentication) and [Entra web-app sign-in](https://learn.microsoft.com/en-us/entra/msidweb/getting-started/quickstart-webapp) support server-side sign-in with authorization code/PKCE and a cookie session.
- [Microsoft identity platform app-only access](https://learn.microsoft.com/en-us/entra/identity-platform/app-only-access-primer) supports integration identity separation.
- [ASP.NET Core antiforgery](https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery) and [resource-based authorization](https://learn.microsoft.com/en-us/aspnet/core/mvc/security/authorization/resource-based) support the browser and per-record policies.
- [Container Apps managed identities](https://learn.microsoft.com/en-us/azure/container-apps/managed-identity), [shared ASP.NET Core Data Protection keys](https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/implementation/key-storage-providers), and [Azure AI Search vectorizer identity](https://learn.microsoft.com/en-us/azure/search/vector-search-vectorizer-azure-open-ai) support the selected deployment boundaries.
