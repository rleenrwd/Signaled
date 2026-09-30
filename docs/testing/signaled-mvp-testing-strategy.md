# Signaled MVP Testing Strategy

**Status:** Approved for MVP  
**Date:** September 30, 2026  
**Purpose:** Define verification coverage, test conditions, measurable quality targets, and release acceptance for the approved Signaled MVP.

## 1. Authority and meaning of approval

This strategy implements the [MVP requirements](../requirements/signaled-mvp-requirements.md), [user stories](../requirements/signaled-user-stories.md), [user flows](../design/signaled-user-flows.md), [architecture](../architecture/signaled-mvp-architecture.md), [data model](../architecture/signaled-mvp-data-model.md), [API contract](../architecture/signaled-mvp-api-contract.md), and [security design](../security/signaled-mvp-security-design.md). Approved images in `../design/mockups/` guide visual checks; their example counts, people, statuses, fields, and file types do not change business rules.

**Approved for MVP means the testing plan is approved. It does not mean tests have been implemented, executed, or passed, or that the application is ready for release.** Release requires the evidence and gates in this document. Test results must identify the exact commit and environment tested.

The strategy covers the modular monolith, browser interface, HTTP API, SQL persistence, private Blob evidence, Service Bus/outbox/inbox processing, separate worker, structured and semantic search, administration, and audit. It verifies the single-workspace permission model already approved in security design. It does not add departments as access boundaries or create additional product capabilities.

Deployment and operations documentation owns provisioning, environment topology, deployed rate limits, retry counts/backoff, message locks, idempotency retention, backup/restore objectives, alert thresholds, and release/rollback procedures. Tests verify those approved values once configured; this document defines verification and performance acceptance, not a second operational policy.

## 2. Test layers and tools

Use xUnit for C# automated suites and the normal .NET test runner in CI. Use an ASP.NET Core test host for HTTP integration/functional tests and Playwright for a small browser acceptance suite. These are development tools, not new runtime services. Keep test projects aligned with the solution's Domain, Application, Infrastructure, and host boundaries without requiring one test project per business module.

| Layer | Verifies | Required boundary |
| --- | --- | --- |
| Unit | Domain transitions, validation, permission eligibility, completeness checks, retry classification, safe mappings | Fast, isolated; no network. Doubles are appropriate for external interfaces. |
| Integration | EF Core mappings, SQL constraints/transactions, authorization queries, outbox/inbox, storage adapter behavior | Disposable SQL Server using production mappings/migrations. Provider behavior cannot be proven with EF InMemory or SQLite. |
| Functional/API | HTTP contract, middleware, policies, command orchestration, audit, complete lifecycle | Real application host and SQL; controlled external doubles where useful. Assert state and history as well as response status. |
| Browser | Primary user journeys, status/validation feedback, navigation, locked/read-only behavior | Actual frontend against the application; separate role sessions. No API-only substitute for the human interface check. |
| Security | Allowed/denied access, identity/session/CSRF boundaries, file handling, privacy | Automated negative cases plus deployed checks for provider and Azure configuration. |
| Performance/reliability | Response targets, ingestion lag, scale-out, duplicates, crashes and outages | Repeatable workload and failure injection; real deployed dependencies for acceptance measurements. |
| Manual/deployment | Visual design, keyboard usability, cloud identities/encryption, recovery and monitoring | Recorded checklist and evidence. Manual checks supplement the required automated suites. |

Real Azure Blob, Service Bus, AI Search/embedding, Entra, Key Vault, App Configuration, and telemetry checks run against isolated nonproduction resources before MVP release. Local substitutes or doubles are useful for deterministic failures but cannot establish deployed IAM, broker settlement/dead-letter behavior, or provider connectivity. Tests never run destructive failure injection against the public MVP environment.

## 3. Environments, identities, and fixtures

