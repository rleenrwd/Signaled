# Signaled User Stories

**Status:** Approved for MVP  
**Purpose:** Define the primary user needs and outcomes that will guide Signaled's MVP requirements, user flows, interface design, and architecture.

## User Story Format

Each story follows the format:

> As a **[role]**, I want to **[goal/action]**, so that **[reason/value]**.

These stories describe user needs rather than implementation details. Formal system requirements and acceptance criteria will be defined separately.

---

## Contributor

### Event Reporting

**US-CON-001 — Create an event**  
As a **Contributor**, I want to create an event record when an operational incident occurs, so that the incident can be formally documented and reviewed.

**US-CON-002 — Provide event details**  
As a **Contributor**, I want to record relevant details about an event, so that reviewers have enough context to understand what happened.

**US-CON-003 — Attach supporting information**  
As a **Contributor**, I want to attach supporting files or evidence to an event, so that investigators can access the information related to the incident.

**US-CON-004 — Submit an event for review**  
As a **Contributor**, I want to submit a completed event record for review, so that the appropriate team can determine whether further action is required.

### Follow-Up and Visibility

**US-CON-005 — View permitted events**  
As a **Contributor**, I want to view events that I am authorized to access, so that I can reference previously submitted information.

**US-CON-006 — Check event status**  
As a **Contributor**, I want to see the current status of an event I submitted, so that I know whether it has been received, reviewed, or escalated.

**US-CON-007 — Add follow-up information**  
As a **Contributor**, I want to provide additional information when requested, so that investigators can complete their review without relying on separate communication channels.

---

## Investigator

### Event Review and Triage

**US-INV-001 — Review submitted events**  
As an **Investigator**, I want to review submitted events and their supporting information, so that I can determine whether further investigation is required.

**US-INV-002 — Record a triage decision**  
As an **Investigator**, I want to document the outcome of event triage, so that the decision and reasoning are traceable.

**US-INV-003 — Create a case from an event**  
As an **Investigator**, I want to create an investigation case from a significant event, so that the incident can be managed through a structured investigation process.

### Case Management

**US-INV-004 — Manage assigned cases**  
As an **Investigator**, I want to view and manage the cases assigned to me, so that I can organize and prioritize my investigative workload.

**US-INV-005 — Link related events to a case**  
As an **Investigator**, I want to associate multiple related events with one case, so that connected incidents can be investigated together.

**US-INV-006 — Update case status**  
As an **Investigator**, I want to update the status of a case as work progresses, so that the current state of the investigation is clear.

### Investigation Activity

**US-INV-007 — Add investigation notes**  
As an **Investigator**, I want to record investigation notes within a case, so that important observations and actions remain with the official record.

**US-INV-008 — Add and organize evidence**  
As an **Investigator**, I want to add and organize supporting evidence within a case, so that information used during the investigation is easy to locate and review.

**US-INV-009 — Track investigative actions**  
As an **Investigator**, I want important investigative actions to be recorded over time, so that the sequence of work can be reconstructed later.

### Search and Analysis

**US-INV-010 — Search historical events and cases**  
As an **Investigator**, I want to search historical events and cases, so that I can find information relevant to a current investigation.

**US-INV-011 — Find semantically related incidents**  
As an **Investigator**, I want to find incidents with similar meaning even when different wording was used, so that I can identify potentially related patterns or prior cases.

### Findings and Resolution

**US-INV-012 — Record findings**  
As an **Investigator**, I want to document my findings and supporting rationale, so that the outcome of the investigation is clear and defensible.

**US-INV-013 — Submit a case for supervisor review**  
As an **Investigator**, I want to submit a completed investigation for supervisor review, so that the findings can be evaluated before closure.

---

## Supervisor

### Assignment and Workload

**US-SUP-001 — Assign cases**  
As a **Supervisor**, I want to assign cases to investigators, so that investigative responsibility is clearly established.

**US-SUP-002 — Reassign cases**  
As a **Supervisor**, I want to reassign cases when necessary, so that workload or staffing changes do not prevent investigations from progressing.

