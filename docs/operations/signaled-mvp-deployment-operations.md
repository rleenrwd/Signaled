# Signaled MVP Deployment & Operations

**Status:** Approved for MVP

**Date:** October 3, 2026

**Purpose:** Define the Azure environment, release process, operational limits, monitoring, and recovery procedures for the Signaled MVP.

## 1. Scope and authority

This document implements the [MVP requirements](../requirements/signaled-mvp-requirements.md), [architecture](../architecture/signaled-mvp-architecture.md), [data model](../architecture/signaled-mvp-data-model.md), [API contract](../architecture/signaled-mvp-api-contract.md), [security design](../security/signaled-mvp-security-design.md), and [testing strategy](../testing/signaled-mvp-testing-strategy.md). The [user flows](../design/signaled-user-flows.md) remain the workflow baseline.

**Approved for MVP means this operational plan is approved. Resources, pipelines, alerts, backups, and recovery tests still require implementation and measured verification before release.** Approval is not evidence that a deployment or test has succeeded.

The application remains an ASP.NET Core modular monolith with a separate .NET background worker. This document adds operational controls, not product features, roles, business transitions, or evidence/readiness requirements. Azure SQL is authoritative; Blob Storage holds private evidence bytes; queues and the search index are not authoritative records. No Redis, Kubernetes, additional business microservices, or automatic malware-scanning service is introduced.

## 2. Environments and deployment record

| Environment | Purpose | Isolation and data |
|---|---|---|
| Local | Development and debugging | Local SQL Server-compatible database, development configuration, synthetic fixtures. Use development-only substitutes when necessary; they do not satisfy deployed Azure acceptance checks. |
| CI | Automated verification | Disposable SQL and test resources, independent run identifiers, no deployed MVP credentials or data. Untrusted pull requests receive no cloud deployment access. |
| Azure test | Actual Azure integration, acceptance, performance, and recovery verification | Separate resource group, database, storage, queues, index, identities, and configuration from the deployed MVP. Synthetic data only. Same infrastructure definitions and security policies as MVP. |
| Azure MVP | Maintained demonstration/release environment | Single organization and Entra tenant. Synthetic demonstration data is the initial baseline. Real sensitive organizational data requires an explicit suitability review before onboarding. |

Use Bicep under `infra/` with environment parameter files containing no secrets. These implementation files are created during the build phase. Resource names include project and environment; tags identify environment, owner, and purpose. Provision only into the explicitly selected subscription and resource group.

Prefer one US Azure region for the initial deployment, with West US 2 as the starting candidate. Before provisioning, verify subscription quota, Container Apps networking, service SKUs, and a supported regional Azure embedding deployment. If unavailable, select and record a supported US region before provisioning; do not silently route incident text to a different region or external provider.

Each release record identifies commit, web/worker image digests, supported .NET version, EF migration, infrastructure revision, region, SKU/capacity, configuration version, embedding model/version/dimensions, index schema version, Entra tenant/audiences, and verification links. Record identifiers and secret version references, never secret values. Keep the record with release evidence under `docs/testing/results/` when a release exists.

## 3. Azure resource baseline

These are initial capacities, not measured capacity or availability guarantees. Change capacity only through reviewed deployment configuration and rerun affected performance checks. If a listed SKU cannot meet the test strategy, tune it before release and record the resulting configuration.