- Local and ordinary CI suites use disposable databases, independent test namespaces, and synthetic records. Seed the five approved roles and capability catalog from the same versioned definitions as the application.
- Azure verification uses an isolated test environment configured by deployment documentation. Give test runners only the permissions needed to create fixtures and inspect results. Separate fixture/migration credentials from the runtime identity.
- Use at least two Contributors and two Investigators, one Supervisor, one Administrator, one Auditor, and two external source identities. Include disabled/unregistered principals and a worker identity. Restricted-capability tests use separate eligible accounts with and without explicit grants.
- Include own and other-user drafts, submitted/follow-up/triaged events, assigned and unassigned cases, every approved case state, multiple review cycles, corrections, removed/unavailable evidence, and stale index documents.
- Use known synthetic PDF, JPEG, PNG, and DOCX files plus malformed, mismatched, empty, oversized, unsafe-name, and excessive-decompression fixtures. No live malware, real agency narratives, personal credentials, or production evidence is required.
- Use controlled clocks for timeouts, date boundaries, ordering, due/overdue calculations, and retry schedules. Use UTC persistence and explicit offsets in input fixtures. A fixed seed and manifest make failures reproducible.
- Each run isolates its SQL, Blob, queue, index, and browser state. Clean up only resources created by that run. Cleanup never exercises an ordinary-user permanent-deletion feature.

## 4. Required functional coverage

The scenario IDs below are stable acceptance references. Implement their meaningful paths and negative cases as multiple tests where appropriate; one passing test must not stand in for an entire group.

| ID | Scenario and minimum assertions | Primary methods |
| --- | --- | --- |
| TS-01 | Contributor creates an incomplete draft, saves/edits it, uploads a valid file, corrects validation, and submits. Stable ID/creator/time; draft excluded from others' queues; submission snapshot retained; normal submitted edits denied. | Unit, API, browser |
| TS-02 | Investigator requests follow-up; owning Contributor sees it and adds a response/files. Preserve original event, requestor/responder/time and response association; deny other Contributor, closed-request response, and premature triage with blocking follow-up. Partial storage failure cannot produce a completed response. | API, integration, browser |
| TS-03 | Investigator records rationale for both NoCase and OpenCase. NoCase retains a searchable event without a case. OpenCase produces EscalationPending until separate case creation succeeds. Reject missing rationale, private draft, repeated incompatible action; authorized corrections retain earlier decisions. | Unit, API |
| TS-04 | Create case from eligible triage; case, Origin link, status, audit commit together. Duplicate retry cannot create another case for the same decision. Link/unlink/relink related events, reject duplicate active links and origin unlink; retain association history and both records. | API, SQL integration |
| TS-05 | Supervisor assigns/reassigns active Investigator, closes prior assignment interval, and updates current owner/workload. Deny inactive/non-Investigator assignment; prior assignee cannot continue editing after reassignment; history identifies actor and time. | Unit, API, integration |
| TS-06 | Assigned Investigator manages permitted priority/due date/synopsis, notes, evidence metadata, and findings. Enforce transition rules and field permissions. Corrections/removal require eligible restricted capability/reason; retain revisions and tombstones. Nonassigned Investigator may read but cannot mutate. | Unit, API, integration |
| TS-07 | Readiness identifies missing required investigation information; incomplete submission is rejected without status/audit success. Complete case enters AwaitingReview and blocks review-controlled edits, including nested notes/findings/files/links and management fields. | Unit, API, browser |
| TS-08 | Supervisor reviews supporting records, returns with reason, Investigator resumes and resubmits, Supervisor closes with resolution. Reopen with reason preserves prior closure/findings/revisions and selects InProgress or AwaitingAssignment according to active eligible assignee. Reject invalid states, wrong role, or missing explanation. | Unit, API, browser |
| TS-09 | Scoped event/case lists and structured search support approved filters, stable pagination, identifiers and date boundaries. Similarity returns source/relevance only for currently permitted records; test stale metadata, forbidden seed, denied candidates, authorized zero results and dependency outage. No search operation makes a business decision. | API, integration, Azure verification |
| TS-10 | Supervisor summaries/workload/trends reconcile against a known SQL fixture by status, priority, owner and period. Counts and drill-down share scope; high-priority/overdue information is distinguishable; no automated cause/compliance conclusion. | API, browser/manual |
| TS-11 | Auditor reconstructs assignments, links, corrections, review cycles, closure and reopening; searches historical activity, reads permitted evidence and identifies missing information. Every mutation is denied. Audit-history access itself is recorded as required by security design. | API, browser/manual |
| TS-12 | Administrator manages provider-bound local accounts/service records and eligible access; disable/enable and grant changes apply on next protected operation without changing historical actors. Reject extra primary roles, ineligible or self restricted grants, self-disable/last-admin removal, human grants to services, secret fields, and provider-subject changes. | Unit, API, integration |
| TS-13 | Administrator changes only the two evidence settings. Validate integer 1–25 MiB and nonempty PDF/JPEG/PNG/DOCX subset; reject unknown keys, new types, secrets and stale versions. Immediate committed policy affects new uploads across replicas; old files remain readable under current record scope. History records actor/time/safe old/new values. | Unit, API, browser, integration |
| TS-14 | External source receives durable Accepted operation, owning-source polling and eventual submitted event with source/time. Test duplicate and conflicting keys, malformed envelope versus deeper validation failure, revoked/source-spoofed identity, correction then controlled replay, and no investigative access through service credentials. | API, integration, worker/Azure tests |

