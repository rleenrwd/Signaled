# Signaled MVP Requirements

**Status:** Approved for MVP  
**Date:** September 22, 2026  
**Purpose:** Define the functional, technical, and non-functional requirements for the Signaled minimum viable product (MVP).

---

## 1. Document Purpose

This document translates the approved Signaled Product Brief, personas, and user stories into concrete requirements for the MVP. It defines what the system must do and the qualities it must demonstrate before the MVP can be considered complete.

These requirements will guide later user-flow, interface, architecture, data-model, API, security, testing, deployment, and implementation decisions. They intentionally do not prescribe detailed screen layouts, endpoint contracts, database schemas, or infrastructure designs.

## 2. Requirement Language and Identification

The following terms indicate requirement priority:

- **Shall** — mandatory for the MVP.
- **Should** — desirable for the MVP but not required for acceptance.
- **May** — optional capability that can be included if it supports the approved scope.

Each requirement has a unique identifier using the format `REQ-[CATEGORY]-[NUMBER]`.

## 3. Definitions

- **Operational Event (Event):** A reported or ingested occurrence that may require review. An event does not automatically become an investigation.
- **Triage:** The review process used to determine what action, if any, an event requires.
- **Case:** A formal investigation opened to examine one or more related events.
- **Evidence:** A file or other supporting item associated with an event or case and used during review or investigation.
- **Investigation Note:** Narrative information recorded as part of investigative work.
- **Finding:** A documented investigative conclusion supported by rationale and relevant information.
- **Audit Record:** An immutable historical record of a significant user or system action.
- **Human User:** A person authorized to use Signaled through an assigned account.
- **Service Identity:** A non-human identity authorized to perform a limited integration or background-processing function.
- **Authorized User:** A user whose permissions and record-access scope allow the requested action.

## 4. MVP Scope

The MVP shall support a complete investigation lifecycle:

**Operational Event → Review / Triage → Investigation / Case → Evidence and Analysis → Findings / Resolution → Audit History**

The MVP shall also demonstrate production-minded security, asynchronous processing, file storage, observability, configuration, testing, containerization, and cloud deployment.

---

## 5. Functional Requirements

### 5.1 Identity, Authentication, and Authorization

- **REQ-AUTH-001:** The system shall require human users to authenticate before accessing protected application capabilities or records.
- **REQ-AUTH-002:** The system shall deny protected operations when the requesting identity is unauthenticated, inactive, or unauthorized.
- **REQ-AUTH-003:** The system shall authorize actions through defined permissions rather than relying exclusively on literal role-name checks throughout the application.
- **REQ-AUTH-004:** The system shall support the Contributor, Investigator, Supervisor, Administrator, and Auditor roles as collections of permissions.
- **REQ-AUTH-005:** The system shall support non-human service identities with permissions limited to their approved integration or processing responsibilities.
- **REQ-AUTH-006:** The system shall enforce record-level visibility rules in addition to action-level permissions.
- **REQ-AUTH-007:** Contributors shall be permitted to view events they created unless their access has been explicitly restricted by an authorized policy or administrative action.
- **REQ-AUTH-008:** Permission changes and account-status changes shall take effect without requiring changes to application source code.
- **REQ-AUTH-009:** Unauthorized records shall not be disclosed through direct access, search results, file access, counts, summaries, or error details.

**Related stories:** US-CON-005; US-ADM-001, US-ADM-002, US-ADM-003, US-ADM-004, US-ADM-005; US-AUD-001; Cross-Cutting User Expectations

### 5.2 Event Creation and Submission