| Resource | Initial MVP baseline | Operational boundary |
|---|---|---|
| Container Registry | Basic; immutable release tags and digest-pinned images | Disable registry admin credentials. Managed identity pulls; deployment identity pushes. Retain current and at least two previous release images. |
| Container Apps environment | VNet-integrated workload-profiles environment using Consumption workloads | Separate web and worker apps. Public HTTPS ingress only for web. Worker has no public ingress. |
| Web/API app | 0.5 vCPU, 1 GiB; minimum 1, maximum 2 replicas; HTTP scaling target 20 concurrent requests | No sticky-session dependency or replica-local durable data. Verify the two-replica scenario before release. |
| Worker app | 0.5 vCPU, 1 GiB; minimum 1, maximum 2 replicas | Always retain a dispatcher/consumer instance. Queue scaling target 10 messages; SQL leases protect work shared by replicas. |
| Azure SQL | Standard S0 starting capacity | Separate databases per environment; Entra runtime access; seven-day point-in-time restore retention; locally redundant backups. |
| Blob Storage | StorageV2, Standard LRS, Hot | Private evidence, staging, and Data Protection containers. Disable anonymous access and shared-key authorization. Enable evidence versioning and 30-day Blob/container soft delete. |
| Service Bus | Standard namespace | Separate `event-ingestion` and `search-index-updates` queues. Managed identity access; disable local/SAS authentication. Queue policies are in §8. |
| Key Vault | Standard, soft delete and purge protection | Store unavoidable secrets and Data Protection encryption keys. Separate environment vaults; no secrets in application settings tables. |
| App Configuration | Standard starting tier | Versioned non-secret environment settings. Signaled's two editable upload settings remain authoritative in SQL. |
| Azure AI Search | Basic, one partition and one replica initially | Derived index only; Entra/RBAC access. Validate search performance and available capacity before release. No enterprise availability claim. |
| Azure-hosted embeddings | Supported regional deployment; prefer `text-embedding-3-small`, 1,536 dimensions where available | Explicit model/version/deployment configuration. Use the same dimensions for indexing and queries; managed identity where supported. A change requires index compatibility/rebuild verification. |
| Monitoring | Application Insights backed by a Log Analytics workspace; Azure Monitor alerts | OpenTelemetry from both hosts; 30-day operational log retention. Telemetry does not replace durable SQL audit history. |

Apply `CanNotDelete` resource locks to the MVP SQL logical server, storage account, and Key Vault after provisioning. Such locks protect resource deletion, not all data-plane deletion; runtime permissions and recovery controls remain necessary. Remove a lock only through a reviewed maintenance action. Test cleanup may remove only explicitly isolated test resources.

## 4. Network, identities, and configuration

### 4.1 Network boundary

Expose the web app through HTTPS with a stable hostname; enforce HTTPS, the security-design headers, and exact Entra redirect URIs. Do not expose worker, SQL, or evidence bytes through anonymous application routes. Health responses contain no connection details, narratives, or secrets.

Use the Container Apps VNet subnet with supported service endpoints/firewall rules for SQL, Storage, and Key Vault. Deny broad SQL "Allow Azure services" access. Run the migration job inside the environment's permitted network. If temporary developer database access is necessary, allow only the specific operator IP for a recorded, time-limited maintenance window and remove it afterward.

ACR, Service Bus, App Configuration, Search, and the embedding service may use their public TLS endpoints with identity authorization for this MVP. Disable anonymous/local-key access where the selected service supports the required Entra-only configuration. This is a deliberately limited network baseline, not a claim of complete private-endpoint isolation. Verify every firewall and identity combination in Azure test before adopting it; an unsupported rule must be corrected in reviewed infrastructure, not bypassed by weakening authentication. Preserve required Azure, Entra, DNS, and telemetry connectivity.

### 4.2 Identity separation

| Identity | Required access | Explicit restrictions |
|---|---|---|
| Web managed identity | SQL application operations and append-only audit; private evidence/staging access; shared Data Protection ring and Key Vault key use; read App Configuration; Search query and embedding inference | No schema migration, audit update/delete, subscription administration, or ordinary permanent evidence deletion. No Service Bus permission is required when the worker publishes the SQL outbox. |
| Worker managed identity | SQL ingestion/outbox/inbox/index work and audit insertion; send/receive on the two queues; index writes and embedding inference; read deployment configuration | No human-role shortcut, public endpoint, general evidence-byte access, or audit update/delete. Bind to an active Signaled worker service identity for business attribution. |
| Migration-job identity | SQL schema changes and narrowly required seed/access provisioning | Separate from runtime identities; not used by normal requests or worker processing. Restrict execution to reviewed releases/maintenance. |
| CI deployment identity | Push release images and update the named environment's apps/infrastructure within assigned scope | GitHub OIDC federation; no stored Azure client secret, subscription Owner grant, or automatic privilege escalation. Privileged initial RBAC/Entra bootstrap is separate. |
| Human operator | Azure monitoring/recovery access appropriate to assigned duties | Cloud operational privilege does not confer investigative access through the Administrator application role. Use synthetic records for routine verification. |

Provision runtime SQL grants explicitly. Audit tables allow runtime `INSERT` and authorized `SELECT`, with no `UPDATE`/`DELETE`; do not grant runtime `db_owner`. Confirm grants after every migration. Limit Blob operations by container and task. A controlled staging-cleanup identity/job may delete confirmed abandoned staging objects; it cannot erase finalized evidence history.