**US-SUP-003 — Review investigator workload**  
As a **Supervisor**, I want to see active case assignments across investigators, so that I can identify workload imbalances.

### Oversight

**US-SUP-004 — Monitor case progress**  
As a **Supervisor**, I want to monitor case status and progress, so that I can identify investigations that are stalled or require attention.

**US-SUP-005 — Identify overdue or high-priority cases**  
As a **Supervisor**, I want important or overdue cases to be easy to identify, so that I can intervene before critical work is delayed.

**US-SUP-006 — Review case activity**  
As a **Supervisor**, I want to review investigation activity, notes, and evidence, so that I can understand how an investigation was conducted.

### Review and Closure

**US-SUP-007 — Review findings**  
As a **Supervisor**, I want to review investigator findings and supporting information, so that I can determine whether an investigation is ready for closure.

**US-SUP-008 — Close a case**  
As a **Supervisor**, I want to close an investigation that has completed the required review process, so that its final outcome is formally recorded.

**US-SUP-009 — Reopen a case**  
As a **Supervisor**, I want to reopen a closed case when new information or a valid review need arises, so that additional investigation can be documented without losing the original history.

### Team Visibility

**US-SUP-010 — Review team trends**  
As a **Supervisor**, I want to view summary information about cases and team activity, so that I can identify recurring issues and operational trends.

---

## Administrator

### User and Access Management

**US-ADM-001 — Manage user accounts**  
As an **Administrator**, I want to create, update, and manage user accounts, so that authorized personnel can access Signaled.

**US-ADM-002 — Assign roles and permissions**  
As an **Administrator**, I want to assign appropriate roles and permissions to users, so that access follows job responsibility and least-privilege principles.

**US-ADM-003 — Remove or disable access**  
As an **Administrator**, I want to disable access when it is no longer required, so that former or unauthorized users cannot continue using the platform.

### Platform Configuration

**US-ADM-004 — Manage system configuration**  
As an **Administrator**, I want to manage authorized system settings, so that Signaled can be configured without modifying application code.

**US-ADM-005 — Manage integrations and service access**  
As an **Administrator**, I want to manage trusted integrations and service access, so that external systems and background processes can interact with Signaled securely.

### Operations and Security

**US-ADM-006 — Review administrative activity**  
As an **Administrator**, I want administrative changes to be recorded and reviewable, so that security-sensitive actions are traceable.

**US-ADM-007 — Troubleshoot platform issues**  
As an **Administrator**, I want access to operational and diagnostic information appropriate to my role, so that I can help identify and resolve platform issues.

---

## Auditor

### Independent Review

**US-AUD-001 — Review cases without modifying them**  
As an **Auditor**, I want to review investigation records in read-only form, so that I can perform an independent review without changing the records being examined.

**US-AUD-002 — Review complete case history**  
As an **Auditor**, I want to see the complete history of a case, so that I can understand how the investigation progressed from creation through resolution.

**US-AUD-003 — Review audit activity**  
As an **Auditor**, I want to review who performed important actions and when they occurred, so that I can verify accountability and traceability.

**US-AUD-004 — Review findings and supporting evidence**  
As an **Auditor**, I want to examine findings and the evidence supporting them, so that I can determine whether the documented outcome is adequately supported.

**US-AUD-005 — Search historical records**  
As an **Auditor**, I want to search and filter historical events, cases, and activity, so that I can locate records relevant to an audit or compliance review.

**US-AUD-006 — Identify documentation gaps**  
As an **Auditor**, I want to identify missing or inconsistent investigation documentation, so that potential process or compliance issues can be reviewed.

---

## Cross-Cutting User Expectations

The following expectations apply across multiple personas and will be translated into formal MVP requirements:

- Users should only access information and actions permitted by their assigned roles and permissions.
- Important user and system actions should produce a traceable audit history.
- Historical records should remain trustworthy even when corrections, status changes, or administrative actions occur.
- Users should be able to find the information they need without relying on disconnected spreadsheets, email threads, or shared drives.
- Signaled should preserve clear ownership, status, and history throughout the event and investigation lifecycle.