- **REQ-EVT-001:** The system shall allow an authorized Contributor to create an operational event record.
- **REQ-EVT-002:** The system shall capture the information required to understand and review an event, including a description of what occurred and when it occurred.
- **REQ-EVT-003:** The system shall assign each event a unique, stable identifier.
- **REQ-EVT-004:** The system shall identify when the event was created and the user or trusted service that created it.
- **REQ-EVT-005:** The system shall validate required event information before accepting an event for review.
- **REQ-EVT-006:** The system shall allow an authorized Contributor to save an incomplete event before submission.
- **REQ-EVT-007:** The system shall allow an authorized Contributor to edit an event they created while it remains an unsubmitted draft.
- **REQ-EVT-008:** The system shall allow an authorized Contributor to submit a completed event for review.
- **REQ-EVT-009:** The system shall distinguish events that are being prepared from events that have been submitted for review.
- **REQ-EVT-010:** The system shall prevent unauthorized modification of a submitted event's original information.
- **REQ-EVT-011:** The system shall display the current status of an event to users authorized to view it.
- **REQ-EVT-012:** The system shall allow authorized users to view the details and supporting information of permitted events.
- **REQ-EVT-013:** The system shall preserve events that do not result in formal cases.

**Related stories:** US-CON-001, US-CON-002, US-CON-004, US-CON-005, US-CON-006

### 5.3 Event Attachments and Follow-Up Information

- **REQ-EVT-014:** The system shall allow an authorized Contributor to attach supporting files to an event before submission.
- **REQ-EVT-015:** The system shall allow authorized investigation personnel to request follow-up information for a submitted event.
- **REQ-EVT-016:** The system shall allow an authorized Contributor to add follow-up information to an event when requested.
- **REQ-EVT-017:** The system shall allow an authorized Contributor to attach additional supporting files when responding to a follow-up request.
- **REQ-EVT-018:** The system shall preserve follow-up information as an addition to the historical record rather than silently replacing the original submitted information.
- **REQ-EVT-019:** The system shall record who requested and who supplied follow-up information and when each action occurred.
- **REQ-EVT-020:** The system shall make outstanding follow-up requests and their status visible within Signaled to the authorized Contributor and investigation personnel involved.

**Related stories:** US-CON-003, US-CON-007; US-INV-001

### 5.4 Event Review and Triage

- **REQ-TRI-001:** The system shall make submitted events available for review by authorized Investigators.
- **REQ-TRI-002:** The system shall allow an authorized Investigator to review an event and its permitted supporting information.
- **REQ-TRI-003:** The system shall allow an authorized Investigator to record a triage decision for an event.
- **REQ-TRI-004:** A triage decision shall include an outcome, rationale, decision maker, and timestamp.
- **REQ-TRI-005:** The system shall support a triage outcome that does not create a case.
- **REQ-TRI-006:** The system shall support escalation of a significant event into a formal case.
- **REQ-TRI-007:** The system shall prevent an event from being treated as fully triaged until the required decision information has been recorded.
- **REQ-TRI-008:** The system shall preserve prior triage decisions and corrections in the event's history.

**Related stories:** US-INV-001, US-INV-002, US-INV-003

### 5.5 Case Creation and Event Association

- **REQ-CASE-001:** The system shall allow an authorized Investigator to create a case from a triaged event that requires formal investigation.
- **REQ-CASE-002:** The system shall assign each case a unique, stable identifier.
- **REQ-CASE-003:** A newly created case shall remain associated with the event from which it was created.
- **REQ-CASE-004:** The system shall allow an authorized Investigator to associate multiple related events with a single case.
- **REQ-CASE-005:** The system shall prevent the same event from being associated with the same case more than once.
- **REQ-CASE-006:** The system shall allow authorized users to view the events associated with a case.
- **REQ-CASE-007:** The system shall record who linked or unlinked an event and when the action occurred.
- **REQ-CASE-008:** Removing an association shall not delete the event, the case, or the historical record of the association.

**Related stories:** US-INV-003, US-INV-005

### 5.6 Case Assignment and Workload

- **REQ-ASG-001:** The system shall allow an authorized Supervisor to assign a case to an active Investigator.
- **REQ-ASG-002:** The system shall allow an authorized Supervisor to reassign a case to another active Investigator.
- **REQ-ASG-003:** The system shall clearly identify the Investigator currently responsible for a case.
- **REQ-ASG-004:** The system shall preserve the history of assignments and reassignments, including the actor and timestamp.
- **REQ-ASG-005:** An Investigator shall be able to view the cases currently assigned to them.
- **REQ-ASG-006:** An authorized Supervisor shall be able to view active case assignments across Investigators.
- **REQ-ASG-007:** The system shall provide enough assignment and status information for an authorized Supervisor to compare active workloads.