Configure human Entra OIDC authorization-code/PKCE authentication and deployed tenant MFA according to the security design. Register the integration API audience and approved app permissions separately. Seed the initial active Administrator, eligible replay grant if needed, and worker identity from reviewed immutable Entra identifiers; do not seed passwords, tokens, or placeholder identities. Reject shipping development authentication bypasses. Local role/access changes must take effect on the next protected operation.

### 4.3 Configuration and secrets

- Key Vault owns secrets and encryption keys. Prefer managed identity rather than secret-bearing connection strings.
- App Configuration owns endpoints, queue names, telemetry settings, model/index configuration, limits, and environment identifiers. Pin a versioned snapshot or equivalent immutable release configuration; apply changes deliberately and verify affected hosts.
- SQL `AppSetting` owns only `evidence.maxAttachmentMiB` and `evidence.allowedTypes`: default 25 MiB and PDF/JPEG/PNG/DOCX, with the security-design validation and concurrency rules. No additional editable configuration controls are introduced.
- Persist ASP.NET Core Data Protection keys in a private shared Blob container and encrypt them with the Key Vault key. Both web replicas use the same application discriminator. Retain decrypt-capable key versions while any persisted ring entry requires them; never delete old keys during routine secret rotation.
- Cookie policy remains Secure, HttpOnly, SameSite Lax, 30-minute idle and eight-hour absolute lifetime, with CSRF protection on cookie-authenticated state changes. Deployment controls cannot relax these settings through the UI.

Rotate any unavoidable secret by provisioning a new version, updating the version reference, restarting/reloading affected hosts deliberately, verifying connectivity, then revoking the old credential. Rollback of an image does not roll back app-scoped secrets or external configuration. Never put values in CI output, commit history, telemetry, queue payloads, or release records.

## 5. Build and release procedure

The `working` branch remains the integration branch; `main` receives finalized work through a reviewed merge. A commit to `working` does not automatically publish the MVP. Protect `main` with the applicable testing-strategy checks. Untrusted pull requests run verification without deployment credentials.

Use GitHub Actions with least-privilege job permissions and environment-scoped OIDC federation. Grant `id-token: write` only to trusted deployment jobs. Pin third-party actions to reviewed commit SHAs. Serialize deployments per environment; a migration lease additionally prevents simultaneous schema changes.

1. **Validate candidate:** Run required build, tests, dependency/security checks, and migration checks from the testing strategy. Resolve blocking failures; do not manufacture passing results or use retries to conceal a failing gate.
2. **Build once:** Build web and worker Docker images from the exact candidate commit. Use a supported pinned .NET toolchain/base image, non-root runtime where supported, explicit port, minimal runtime contents, and no baked credentials. Push immutable images to ACR and record digests.
3. **Deploy Azure test:** Apply reviewed Bicep/configuration, run the privileged migration job, and deploy those digests. Verify empty-database and previous-schema migrations. Run deployed identity, storage, broker, search, security, performance, and recovery checks required by the testing strategy.
4. **Approve concrete release:** The project owner reviews the exact digests, changes, test evidence, migration impact, cost configuration, and rollback plan through the release/environment gate. Documentation approval is not production deployment approval.
5. **Prepare MVP:** Confirm backups/restorability, configuration/identity references, capacity, alert routing, previous images, and migration compatibility. Apply infrastructure changes first; execute the reviewed migration once. No application startup calls to unrestricted `Database.Migrate`.
6. **Roll out:** Deploy the tested image digests. For web, use multiple-revision mode and explicit traffic assignment; verify candidate startup/readiness before moving stable traffic. Do not enable automatic traffic to whichever revision is newest. For worker, use controlled single-revision replacement with graceful draining and schema/message compatibility during overlap.
7. **Verify after cutover:** Check stable-host authentication, allowed/denied access, safe reads, a synthetic write with audit, file upload/download, durable ingestion and worker completion, structured/similarity search, and correlated telemetry. Confirm no unexpected backlog or dependency failures. Observe for at least 15 minutes before marking the release successful.
8. **Record outcome:** Save actual checks, timestamps, migration/configuration versions, digests, failures, and any rollback. Preserve the previous known-good release. A failed essential smoke check blocks completion and triggers §7.

Use the registered stable hostname for normal login callbacks. If a candidate revision is tested through a separate endpoint, explicitly register and protect its callback before authentication tests; do not accept arbitrary redirects or create an authentication bypass for smoke tests.

Use expand/contract migrations: add compatible schema first, deploy compatible readers/writers, and remove obsolete schema only in a later reviewed migration after rollback no longer needs it. Web and worker versions must tolerate overlap. Back up before a destructive maintenance change; normal MVP releases should not require destructive migrations.

