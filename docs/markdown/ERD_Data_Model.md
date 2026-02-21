# Signaled --- ERD / Data Model (V1)

## Purpose

This document defines the **relational data model** for Signaled V1: the
core entities (tables/models), key fields, relationships, and indexing
considerations that support:

-   Dataset ingestion
-   Record exploration (search/filter)
-   Structured case workflows
-   Reporting and trend analysis
-   Role-based access control (RBAC)
-   Audit logging for accountability

> **Note:** ASP.NET Core Identity provides base user and role entities.
> Signaled adds domain entities for records, cases, tags, and audit
> history.

------------------------------------------------------------------------

## Entities (Tables / Models)

### 1) ApplicationUser (ASP.NET Core Identity)

Represents authenticated users of the system.

**Key fields (Identity defaults + common extensions):** - `Id` (PK,
string GUID recommended) - `UserName` - `Email` - `PasswordHash` -
`EmailConfirmed` - `LockoutEnabled` - `LockoutEnd` (nullable) -
`AccessFailedCount` - `IsActive` (optional custom flag) - `CreatedAt`
(optional custom field)

**Notes:** - Identity provides strong defaults for authentication and
account security. - Use `IsActive` if business rules require disabling
access without deleting accounts.

------------------------------------------------------------------------

### 2) ApplicationRole (ASP.NET Core Identity Role)

Defines application roles for RBAC.

**Fields:** - `Id` (PK) - `Name` (unique) --- `Admin`, `Analyst`,
`Viewer` - `NormalizedName` (Identity default) - `Description` (optional
custom field) - `CreatedAt` (optional custom field)

------------------------------------------------------------------------

### 3) UserRole (IdentityUserRole)

Maps users to roles (supports one or more roles per user).

**Fields:** - `UserId` (FK → `ApplicationUser.Id`) - `RoleId` (FK →
`ApplicationRole.Id`) - `CreatedAt` (optional extension)

**Constraints:** - Unique (`UserId`, `RoleId`) (Identity effectively
enforces uniqueness by key)

**Notes:** - In many internal systems, a single primary role is
sufficient in V1. - Supporting multiple roles is useful for future
expansion without schema changes.

------------------------------------------------------------------------

### 4) Event (Operational Record)

Core operational record stored after ingestion and normalization.

**Fields (suggested V1):** - `Id` (PK) - `OccurredAt` (datetime2,
indexed) - `IngestedAt` (datetime2) - `Severity` (nvarchar, indexed) ---
enum-like string (e.g., `LOW`, `MEDIUM`, `HIGH`) - `Component`
(nvarchar, indexed) - `Service` (nvarchar, indexed; optional but
useful) - `Environment` (nvarchar, indexed) --- e.g., `prod`, `staging`,
`dev` - `SystemId` (nvarchar, indexed) --- device/system identifier -
`EventType` (nvarchar, indexed; optional) - `Message` (nvarchar(max)) -
`RawPayload` (nvarchar(max)) --- JSON string of original record
(optional but helpful) - `Fingerprint` (nvarchar, indexed; optional) ---
signature/hash for dedupe and recurrence grouping

**Notes:** - `RawPayload` preserves the original source record for
traceability. - `Fingerprint` supports "recurring incident patterns"
reporting and basic deduping strategies. - Store JSON as nvarchar(max);
validate JSON format in ingestion service where needed.

------------------------------------------------------------------------

### 5) Case (Investigation Record)

Represents a structured investigation workflow.

**Fields:** - `Id` (PK) - `Title` (nvarchar) - `Description`
(nvarchar(max)) - `Status` (nvarchar) --- `OPEN`, `INVESTIGATING`,
`RESOLVED` - `Priority` (nullable) --- optional (e.g., `LOW`, `MEDIUM`,
`HIGH`) - `CreatedById` (FK → `ApplicationUser.Id`) - `AssignedToId` (FK
→ `ApplicationUser.Id`, nullable) - `CreatedAt` (datetime2) -
`UpdatedAt` (datetime2)

**Notes:** - `CreatedById` supports accountability for record
creation. - `AssignedToId` supports workflow ownership, but can remain
optional in V1.

------------------------------------------------------------------------