**Related stories:** US-INV-004; US-SUP-001, US-SUP-002, US-SUP-003

### 5.7 Case Lifecycle and Progress

- **REQ-WFL-001:** The system shall maintain a defined status for each case throughout its lifecycle.
- **REQ-WFL-002:** The system shall allow an authorized Investigator to perform permitted case-status transitions as investigation work progresses.
- **REQ-WFL-003:** The system shall reject status transitions that violate the defined case workflow or the user's permissions.
- **REQ-WFL-004:** The system shall record each case-status change, including the previous status, new status, actor, and timestamp.
- **REQ-WFL-005:** The system shall support case priority and a target or due date so authorized users can identify important or overdue cases.
- **REQ-WFL-006:** The system shall allow an authorized Supervisor to view case status, priority, due date, assignment, and recent activity.
- **REQ-WFL-007:** The system shall make overdue and high-priority cases distinguishable in Supervisor-facing case information.
- **REQ-WFL-008:** The system shall preserve case ownership, status, priority, and lifecycle history when the case is reassigned, closed, or reopened.

**Related stories:** US-INV-004, US-INV-006; US-SUP-004, US-SUP-005

### 5.8 Investigation Notes and Activity

- **REQ-INV-001:** The system shall allow an authorized Investigator to add investigation notes to an assigned or otherwise permitted case.
- **REQ-INV-002:** Each investigation note shall identify its author and creation timestamp.
- **REQ-INV-003:** The system shall prevent investigation notes from being silently overwritten or removed.
- **REQ-INV-004:** The system shall represent an authorized correction or removal in a manner that preserves the original action and subsequent change in the history.
- **REQ-INV-005:** The system shall record significant investigative actions in chronological order.
- **REQ-INV-006:** Authorized Investigators, Supervisors, and Auditors shall be able to review the permitted activity history of a case.
- **REQ-INV-007:** The activity history shall distinguish business actions from explanatory investigation notes.

**Related stories:** US-INV-007, US-INV-009; US-SUP-006; US-AUD-002, US-AUD-003

### 5.9 Evidence and File Management

- **REQ-EVD-001:** The system shall allow an authorized user to upload supporting files to a permitted event or case.
- **REQ-EVD-002:** The system shall record metadata needed to identify and manage each uploaded file, including its uploader, upload time, original name, content type, size, and associated record.
- **REQ-EVD-003:** The system shall store file content separately from transactional event and case data.
- **REQ-EVD-004:** The system shall validate uploaded files against configured type and size restrictions.
- **REQ-EVD-005:** The system shall prevent users from accessing file content unless they are authorized to access its associated event or case.
- **REQ-EVD-006:** The system shall allow an authorized Investigator to organize case evidence using defined metadata rather than relying only on file names.
- **REQ-EVD-007:** The system shall record evidence upload, access where required for accountability, metadata change, and authorized removal actions.
- **REQ-EVD-008:** An authorized evidence correction or removal shall preserve the historical record and reason for the action.
- **REQ-EVD-009:** The system shall not create a completed evidence record when file storage fails.
- **REQ-EVD-010:** The system shall detect and report when evidence metadata exists but its expected stored file is unavailable.

**Related stories:** US-CON-003; US-INV-008; US-SUP-006, US-SUP-007; US-AUD-004

### 5.10 Search and Analysis