Review readiness tests must use the approved business fields and application rule set; generated mockup checklists cannot create a universal attachment count or mandatory note. Every required-field/completeness rule must be recorded in the API/business-rule baseline before that rule is implemented, with a passing complete example and a failing example for each missing item. This strategy does not silently settle a product rule through a test fixture.

## 5. Authorization and security acceptance

Build parameterized API tests from security design's complete capability matrix. For **each mutating route**, test permitted principal, wrong capability, inaccessible record where applicable, current account/service state, lifecycle guards, and valid/invalid concurrency preconditions. For read routes, verify direct/nested reads, lists, pagination, counts, summaries, search and file bytes. UI hiding alone is insufficient.

| Principal | Essential positive and negative cases |
| --- | --- |
| Contributor | Own draft/submitted event and requested follow-up allowed; another Contributor's record, triage, case investigation, resulting case solely via origin event, administration and audit denied. |
| Investigator | Submitted events and case research allowed; own assigned investigation edits allowed; others' drafts, unassigned case edits, Supervisor disposition, admin work and unauthorized restricted corrections denied. |
| Supervisor | Assignment, oversight, review and reopen allowed; ordinary Investigator note/finding mutation, other users' drafts and admin work denied. |
| Administrator | Account/service/settings, safe operation metadata and administration audit allowed; general incident narratives, evidence bytes, global investigative search and investigation audit denied. |
| Auditor | Submitted-event/case/history/evidence read and structured search allowed; all investigation mutations, private drafts, admin changes and similarity search denied. |
| Integration/worker | Only assigned service capabilities and source/operation scope allowed; human routes, cross-source operation access, source spoofing, and unregistered/revoked identity denied. |

Verify `401` for unauthenticated API access, `403` for capability denial, and an indistinguishable `404` for absent versus inaccessible direct records. Check response bodies, snippets, counts, relations, readiness data and error details for disclosures, not only status codes.

Identity/session tests cover wrong tenant/audience, invalid signature/expiry, unregistered subject/client, OIDC state/nonce rejection, sign-out, 30-minute idle and 8-hour absolute expiry, and disabled access with an already-valid session/token. Deterministic test authentication is allowed only in test hosts; deployed verification must exercise actual Entra sign-in, tenant MFA policy and app-only tokens. Confirm the test bypass cannot activate in the deployed MVP.

Browser security checks cover CSRF missing/invalid tokens on mutations, unwanted cross-origin requests, cookie flags, HTTPS/HSTS, CSP/frame restrictions, output escaping of authored text, safe file disposition/`nosniff`, and sensitive-data-free errors. Verify authentication survives replica changes using shared protected Data Protection keys. Scan committed files/build output for secrets and review dependency/security scan findings; no release with an unresolved exploitable critical/high issue on the shipped path.

## 6. Evidence, persistence, and historical integrity