## 6. Health, shutdown, and application limits

`/health/live` reports whether the process can serve its host function. `/health/ready` is a bounded check of core readiness: valid required configuration and reachable SQL; web also requires initialized authentication and shared Data Protection. Missing optional Search/embedding availability does not remove core readiness. Required evidence/broker failures surface as degraded dependency status and alerts, with affected operations failing safely; do not report them as healthy functionality. Worker readiness also checks broker connectivity. Return only a generic status externally; detailed dependency state is operator-authorized.

Configure startup probes separately from liveness/readiness so migration-free startup can finish without a restart loop. Initial probes: startup every five seconds for up to two minutes; readiness every 30 seconds with a five-second timeout and three-failure threshold; liveness every 30 seconds with a five-second timeout and three-failure threshold. Tune only against observed startup behavior and record changes. Liveness does not restart the host solely because an external service is down.

On worker shutdown, stop accepting new messages and outbox claims, finish or safely abandon in-flight work, and settle messages only after durable completion. Set a 60-second application shutdown budget and verify the platform termination configuration permits it. Locks/leases expire safely if the process is killed; duplicate delivery must remain harmless.

| Limit | Initial deployment value |
|---|---|
| Ordinary JSON and ingestion envelope | 64 KiB request body; reject oversized bodies before durable acceptance. |
| Similarity query body | 16 KiB; validate API field bounds and provider token limits before inference. |
| Upload | Per-file current SQL setting, with hard ceiling 25 MiB (26,214,400 bytes). Evidence routes accept one `file` as contracted, with a 26 MiB body ceiling. A follow-up response accepts at most three `files[]`, with a 76 MiB body ceiling including multipart overhead; validate all files before completing that response. |
| Upload execution | Stream to private staging; no whole-file memory buffering. 120-second request budget; failed/cancelled upload remains unavailable. |
| Pagination | API contract default 25, maximum 100, stable cursor ordering. |
| Reads | 600 requests/minute per authenticated principal per web replica. |
| Human writes | 120 requests/minute per principal per web replica. |
| Integration acceptance/polling | 300 requests/minute per registered source identity per web replica. |
| Upload calls | Six/minute and two concurrent calls per principal per replica. |
| Similarity | 12 queries/minute per principal; two concurrent embedding calls per web replica; bounded queue, no unbounded waiting. |

Return contract-compatible `429` with `Retry-After` for throttling and `413` for body limits; never label a rejected ingestion request Accepted. These are per-replica protective limits, not exact global quotas: at two replicas the aggregate allowance can double. No distributed limiter or new cache service is required. Record this behavior and test it. Authorization, optimistic concurrency, and idempotency remain required independently of throttling. Use trusted platform forwarding configuration for client addresses; never trust arbitrary forwarded headers.

## 7. Rollback and incident response

The project owner is the initial release/operations owner. Route alerts to an owner-monitored Azure Monitor action group; assign a backup operator before the environment is treated as continuously supported. This MVP does not claim staffed 24/7 response or a production availability SLA.

For failed rollout: stop further deployments, capture safe failure/correlation evidence, return web traffic to the previous compatible revision, and deploy the last compatible worker digest. Verify configuration/secret references independently and rerun essential smoke checks. A rollback must not re-enable revoked identities or weaken controls. Do not automatically down-migrate SQL or restore an old database to undo an application release: that can discard new actions and history. Prefer a compatible image or a reviewed forward schema fix. Use restoration only for a data-loss/corruption incident.

For suspected access leakage or corruption: contain the affected route/identity or pause processing, preserve authoritative records and telemetry, and notify the owner. Restore functionality only after the cause is understood and affected negative security tests pass. Record impact, containment, repair, reconciliation, and actual recovery time. Do not expose raw incident text through an administrative debugging page.

For dependency outage: fail the affected operation safely; never fabricate successful upload, submission, or search results. Preserve accepted SQL operations/outbox work and resume bounded processing when dependencies recover. Structured SQL search and core case work may continue while similarity is unavailable. If SQL is unavailable, reject new writes/acceptance rather than acknowledging data that was not stored.

## 8. Reliable processing, retention, and replay

### 8.1 Queue and dispatcher policy

Both queues use Peek-Lock, a 60-second lock, maximum delivery count five, seven-day message TTL, and dead-letter-on-expiration. Disable automatic completion. Start with prefetch zero and two concurrent handlers per queue per worker replica. Broker duplicate detection is not a correctness dependency; SQL uniqueness/inbox/outbox handling supplies idempotency.