- **REQ-SRC-001:** The system shall allow authorized Investigators and Auditors to search permitted historical events and cases.
- **REQ-SRC-002:** Structured search shall support filtering by relevant criteria including identifier, status, date range, and record type.
- **REQ-SRC-003:** Case search for authorized investigation personnel shall support filtering by assignment and priority.
- **REQ-SRC-004:** Search results shall be paginated and shall not return an unrestricted data set in a single response.
- **REQ-SRC-005:** Search results shall include enough summary information for a user to identify a relevant event or case without exposing unauthorized information.
- **REQ-SRC-006:** The system shall provide semantic search over approved investigative text so an authorized Investigator can find incidents with similar meaning despite different wording.
- **REQ-SRC-007:** Semantic-search results shall identify the matching source record and provide a relevance indicator.
- **REQ-SRC-008:** Semantic similarity shall be presented as an investigative aid and shall not automatically classify an event, link records, assign responsibility, or determine a finding.
- **REQ-SRC-009:** Structured and semantic searches shall enforce the same authorization and record-visibility rules as direct record access.
- **REQ-SRC-010:** Failure of semantic search shall not prevent authorized users from using core structured search or completing the investigation workflow.

**Related stories:** US-INV-010, US-INV-011; US-AUD-005

### 5.11 Findings, Review, Closure, and Reopening

- **REQ-RES-001:** The system shall allow an authorized Investigator to record findings and supporting rationale for a case.
- **REQ-RES-002:** The system shall identify the author and timestamp of each finding.
- **REQ-RES-003:** The system shall require defined investigation information to be complete before a case can be submitted for Supervisor review.
- **REQ-RES-004:** The system shall allow an authorized Investigator to submit a completed investigation for Supervisor review.
- **REQ-RES-005:** The system shall prevent an Investigator from modifying review-controlled case information while it is awaiting Supervisor review unless the case is returned for additional work.
- **REQ-RES-006:** The system shall allow an authorized Supervisor to review the case's findings, notes, evidence, activity, and associated events.
- **REQ-RES-007:** The system shall allow an authorized Supervisor to return a case for additional investigative work with a documented reason.
- **REQ-RES-008:** The system shall allow an authorized Supervisor to close a case that has completed the required review process.
- **REQ-RES-009:** Closing a case shall require a documented resolution and shall record the closing Supervisor and timestamp.
- **REQ-RES-010:** Closed cases shall be read-only except for actions explicitly permitted by the closed-case workflow.
- **REQ-RES-011:** The system shall allow an authorized Supervisor to reopen a closed case with a documented reason.
- **REQ-RES-012:** Reopening a case shall preserve its prior findings, resolution, closure information, and complete historical record.

**Related stories:** US-INV-012, US-INV-013; US-SUP-007, US-SUP-008, US-SUP-009; US-AUD-004

### 5.12 Supervisor Trends and Operational Summaries

- **REQ-RPT-001:** The system shall provide authorized Supervisors with summary information about case volume, status, priority, timeliness, and assignment.
- **REQ-RPT-002:** The system shall allow authorized Supervisors to review summary information over a selected time period.
- **REQ-RPT-003:** Summary counts shall be derived from records the requesting user is authorized to access.
- **REQ-RPT-004:** Summary information shall allow a Supervisor to navigate to the underlying permitted records used to produce it.
- **REQ-RPT-005:** Operational summaries shall not be represented as formal statistical conclusions or automated determinations of cause.

**Related stories:** US-SUP-003, US-SUP-004, US-SUP-005, US-SUP-010

### 5.13 User and Access Administration

- **REQ-ADM-001:** The system shall allow an authorized Administrator to create and manage user accounts.
- **REQ-ADM-002:** The system shall allow an authorized Administrator to assign and remove roles or permissions.
- **REQ-ADM-003:** The system shall allow an authorized Administrator to disable a user account without deleting the account's historical activity.
- **REQ-ADM-004:** The system shall prevent disabled users from authenticating or performing new actions.
- **REQ-ADM-005:** The system shall prevent administrative access changes from changing the recorded identity of prior actions.
- **REQ-ADM-006:** The system shall validate administrative changes and prevent configurations that violate defined security constraints.
- **REQ-ADM-007:** The system shall record account, role, permission, and access-status changes in the audit history.

**Related stories:** US-ADM-001, US-ADM-002, US-ADM-003, US-ADM-006

### 5.14 Configuration and Service Access