- Test each allowed file type with matching extension, MIME, signature/container and actual length. Verify the exact configured byte limit and one byte over it, including the 25 MiB ceiling (26,214,400 bytes). Test fractional/out-of-range settings and changed policy during subsequent requests; SQL setting failure cannot fall back to weaker upload rules.
- Reject malformed DOCX containers, excessive decompression, type mismatches, path-like names and unsupported files. Confirm private Blob access, opaque storage references, no exposed signed/public URL, and no inline rendering or malware-clearance claim.
- Inject Blob-write, SQL-finalization and multi-file follow-up failure. Assert no Available evidence/completed response or false success; staged leftovers are observable and eligible for the operational cleanup procedure. Missing expected Blob produces Unavailable metadata/safe feedback and telemetry.
- Check parent authorization for metadata and every download, including linked-event files shown in a case, removed files and permissions changed after the list was loaded. Upload/download/correction/removal produces the required audit history.
- Exercise production SQL constraints: stable/unique identifiers, restricted historical FKs, actor/parent/target rules, revision uniqueness, one active case/event pair and one open assignment. Prevent cross-parent revision pointers and response/file association mismatch.
- Fail the audit insert, outbox insert, assignment interval or origin-link step and confirm the whole related SQL command rolls back. Business projections, revisions, decisions, receipts and audit must agree after commit and restart.
- Race two edits with the same ETag: one succeeds and the other gets `412`; missing If-Match gets `428`; invalid lifecycle action gets `409`. Child actions advance the correct parent version. Client retries with equal Idempotency-Key/body return the original result; changed body conflicts without another mutation.
- Confirm no runtime role/API can update or delete audit/history. Use the actual runtime SQL identity to verify audit INSERT/SELECT succeeds and UPDATE/DELETE fails. Disabled accounts retain historical attribution. Safe audit fields and routine telemetry exclude secrets, file bytes and prohibited narrative content.
- Apply migrations to empty and previous supported schemas, preserve representative history, verify seeded grants/settings, and run a smoke lifecycle after upgrade. Migration failure and restoration/rollback are exercised under operations' procedure.

## 7. Asynchronous processing and search reliability

Use deterministic automated tests for permanent/transient classification, retry scheduling and configured exhaustion. Before release, also use real Service Bus to verify broker delivery, settlement, lock expiry/redelivery and actual dead-letter placement. Assert operation state, SQL event/audit/outbox/inbox and broker outcome together.

Required failure cases:

1. SQL acceptance commits while Service Bus is unavailable: operation/outbox remains durable; dispatcher later publishes without losing accepted work.
2. Broker accepts publication but publisher crashes before marking dispatch complete: resend yields exactly one business event.
3. Worker crashes before commit or after commit but before message completion: redelivery is safe; receipt/result/audit agree and original source key prevents a second event even with a new message ID.
4. Transient dependency failure retries within the operational policy; permanent payload/source/schema failure does not loop indefinitely. Exhaustion produces dead-lettered work and a safe inspectable outcome, not silent disappearance.
5. Only the original active source can correct its failed payload. Only an Administrator with restricted replay permission can replay with reason/current version. Preserve original payload/hash and correction history; replay cannot bypass revoked access or create duplicates.
6. Invalid/unknown message type, missing operation, stale correction and conflicting source key are rejected safely. Worker startup/restart and concurrent consumers retain attribution and idempotency.
7. Index updates derive from committed approved text only. Out-of-order/redelivered indexing work cannot permanently overwrite a newer document; failures remain retryable/observable and the index can be rebuilt from SQL.
8. AI Search/embedding outage returns safe unavailable feedback only for similarity; structured search and core workflows remain available. Stale index data cannot bypass current SQL authorization or expose denied counts/snippets.

Use a small fixed similarity evaluation set with at least 10 synthetic query/incident pairs that have intentionally different wording. For at least 8 queries, the designated relevant permitted record must appear in the top five results. Record corpus, queries, expected records, model/index configuration and actual relevance results. This is a demonstration acceptance target, not a claim of statistical accuracy or automated investigative judgment. Authorization leakage is a zero-tolerance failure independent of relevance quality.

## 8. Measurable MVP performance targets