Messages contain bounded references/schema version, message ID, operation/record ID, and trace/correlation context; target a maximum serialized size of 4 KiB. Do not include evidence bytes, credentials, signed Blob URLs, or raw incident narratives. Read the approved source payload from protected SQL after identity and state validation.

The worker polls the SQL outbox every two seconds, claims rows through atomic leases, and uses a 60-second renewable lease. Publish before marking dispatched. A crash between broker acknowledgement and SQL update may republish; consumers must not duplicate the event, business action, or audit outcome. Retry dispatcher transient failures with 1, 2, 4, 8, 16, then 30-second delays plus bounded jitter, preserving attempts/next-attempt time in SQL. Permanent authorization/configuration failures pause that channel and alert the operator; never discard its rows.

For each message delivery, allow at most two short SDK transport retries within the processing budget. Use exponential delay starting at 0.5 seconds, capped at two seconds. Budget a handler attempt at 60 seconds; automatic lock renewal may run for up to five minutes to accommodate safe transaction settlement. Stop/cancel work safely when the lock is lost. Retry only classified transient failures and only through idempotent application operations. Do not retry validation, revoked identity, source conflict, stale correction, or forbidden actions as if they were infrastructure failures.

On transient processing failure, abandon after a delivery-based delay of 2, 4, 8, or 16 seconds plus up to one second of jitter. The fifth unsuccessful delivery is dead-lettered; permanent failures may dead-letter immediately with a safe reason code and operation ID. Preserve the SQL outcome/attempt history. SQL and broker settlement cannot share one transaction: a 60-second reconciliation pass checks unsettled operations and observed dead letters so a crash cannot leave misleading terminal status. Do not claim broker dead-letter completion until it is observed or acknowledged.

For indexing, reread the latest approved SQL projection. Serialize updates for a record using a SQL lease/version guard across replicas; obsolete messages cannot overwrite newer indexed content. If the source changes during indexing, enqueue/retain the latest refresh. Search-result authorization always rechecks current SQL scope. Index delay or provider failure never grants access and never blocks core workflow completion.

### 8.2 Retention and cleanup

| Data | MVP retention policy |
|---|---|
| Events, cases, submitted snapshots, revisions, decisions, evidence metadata/bytes, audit and administrative history | Retain for the maintained MVP lifetime. No ordinary permanent-deletion or automatic history purge. Logical evidence removal preserves the security-design history. Legal disposition remains outside scope. |
| External source-key/event mapping and ingestion outcome/correction/replay history | Retain for MVP lifetime so an old source retry cannot create a second event. Protect original payloads in SQL; expose only permitted source data or safe operator metadata. |
| Consumer completion keys | Retain for MVP lifetime in the initial implementation; include replay attempt identity where required. Do not expire a guard while the corresponding message can still be replayed. |
| Human request idempotency records | Seven days from first acceptance. Preserve response association during that window; after expiration, the client must reconcile the original operation rather than blindly retry an old create. |
| Dispatched outbox transport payload | May compact after 30 days only if delivery/reconciliation is confirmed and no pending replay depends on it. Preserve authoritative operation, correlation, and audit history. Pending/failed outbox rows are never age-purged. |
| Queue dead letters | No automatic purge. Inspect/reconcile explicitly; broker TTL is not a substitute for DLQ management. |
| Staging objects | Sweep daily; delete only unfinalized objects older than 24 hours proven abandoned and unreferenced by any in-flight/finalized operation. Unknown objects are held for investigation, not assumed disposable. |
| Telemetry and release evidence | Operational logs 30 days; release/test/recovery evidence at least 90 days and retain the latest known-good release/recovery record. SQL audit is not subject to telemetry expiration. |

### 8.3 Controlled replay

1. Inspect safe operation metadata, attempts, broker state, and correlation using an authorized operator. Identify a transient problem versus invalid source content; do not edit raw SQL payloads as a shortcut.
2. Resolve infrastructure failure. If content requires correction, the owning registered integration submits the contract-defined correction with concurrency protection; preserve the original and all revisions.
3. An Administrator holding restricted `admin.ingestionReplay` records a reason and requests replay through the approved API. Commit replay/audit/outbox intent atomically. Reuse the original source key; create a new transport/replay attempt identity so the old delivery guard does not suppress legitimate processing.
4. The worker validates current service state and source ownership again. An already completed source maps to its existing event; replay must never create a second event.
5. Verify durable success and audit before settling the corresponding old dead-letter message. If replay fails, retain the failure and dead letter for further investigation. Record the outcome; no automatic infinite replay or uninspected DLQ clearing.