- **REQ-CFG-001:** The system shall allow authorized configuration values to vary by environment without modifying application source code.
- **REQ-CFG-002:** The system shall distinguish sensitive secrets from non-secret application configuration.
- **REQ-CFG-003:** The system shall allow an authorized Administrator to manage only those runtime settings designated as administratively configurable.
- **REQ-CFG-004:** The system shall validate administratively configurable values before applying them.
- **REQ-CFG-005:** The system shall audit changes to administratively managed configuration.
- **REQ-CFG-006:** The system shall support trusted service identities for approved integrations and background processes.
- **REQ-CFG-007:** Service identities shall receive only the permissions necessary for their defined responsibilities.
- **REQ-CFG-008:** The system shall support disabling or revoking service access without deleting the service identity's historical actions.

**Related stories:** US-ADM-004, US-ADM-005, US-ADM-006

### 5.15 Audit History and Independent Review

- **REQ-AUD-001:** The system shall create an audit record for security-sensitive and business-significant user and system actions.
- **REQ-AUD-002:** Each audit record shall identify the action, timestamp, actor or service identity, affected record, and correlation information when applicable.
- **REQ-AUD-003:** Audit records for changes shall preserve the nature of the change and relevant prior and new values where appropriate and safe.
- **REQ-AUD-004:** Audit history shall be append-only through normal application operations.
- **REQ-AUD-005:** No application role, including Administrator, shall be able to silently modify or delete audit history.
- **REQ-AUD-006:** Corrections to audited information shall generate additional audit records rather than replacing existing audit records.
- **REQ-AUD-007:** The system shall allow an authorized Auditor to review permitted events, cases, findings, evidence metadata, and audit history without modifying investigative records.
- **REQ-AUD-008:** The system shall allow an authorized Auditor to search and filter permitted historical activity.
- **REQ-AUD-009:** The system shall expose enough history for an authorized Auditor to reconstruct material case assignments, status transitions, review decisions, closure, and reopening.
- **REQ-AUD-010:** The system shall make missing required investigation information identifiable to an authorized Auditor without automatically declaring a compliance violation.
- **REQ-AUD-011:** Audit access itself shall be recorded when required by the defined security and compliance policy.

**Related stories:** US-ADM-006; US-AUD-001, US-AUD-002, US-AUD-003, US-AUD-004, US-AUD-005, US-AUD-006; Cross-Cutting User Expectations

### 5.16 External Event Ingestion

- **REQ-ING-001:** The system shall allow an approved external system to submit operational event data through an authenticated service identity.
- **REQ-ING-002:** The system shall validate ingested data before creating or updating an operational event.
- **REQ-ING-003:** An ingested event shall identify its source system and ingestion time.
- **REQ-ING-004:** The system shall prevent a duplicate delivery from creating duplicate event records when the source supplies the required idempotency information.
- **REQ-ING-005:** Invalid ingested data shall not be persisted as a valid submitted event.
- **REQ-ING-006:** The system shall record ingestion success or failure with enough context for an authorized Administrator to troubleshoot the outcome.
- **REQ-ING-007:** An external integration shall not receive broader access to human-user or investigative capabilities than its approved purpose requires.

**Related source:** Signaled Product Brief, Sections 1, 4, 5, and 6; US-ADM-005, US-ADM-007

### 5.17 Human-User Interface

- **REQ-UI-001:** The system shall provide a web-based user interface through which authorized human users can perform the MVP capabilities permitted to their roles.
- **REQ-UI-002:** The user interface shall present only records and actions the authenticated user is authorized to access.
- **REQ-UI-003:** The user interface shall clearly communicate the current status, ownership, and available next actions for events, follow-up requests, and cases.
- **REQ-UI-004:** The user interface shall provide understandable validation, authorization, and processing feedback without exposing sensitive implementation details.
- **REQ-UI-005:** The user interface shall support the primary event-to-resolution workflow without requiring users to interact directly with the API, database, cloud portal, or messaging infrastructure.

**Related stories:** US-CON-001–007; US-INV-001–013; US-SUP-001–010; US-ADM-001–007; US-AUD-001–006

---

## 6. Asynchronous Processing Requirements