These are initial **acceptance targets**, not measured results or an availability SLA. Meet them on the documented deployed test configuration; record any proposed adjustment and obtain a baseline decision before release instead of quietly changing the test.

| Measurement | Target under healthy dependencies |
| --- | --- |
| Common scoped detail/list/structured-search/overview API reads | p95 ≤ 2 seconds |
| Normal non-file interactive writes and durable ingestion acceptance | p95 ≤ 3 seconds |
| Similarity response, including embedding and SQL authorization checks | p95 ≤ 5 seconds |
| Accepted ingestion to committed submitted event | p95 ≤ 60 seconds; every valid operation ≤ 120 seconds during measured run |
| Unexpected failed requests/operations in healthy workload | ≤ 1%; zero lost accepted operations, duplicate business outcomes or integrity/privacy failures |

Measure client-observed API latency from request send to full response receipt over HTTPS, excluding the interactive Entra login sequence. Ingestion time is server acceptedAt to committed completion timestamp; polling delay is measured separately. Semantic search timing includes remote providers. Valid files are excluded from the non-file latency targets because transfer time depends on bytes/network; verify a 25 MiB file completes safely under recorded request timeout, limits and storage configuration.

Reproducible baseline workload:

- Seed 1,000 events and 200 cases with distributed statuses/owners/dates, at least 2,000 audit entries, and valid evidence metadata; document index population and fixture manifest.
- Run 10 concurrent authenticated clients for 10 measured minutes after 2 minutes of warm-up. Use a recorded mix of 70% approved reads/structured search, 20% non-file writes and 10% ingestion acceptance. Each client uses independent eligible records and refreshed versions so conflicts are not accidental load-test errors.
- Collect at least 100 observations for each API operation class evaluated and at least 100 completed valid ingestions; extend duration if needed. Run at least 100 similarity queries separately at two concurrent callers. Report sample size, p50/p95/max and expected versus unexpected failures for each class, not one blended percentile.
- Record commit/image, region, resource SKUs, replica/worker counts, dataset, test origin/network, warm/cold state, concurrency, configured throttles, dependency versions/model and measurement method. Include cold-start behavior separately; warm targets cannot conceal unusable startup.
- Healthy-baseline load must fit documented rate limits; deliberate excess load is a separate test expecting bounded `429`/safe feedback, recovery and no resource-exhaustion crash. Retries cannot conceal initial errors or inflate apparent success.

Verify two web replicas can handle the same authenticated session and read/write shared durable records/evidence without affinity or local file dependence. Repeat representative work during worker backlog and replica restart; accepted work remains durable and interactive calls remain within the API targets. Injected-outage runs are reported separately from healthy latency gates and must recover according to operational retry/recovery policy.

## 9. Browser and manual acceptance

Automate browser paths for Contributor draft/submission/follow-up; Investigator triage/case work/review submission; Supervisor return/closure; Auditor read-only review; and Administrator evidence settings/access disablement. Verify at least one real deployed Entra session for each role and app-only source access before release; test-host role injection alone is insufficient.

Inspect all approved mockup screens, including search with filters collapsed/expanded and submitted/follow-up event states. Check the ivory background, black main text, teal navigation, copper actions, gold logo, section/card structure, bounded synopsis with expansion, timeline/history, selected-record panels, and table-card pagination. Allow intentional differences required by authoritative permission/business rules; generated text artifacts do not become implementation defects.

Check keyboard navigation, visible focus, form labels, understandable validation, status indicators with text beyond color, readable contrast, sensible empty/loading/unavailable states, and no horizontal clipping at supported desktop sizes (1280×800 and 1536×1024) and 200% browser zoom. Use current stable Chromium for automated desktop acceptance and manually verify the primary workflow in Firefox; record tested browser versions. A separate mobile app or mobile-specific screen set is not added to MVP scope.

Readiness feedback must explain missing information without declaring a compliance violation. Saved/accepted/processing/failed states must reflect durable server state. API errors must not expose implementation details. Session timeout/conflict/permission changes must produce a recoverable refresh or sign-in path without falsely confirming unsaved work.