### 6) CaseEvent (Join Table)

Many-to-many relationship between **Case** and **Event**.

**Fields:** - `Id` (PK) - `CaseId` (FK → `Case.Id`) - `EventId` (FK →
`Event.Id`) - `LinkedById` (FK → `ApplicationUser.Id`) - `LinkedAt`
(datetime2)

**Constraints:** - Unique (`CaseId`, `EventId`)

**Notes:** - This join table allows an Event to be linked to multiple
Cases if needed. - `LinkedById` supports accountability for
investigative actions.

------------------------------------------------------------------------

### 7) CaseNote

Notes added during an investigation.

**Fields:** - `Id` (PK) - `CaseId` (FK → `Case.Id`) - `AuthorId` (FK →
`ApplicationUser.Id`) - `Content` (nvarchar(max)) - `CreatedAt`
(datetime2)

**Notes:** - Notes provide structured context and supporting information
for investigations. - Notes can be restricted by role if needed in
future versions.

------------------------------------------------------------------------

### 8) Tag

Reusable tags for categorizing cases (and possibly records in future
versions).

**Fields:** - `Id` (PK) - `Name` (unique) - `CreatedAt` (datetime2)

------------------------------------------------------------------------

### 9) CaseTag (Join Table)

Many-to-many relationship between **Case** and **Tag**.

**Fields:** - `Id` (PK) - `CaseId` (FK → `Case.Id`) - `TagId` (FK →
`Tag.Id`) - `CreatedAt` (datetime2)

**Constraints:** - Unique (`CaseId`, `TagId`)

------------------------------------------------------------------------

### 10) AuditLog

Append-only audit history for sensitive actions and administrative
changes.

**Fields:** - `Id` (PK) - `ActorId` (FK → `ApplicationUser.Id`) -
`Action` (nvarchar) --- e.g., `CASE_CREATED`, `CASE_UPDATED`,
`EVENT_LINKED`, `ROLE_ASSIGNED` - `TargetType` (nvarchar) --- e.g.,
`Case`, `Event`, `User`, `Role` - `TargetId` (nvarchar) --- identifier
of target object (string to support multiple entity key types) -
`Metadata` (nvarchar(max)) --- optional JSON details (diff summary,
filters used, etc.) - `IpAddress` (nvarchar, optional) - `UserAgent`
(nvarchar, optional) - `CreatedAt` (datetime2, indexed)

**Notes:** - Audit logs should remain append-only. - Admin retention
policies may be applied in V2+ if needed.

------------------------------------------------------------------------

## Relationships (Cardinality)

-   **ApplicationUser 1---M Case** (as `CreatedBy`)
-   **ApplicationUser 1---M Case** (as `AssignedTo`, optional)
-   **Case M---M Event** via **CaseEvent**
-   **Case 1---M CaseNote**
-   **Case M---M Tag** via **CaseTag**
-   **ApplicationUser M---M ApplicationRole** via **UserRole**
-   **ApplicationUser 1---M AuditLog** (as `Actor`)

------------------------------------------------------------------------

## Indexing Considerations (Performance)

Recommended indexes for V1 search/filter performance:

### Event

-   `OccurredAt`
-   `Severity`
-   `Component`
-   `Service` (if used)
-   `Environment`
-   `SystemId`
-   `EventType` (if used)
-   `Fingerprint` (if used for recurrence reporting)

> Consider composite indexes later based on real query patterns (e.g.,
> `(Environment, OccurredAt)`).

### AuditLog

-   `CreatedAt`
-   `ActorId` (optional)

### CaseEvent

-   Index `CaseId` and `EventId` (often added by FK defaults)
-   Uniqueness constraint supports common lookup patterns

------------------------------------------------------------------------

## Data Model Notes (V1 Decisions)

-   **Keep Event schema flexible** by storing `RawPayload` as JSON
    string while still normalizing key fields for fast filtering.
-   **Use join tables** (`CaseEvent`, `CaseTag`, `UserRole`) to keep
    relationships explicit and scalable.
-   **AuditLog remains append-only** to preserve integrity of sensitive
    actions; avoid update/delete for audit rows except via retention
    policies (V2+).

------------------------------------------------------------------------