- **REQ-ASY-001:** The system shall process at least one approved long-running or failure-prone MVP workflow asynchronously outside the originating API request.
- **REQ-ASY-002:** The API shall publish asynchronous work through reliable messaging rather than requiring the client to keep the original request open until processing completes.
- **REQ-ASY-003:** Each asynchronous operation shall carry a correlation identifier across the API, message broker, processor, and resulting persistence operations.
- **REQ-ASY-004:** The processor shall validate a message before applying business changes.
- **REQ-ASY-005:** The processor shall handle duplicate message delivery without duplicating the intended business outcome.
- **REQ-ASY-006:** The system shall retry failures classified as transient according to a defined retry policy.
- **REQ-ASY-007:** The system shall not repeatedly retry failures classified as permanent without intervention.
- **REQ-ASY-008:** A message that cannot be processed successfully after its permitted retries shall be moved to a dead-letter queue.
- **REQ-ASY-009:** Dead-lettered work shall retain sufficient context for an authorized Administrator to inspect the failure and identify the affected operation.
- **REQ-ASY-010:** The system shall support controlled reprocessing of corrected dead-lettered work without creating duplicate business outcomes.
- **REQ-ASY-011:** Asynchronous processing failures shall be observable and shall not be silently discarded.

**Related source:** Signaled Product Brief, Sections 5 and 6; US-ADM-007

---

## 7. Non-Functional Requirements

### 7.1 Security and Privacy

- **REQ-SEC-001:** The system shall apply least-privilege access to human users, service identities, application components, data stores, and cloud resources.
- **REQ-SEC-002:** The system shall protect data in transit using current supported transport encryption.
- **REQ-SEC-003:** Deployed data stores shall use encryption at rest.
- **REQ-SEC-004:** Application secrets and credentials shall not be stored in source control or committed configuration files.
- **REQ-SEC-005:** The deployed application shall retrieve secrets from an approved secure secret store and shall use managed identities where supported and appropriate.
- **REQ-SEC-006:** The system shall validate and safely handle user, file, integration, and message input.
- **REQ-SEC-007:** Client-facing error responses shall not expose secrets, credentials, stack traces, internal connection details, or unauthorized record information.
- **REQ-SEC-008:** Logs, traces, and metrics shall not contain secrets and shall minimize exposure of sensitive investigative content.
- **REQ-SEC-009:** The system shall apply configured limits to requests and file uploads to reduce accidental or malicious resource exhaustion.
- **REQ-SEC-010:** Security-relevant failures and denied sensitive actions shall produce appropriate operational or audit telemetry without exposing protected information.

### 7.2 Data Integrity and Reliability

- **REQ-REL-001:** The system shall use transactional consistency when multiple data changes must succeed or fail as one business operation.
- **REQ-REL-002:** A failed business operation shall not leave a record in a state that falsely indicates successful completion.
- **REQ-REL-003:** The system shall detect conflicting concurrent updates to protected business records and shall not silently overwrite a newer change.
- **REQ-REL-004:** Persistent records shall use stable identifiers that do not depend on mutable display values.
- **REQ-REL-005:** The system shall preserve referential integrity among events, cases, assignments, findings, evidence metadata, and audit records.
- **REQ-REL-006:** Temporary infrastructure failures shall produce recoverable or retryable outcomes where safe rather than silent data loss.
- **REQ-REL-007:** The system shall expose health information sufficient to distinguish application availability from critical dependency readiness.
- **REQ-REL-008:** Database schema changes shall be versioned and repeatable across supported environments.

### 7.3 Performance and Scalability

- **REQ-PERF-001:** The system shall define and verify measurable response-time targets for common API operations in the deployed MVP environment before MVP acceptance.
- **REQ-PERF-002:** The system shall define and verify a measurable processing-time target for the selected asynchronous workflow before MVP acceptance.
- **REQ-PERF-003:** List and search operations shall use pagination and bounded page sizes.
- **REQ-PERF-004:** Long-running processing shall not unnecessarily block interactive API requests.
- **REQ-PERF-005:** The application container shall remain stateless with respect to durable business data so multiple instances can serve requests safely.
- **REQ-PERF-006:** The deployed API shall support horizontal scaling without relying on local instance storage for durable records or evidence.
- **REQ-PERF-007:** Performance targets and test conditions shall be documented with the test results so measurements are reproducible.