## 9. Backups and recovery

Initial objectives for recoverable database loss/corruption are **RPO at most 15 minutes** and **RTO at most four hours from operator response**, subject to demonstrated Azure restore behavior. They are acceptance objectives, not a guarantee of 24/7 response. Restore testing must measure actual loss and elapsed time; unmet objectives block release until capacity/process is corrected or the approved baseline is explicitly revised.

Azure SQL uses seven-day point-in-time restore retention. Check backup/restore availability daily while maintained and before migration. Blob evidence uses versioning plus 30-day Blob and container soft delete; retain finalized versions without an automatic version-deletion lifecycle policy. Keep Data Protection keys/decryption versions recoverable. Infrastructure/configuration definitions are in source control; secret material is recovered through protected Azure mechanisms, not Git.

LRS and this plan address local/accidental-loss recovery. They do not establish regional disaster recovery, cross-region failover, legal retention, or a SQL/Blob atomic backup. Do not claim the 15-minute objective for loss of an entire Azure region. Adding regional resilience requires a reviewed operational change.

Run the following drill in Azure test before first release, after material recovery changes, and monthly while the MVP is maintained:

1. Select a known recovery point and synthetic fixtures with event/case IDs, audit history, evidence files, source keys, and expected current access state. Preserve the existing database/storage and current administrative state for reconciliation.
2. Pause ingestion/dispatcher and restrict web writes. Restore SQL into a **new** database; never overwrite the only surviving source. Restore deleted/changed fixture Blobs from retained versions/soft delete when needed. Reapply migration-compatible configuration and runtime grants.
3. Verify record counts, IDs, revisions, audit ordering, source uniqueness, evidence metadata and downloadable fixture bytes. Check all available evidence references for existence before reopening affected records. A missing Blob is explicitly unavailable and an incident, never a successful attachment.
4. Reconcile role changes, disabled users/services, grants, configuration changes, and submissions after the recovery point using the preserved source and provider state. Keep protected traffic closed until current access is trustworthy. If access history cannot be recovered, default affected identities to disabled pending review; do not resurrect old grants by assumption.
5. Reconcile accepted requests, inbox/outbox, and broker messages before resuming. Resubmit only recorded legitimate work through idempotent processing. Record any unrecoverable accepted operation as data loss; do not hide it or manufacture historical audit entries.
6. Rebuild the derived search index from approved SQL projections using the recorded model/schema. Recheck authorization and similarity behavior. Search recovery may lag core reopening if the degraded state is explicit.
7. Switch configuration to the verified database, restart affected hosts deliberately, run release smoke/negative checks, then resume writes/processing. Record recovery point, start/end, measured RPO/RTO, evidence outcomes, reconciliation, and remaining impact.

Check evidence references daily for missing finalized Blobs and alert on any discrepancy. Do not use this inventory to delete unknown finalized evidence. Privileged restore/maintenance actions identify operator, reason, affected resources, and outcome in the operational record without pretending they were ordinary application transactions.

## 10. Telemetry, alerts, and KQL

Use OpenTelemetry for web/worker requests, dependencies, traces, and exceptions with Application Insights/Azure Monitor. Propagate validated correlation and W3C trace context across request, SQL outbox, Service Bus, and worker. Generate trusted IDs when incoming context is invalid. Safe operational fields include `eventName`, `operationId`, `correlationId`, `stage`, `failureCode`, `environment`, `build`, and `instanceId`.

Never log tokens, cookies, secret values, signed URLs, file bytes, incident narratives, raw search queries, or full ingestion bodies. Normalize/sanitize URLs and exception details; use route templates instead of record/query-bearing URLs. Record expected validation/concurrency/authorization rejection separately from unexpected application/dependency failure. Business audit remains in SQL even when telemetry is sampled; critical failure and operational snapshot signals are unsampled.

Emit `OperationStage` traces for durable acceptance, dispatch, processing, retry, completion, correction/replay, and dead-letter observation. Emit `OperationalSnapshot` once a minute with numeric `outboxOldestAgeSeconds`, `ingestionOldestAgeSeconds`, `deadLetterCount`, and `indexOldestAgeSeconds`; emit `WorkerHeartbeat` and safe `EvidenceUnavailable`/`AuthorizationDenied` signals. Snapshot values describe actual observed state, not inferred success.