## 10. CI and deployed verification gates

| Stage | Required checks | Gate |
| --- | --- | --- |
| Working-branch change / pull request | Restore/build; unit, SQL integration and API functional suites; contract/schema checks; secret/dependency scanning | Required automated checks pass before merge to main. Test-only authentication cannot ship enabled. |
| Frontend/workflow change | Relevant browser paths and automated usability checks, plus visual inspection when layout changes | Approved user flow and role behavior remain usable. |
| Candidate deployed to test environment | Actual identity/storage/broker/search/configuration integration; smoke lifecycle; telemetry/security headers and runtime permissions | Failed deployment/readiness or failed essential smoke check blocks promotion. |
| First MVP release or material infrastructure/security change | Full scenario coverage, failure injection, performance/relevance/scaling verification, operational recovery rehearsal and manual acceptance | Evidence complete; all release gates below satisfied. |
| Public MVP deployment | Non-destructive sign-in/read/readiness checks, designated synthetic smoke data only where safe, version confirmation and telemetry check | Report actual rollout outcome; use approved operational rollback/recovery procedure if essential checks fail. |

Azure-dependent suites may run as a separately triggered CI job because they need provisioned resources and credentials. They remain automated required release checks; a green local suite cannot waive them. Run focused affected tests during development, regression suites in CI, and broader deployed tests when a changed dependency or unresolved failure justifies them. There is no arbitrary global code-coverage percentage; mandatory scenarios and meaningful assertions determine adequacy.

Do not retry flaky tests until a candidate appears green. Investigate and record nondeterminism; a quarantined required test leaves its release criterion unmet until equivalent reliable evidence exists. Store test reports, safe diagnostic artifacts and security findings with controlled access; traces/screenshots must not capture tokens or sensitive data.

## 11. Requirement-to-verification matrix

Every mandatory requirement in the baseline is assigned below. During implementation, individual test cases/results reference their exact requirement IDs and scenario IDs. Group ranges cover **every** ID in that range; a group cannot pass because only its happy path passes.

| Requirement IDs | Coverage references and required evidence |
| --- | --- |
| REQ-AUTH-001–009 | TS-01/09/11–14; §5 complete allowed/denied matrix, direct/list/count/file and immediate revocation tests. |
| REQ-EVT-001–013 | TS-01/03/14; unit/API draft/submission/original preservation, IDs/actors/status and no-case retention. |
| REQ-EVT-014–020 | TS-01/02; validated attachments, requested response/file association, additive history and visible request status. |
| REQ-TRI-001–008 | TS-03/04; outcome/rationale/actor/time, guards and correction history. |
| REQ-CASE-001–008 | TS-04; atomic creation/origin, unique IDs/links, unlink history and scoped reads. |
| REQ-ASG-001–007 | TS-05/10; active assignee, reassignment intervals, my-cases and workload reconciliation. |
| REQ-WFL-001–008 | TS-05–08/10; full transition/state matrix, priority/due/history and oversight views. |
| REQ-INV-001–007 | TS-06/11; authors/revisions/tombstones, chronology and activity-versus-note distinction. |
| REQ-EVD-001–010 | TS-01/02/06/11; §6 file validation/storage/metadata/access/removal/failure evidence. |
| REQ-SRC-001–010 | TS-09/11; §7 real indexing/relevance, scoping, bounded search and outage fallback. |
| REQ-RES-001–012 | TS-06–08/11; findings/readiness, lock, return/closure/reopen and complete retained history. |
| REQ-RPT-001–005 | TS-10; known-fixture counts/time filters/drill-down and descriptive-only presentation. |
| REQ-ADM-001–007 | TS-12; profile/access validation, disablement and atomic preserved admin history. |
| REQ-CFG-001–008 | TS-12/13; environment configuration verification, secret separation, typed settings and least-privilege service grants. |
| REQ-AUD-001–011 | TS-03–08/11–14; §6 atomic audit/immutable runtime access, safe values and logged Auditor access. |
| REQ-ING-001–007 | TS-14; §7 real processing, input/source validation, deduplication and safe diagnosis. |
| REQ-UI-001–005 | TS-01–14 role-relevant screens; §9 browser and manual journeys/status/validation. |
| REQ-ASY-001–011 | TS-14; §7 durable acceptance/outbox, retries/classification/DLQ/replay/correlation evidence. |
| REQ-SEC-001–010 | §5–7; negative/security/file/telemetry tests; deployment IAM/TLS/encryption/secret-store and resource-limit inspection. |
| REQ-REL-001–008 | §6–8; transaction/constraint/concurrency tests, restart/outage recovery, migration and host readiness verification. |
| REQ-PERF-001–007 | §8 measured API/ingestion latency, pagination, independent worker, stateless replicas and reproducible results. |
| REQ-OBS-001–008 | §7/10/12; actual correlated Azure trace/metrics, safe failure signals, business-rejection distinction and executable operations KQL. |
| REQ-MNT-001–008 | Build/dependency-boundary checks and reviewed solution/architecture; confirm modular monolith, inward interfaces/DI, shared policies and justified runtime services. |
| REQ-TST-001–010 | §2–12 implemented automated suites, CI execution and exact requirement-linked evidence. |
| REQ-DEP-001–012 | §10/12 deployment verification using operations document: Docker/ACR/Container Apps, approved Azure services, automated CI/CD, environment config, health and honest rollout outcome. |