### 7.4 Observability and Troubleshooting

- **REQ-OBS-001:** The system shall emit structured application logs for important application, security, and processing events.
- **REQ-OBS-002:** The system shall emit distributed traces and relevant metrics through OpenTelemetry-compatible instrumentation.
- **REQ-OBS-003:** Deployed telemetry shall be available through Application Insights and Azure Monitor.
- **REQ-OBS-004:** The system shall propagate correlation information across API requests, Service Bus messages, background processing, and persistence operations.
- **REQ-OBS-005:** An authorized Administrator shall be able to trace a selected asynchronous operation from its originating request through its processing outcome.
- **REQ-OBS-006:** The system shall provide observable signals for failed requests, dependency failures, message retries, dead-lettered messages, and failed file operations.
- **REQ-OBS-007:** Operational documentation shall include KQL queries for investigating selected health, failure, and end-to-end correlation scenarios.
- **REQ-OBS-008:** Telemetry shall distinguish expected business-rule rejections from unexpected application or infrastructure failures.

### 7.5 Maintainability and Architecture

- **REQ-MNT-001:** The application shall be implemented as one primary deployable modular monolith rather than as independently deployed microservices.
- **REQ-MNT-002:** The solution shall separate Domain, Application, Infrastructure, and API responsibilities through explicit project or module boundaries.
- **REQ-MNT-003:** Domain and application rules shall not depend directly on user-interface, database-provider, messaging-provider, file-storage-provider, or cloud-hosting implementations.
- **REQ-MNT-004:** Infrastructure components shall implement abstractions owned by an inward application boundary where dependency inversion is appropriate.
- **REQ-MNT-005:** Dependencies shall be explicit and supplied through dependency injection where appropriate.
- **REQ-MNT-006:** Cross-cutting behavior such as authorization, validation, error handling, and telemetry shall be implemented consistently rather than duplicated across individual endpoints.
- **REQ-MNT-007:** Major architectural decisions and their product justification shall be recorded in architecture documentation or decision records.
- **REQ-MNT-008:** The system shall avoid adding infrastructure services that do not satisfy an approved product or quality requirement.

### 7.6 Testing and Verification

- **REQ-TST-001:** Automated unit tests shall verify important domain and application rules.
- **REQ-TST-002:** Automated integration tests shall verify critical persistence and infrastructure interactions.
- **REQ-TST-003:** Automated functional or API tests shall verify the primary event-to-case investigation workflow.
- **REQ-TST-004:** Automated tests shall verify representative allowed and denied authorization scenarios for each human role.
- **REQ-TST-005:** Automated tests shall verify case-transition, review, closure, and reopening rules.
- **REQ-TST-006:** Automated tests shall verify that significant actions produce the expected audit records.
- **REQ-TST-007:** Automated tests shall verify duplicate-message handling, retry classification, and dead-letter behavior for the selected asynchronous workflow.
- **REQ-TST-008:** Automated tests shall verify representative file-validation, storage-failure, and unauthorized-file-access scenarios.
- **REQ-TST-009:** Automated tests shall run as part of the continuous-integration process.
- **REQ-TST-010:** The test strategy shall identify which requirements are verified by unit, integration, functional, security, performance, or manual testing.

### 7.7 Deployment and Environment Configuration