| Signal | Initial threshold and action |
|---|---|
| Suspected unauthorized disclosure or data/audit corruption | Critical immediately upon detection; contain and investigate (§7). |
| Core readiness | Three consecutive failed 30-second checks; critical. |
| Unexpected web failures | Warning above 1% over five minutes with at least 20 requests; critical above 5% with at least five failures. Expected 4xx business rejections are separate. |
| Worker heartbeat | Missing for three minutes while the worker is expected running; critical. |
| Pending outbox or ingestion age | Warning above 60 seconds for two minutes; critical above five minutes. Compare processing performance to the stricter testing acceptance target. |
| Dead letters/permanent worker failure | At least one new dead letter or permanent failure; critical with safe operation/correlation metadata. |
| Missing finalized evidence | Any confirmed missing object; critical. Failed upload/storage dependencies: warning at three failures in five minutes. |
| Search/index delay | Oldest pending update above five minutes for ten minutes, or repeated provider failure; warning/degraded similarity, not core downtime. |
| Repeated authorization denial | At least 20 denials for one local principal in five minutes; warning and review. Do not automatically grant access or disable a user based solely on this count. |
| SQL capacity | DTU consumption above 85% for ten minutes; warning and investigate workload/capacity. |
| Cost | Budget alerts at 80% and 100% of the owner's recorded monthly ceiling; review usage, without automatic deletion of records/resources. |

Route critical signals immediately through the action group; use five-minute evaluation for aggregate warnings. Verify alert delivery with synthetic failures before release. Resolve an incident only after the underlying state recovers; suppress duplicate notifications without suppressing new affected operations.

The following queries use workspace-based Application Insights tables. Implement the named safe trace fields above, validate queries against actual emitted schema, and capture working results before release. Replace the example ID; do not paste narrative text into a query.

### 10.1 Trace one asynchronous operation

```kusto
let selectedOperation = "REPLACE_WITH_OPERATION_ID";
AppTraces
| where TimeGenerated > ago(24h)
| where tostring(Properties.operationId) == selectedOperation
| project TimeGenerated, AppRoleName,
    Event = tostring(Properties.eventName),
    Stage = tostring(Properties.stage),
    Failure = tostring(Properties.failureCode),
    Correlation = tostring(Properties.correlationId), OperationId
| order by TimeGenerated asc
```

### 10.2 Unexpected request failures

```kusto
AppRequests
| where TimeGenerated > ago(1h)
| extend Code = toint(ResultCode)
| summarize Requests = count(), UnexpectedFailures = countif(Code >= 500)
    by bin(TimeGenerated, 5m), AppRoleName
| extend FailurePercent = 100.0 * UnexpectedFailures / Requests
| order by TimeGenerated desc
```

### 10.3 Latest observed backlog and dead letters

```kusto
AppTraces
| where TimeGenerated > ago(10m)
| where tostring(Properties.eventName) == "OperationalSnapshot"
| extend Instance = tostring(Properties.instanceId)
| summarize arg_max(TimeGenerated, *) by AppRoleName, Instance
| project TimeGenerated, AppRoleName, Instance,
    OutboxAgeSeconds = todouble(Properties.outboxOldestAgeSeconds),
    IngestionAgeSeconds = todouble(Properties.ingestionOldestAgeSeconds),
    DeadLetters = tolong(Properties.deadLetterCount),
    IndexAgeSeconds = todouble(Properties.indexOldestAgeSeconds)
```

### 10.4 Failed operations and unavailable evidence

```kusto
AppTraces
| where TimeGenerated > ago(1h)
| where tostring(Properties.eventName) in
    ("OperationFailed", "DeadLetterObserved", "EvidenceUnavailable")
| project TimeGenerated, AppRoleName,
    Event = tostring(Properties.eventName),
    Operation = tostring(Properties.operationId),
    Correlation = tostring(Properties.correlationId),
    Failure = tostring(Properties.failureCode)
| order by TimeGenerated desc
```

For alert calculations, deduplicate global queue/SQL snapshots across worker instances; do not add two replicas' readings of the same queue. A stale snapshot is missing telemetry, not zero backlog. Validate sampled request-count behavior or disable sampling for the counters used in failure-rate alerts. Portal/KQL access is restricted to approved cloud operators; the application Administrator receives only security-approved operational metadata.

## 11. Cost and routine maintenance