## 12. Evidence, defects, and MVP exit criteria

Keep release verification records under `docs/testing/results/` when runs exist, with a release/commit-specific summary and references to CI artifacts. A result includes date, executor, commit/image, environment/configuration, fixture seed, exact requirement/scenario IDs, commands or job links, expected/actual outcome, failures and retest evidence. Record performance data and conditions from §8 and manual checklist outcomes. Do not prepopulate passing results in this strategy.

Verify a selected ingestion trace across acceptance → SQL/outbox → broker → worker → event/audit, with actual Application Insights/Azure Monitor evidence and operational KQL. Show expected rejection versus unexpected failure, retry/dead-letter, failed/missing file and indexing failure signals without narrative/secret disclosure. Verify host liveness separately from readiness and show an optional similarity outage does not withdraw core application readiness.

Operations verification must demonstrate the approved migration/rollout/rollback procedure, recovery of SQL and corresponding evidence availability, safe replay, index rebuild and correct environment/identity/configuration. Backup objectives, schedules and numerical alert/retry limits are supplied by the deployment/operations document; acceptance requires measured evidence against them before release.

Classify defects as:

- **Critical:** unauthorized disclosure, lost/corrupted history or accepted work, compromised credential, or inability to use the primary lifecycle.
- **High:** incorrect permission/state/audit behavior, reproducible duplicate business outcome, failed mandatory workflow, or failed required reliability/performance/recovery gate.
- **Medium/low:** secondary usability/visual or diagnostic issues that do not prevent an approved capability or weaken security/integrity.

MVP release requires all of the following:

1. All mandatory requirements have mapped executed evidence; required automated suites and deployed checks pass on the release candidate.
2. All six user flows and their significant branches are verified, including NoCase, follow-up, reassignment, returned work and reopening.
3. No unresolved critical/high defect, privacy leak, missing audit/integrity control, lost accepted work, or unverified mandatory scenario remains. A cosmetic classification cannot waive a mandatory requirement.
4. Performance, similarity demonstration and replica/reliability targets pass under recorded conditions; measured values are distinguished from targets.
5. Deployment/operations decisions and their recovery/security/observability checks are complete. There is no silent waiver for the still-separate operations document.
6. Manual visual/usability and actual identity checks are complete. Remaining nonblocking defects have recorded impact and an explicit acceptance decision.
7. The release summary identifies the tested commit, results, remaining limitations and final release decision by the project owner. Approval of this strategy alone is not that decision.

## 13. Change control

Update relevant tests when requirements, permissions, workflows, schemas, contracts or deployment policies change. Fix the owning baseline first when a discrepancy is found; do not rewrite an expected result solely to match a bug. This strategy remains scoped to the approved MVP and does not create new product features, legal compliance claims, penetration-test certification or an enterprise availability SLA.