- **REQ-DEP-001:** The ASP.NET Core application shall be packaged as a Docker container.
- **REQ-DEP-002:** The deployable container image shall be stored in Azure Container Registry.
- **REQ-DEP-003:** The primary application shall be hosted in Azure Container Apps.
- **REQ-DEP-004:** The deployed transactional database shall use Azure SQL Database and local development shall support SQL Server through the same EF Core-based application persistence design.
- **REQ-DEP-005:** Deployed file content shall be stored in Azure Blob Storage.
- **REQ-DEP-006:** Reliable asynchronous messaging shall use Azure Service Bus.
- **REQ-DEP-007:** Asynchronous processing shall use an Azure Function or background worker selected during architecture design according to the approved workflow requirements.
- **REQ-DEP-008:** Deployed secrets shall use Azure Key Vault.
- **REQ-DEP-009:** Centralized non-secret environment configuration shall use Azure App Configuration where it provides a justified operational benefit.
- **REQ-DEP-010:** The build and deployment process shall use automated continuous-integration and continuous-delivery workflows for defined environments.
- **REQ-DEP-011:** Deployment shall not require manual modification of application source code between environments.
- **REQ-DEP-012:** The deployment process shall verify application health and report an unsuccessful deployment without presenting it as successful.

---

## 8. MVP Constraints and Explicit Exclusions

The following constraints and exclusions protect the MVP from unjustified complexity:

- Signaled shall remain a modular monolith for the MVP.
- The MVP shall not require Kubernetes or Azure Kubernetes Service.
- The MVP shall not introduce Redis, Cosmos DB, Event Grid, or another platform service unless a later approved requirement demonstrates a genuine need.
- The MVP shall not include a generic chatbot.
- Semantic search shall support a defined investigative-search use case and shall not make investigative decisions.
- Signaled shall not attempt to function as a SIEM, endpoint-detection platform, data-loss-prevention platform, or insider-threat detection system.
- Upstream systems may detect or generate events; Signaled is responsible for their structured review, investigation, documentation, and resolution.
- The MVP shall not be decomposed into microservices solely to demonstrate distributed-system technologies.
- Detailed department- or team-based record visibility rules are not locked in this document and shall not be assumed without an approved authorization design.
- Organization-specific legal retention schedules, records-disposition rules, and permanent-deletion procedures are outside the MVP unless later adopted as explicit requirements.
- The absence of an organization-specific retention schedule shall not permit users or Administrators to silently delete events, cases, evidence history, findings, or audit records through normal application operations.
- Technology shall not be added solely because it appears in a certification curriculum or improves the apparent size of the technology stack.

## 9. Assumptions and Open Decisions

The following decisions remain open and shall be resolved in the appropriate later design artifact:

1. The authentication provider and exact authentication flow.
2. The complete permission catalog and role-to-permission mapping.
3. Record visibility beyond a Contributor's access to events they created, including department- or team-level access.
4. The exact case-status model and permitted transition matrix.
5. The required event fields, case fields, evidence metadata, findings structure, and closure criteria.
6. The selected asynchronous MVP workflow and whether it is processed by an Azure Function or background worker.
7. The exact external-ingestion contract and idempotency mechanism.
8. The vector-search technology, indexing process, source text, and relevance approach.
9. The specific performance targets and representative workload for MVP acceptance.
10. File-type, file-size, application-level retention, malware-scanning, and evidence-access logging policies. Organization-specific legal retention and records-disposition rules are outside the MVP unless explicitly adopted later.
11. Audit-data retention and any organization-specific compliance rules.
12. Which settings are administratively configurable through the application and which remain deployment-managed.

These open decisions do not remove the associated capability from MVP scope and do not prevent approval of this requirements baseline. They identify implementation and design details that shall be resolved in the appropriate workflow, interface, architecture, security, data-model, API, testing, or operational documents before the affected capability is implemented.

## 10. Requirement Traceability and Verification

Traceability begins in this document through the **Related stories** references beneath each functional category. A detailed requirements traceability matrix may be created alongside the future testing documentation once acceptance criteria and verification methods are finalized.

Each mandatory requirement shall eventually be verified by one or more of the following:

- Automated unit test
- Automated integration test
- Automated functional or API test
- Authorization or security test
- Performance or reliability test
- Deployment verification
- Documented manual inspection or demonstration

The MVP shall not be considered complete solely because its primary screens or CRUD operations function. Completion requires verification of the full investigation lifecycle and the security, auditability, reliability, observability, testing, and deployment requirements that make the workflow trustworthy.