Before provisioning, record a current Azure cost estimate for both maintained environments, including always-running replicas, SQL, Search, Service Bus, App Configuration, telemetry, storage/versions, embeddings, and network charges. The owner sets the monthly ceiling and action-group recipients in deployment parameters. Do not claim free operation or quote an unverified fixed price. Test resources may be provisioned only for test windows where practical; deleting them must not remove MVP backups or evidence.

Daily while maintained: check core health, backlog/dead letters, backup availability, evidence inventory, unexpected dependency failures, and budget signals. Weekly: inspect failed/retried operations, review telemetry volume and capacity, and apply reviewed dependency/base-image updates through the normal release gates. Monthly: run the recovery drill, review privileged access and unused integrations, and review costs. Rotate credentials on expiry or suspected exposure, with tested overlap/revocation. Do not make unattended schema, authorization, model, or retention changes.

For planned shutdown, stop new submissions, drain/reconcile accepted work, preserve or export required records through a reviewed maintenance process, and document retention/recovery responsibility before removing resources. Resource cleanup is never an ordinary Administrator product action.

## 12. Release acceptance and requirement coverage

Release requires the testing strategy's actual passing evidence plus verification of: reproducible Bicep provisioning; trusted OIDC pipeline; digest promotion; correct Entra/local identity mapping and MFA; runtime SQL/Blob/broker permissions; shared cookie keys and two replicas; health/shutdown behavior; limits/throttling; migration and image rollback; durable retries/DLQ/replay; KQL and delivered alerts; SQL/evidence recovery with measured objectives; configuration/secret separation; and recorded costs/owner. Critical/high defects and unverified mandatory gates block release.

The testing strategy remains authoritative for latency/load/relevance acceptance, including ingestion completion and the actual Azure tests. This document does not redefine those thresholds or mark any test passed.

| Requirement group | Operational coverage |
|---|---|
| `REQ-DEP-001–012` | §§2–7, 11–12: Docker/ACR/Container Apps, required Azure resources, CI/CD, environment configuration, health and release evidence. |
| `REQ-ASY-001–011`, `REQ-ING-*` | §§6, 8–10: durable acceptance, bounded handling, idempotency, retries, DLQ, controlled replay and correlation. |
| `REQ-OBS-001–008` | §§7, 10–12: safe telemetry, request/dependency/processing failures, Azure monitoring and executable KQL. |
| `REQ-CFG-001–008`, `REQ-AUTH-*`, `REQ-SEC-*` | §§4–7, 9–12: identity, secret separation, typed upload settings, runtime grants, access recovery and verification. |
| Evidence, audit, search, performance and maintainability requirements | §§3–12, together with linked security/testing documents: private recoverable evidence, retained append-only audit, derived search, verified capacity and unchanged modular architecture. |

## 13. Azure implementation references

Use official documentation when implementing and recheck support/limits against the selected region, SKU, SDK, and platform version. These sources explain platform behavior; Signaled-specific operational policies above remain the approved baseline.

- [Container Apps revisions and traffic](https://learn.microsoft.com/en-us/azure/container-apps/revisions)
- [Container Apps networking](https://learn.microsoft.com/en-us/azure/container-apps/networking)
- [GitHub Actions Azure OIDC federation](https://learn.microsoft.com/en-us/azure/developer/github/connect-from-azure-openid-connect)
- [Service Bus locks and settlement](https://learn.microsoft.com/en-us/azure/service-bus-messaging/message-transfers-locks-settlement)
- [Service Bus dead-letter queues](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dead-letter-queues)
- [Azure SQL automated backups](https://learn.microsoft.com/en-us/azure/azure-sql/database/automated-backups-overview?view=azuresql)
- [Azure SQL virtual-network service endpoints](https://learn.microsoft.com/en-us/azure/azure-sql/database/vnet-service-endpoint-rule-overview?view=azuresql)
- [Blob soft delete](https://learn.microsoft.com/en-us/azure/storage/blobs/soft-delete-blob-overview)
- [Blob versioning](https://learn.microsoft.com/en-us/azure/storage/blobs/versioning-overview)
- [Azure-hosted model catalog](https://learn.microsoft.com/en-us/azure/foundry/foundry-models/concepts/models-sold-directly-by-azure)
- [Azure Monitor AppTraces schema](https://learn.microsoft.com/en-us/azure/azure-monitor/reference/tables/apptraces)
- [Azure Monitor AppRequests schema](https://learn.microsoft.com/en-us/azure/azure-monitor/reference/tables/apprequests)
