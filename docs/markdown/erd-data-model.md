# Signaled — ERD / Data Model (V1)

## Purpose

This document defines the **data model** for Signaled V1: the core entities (tables/models), key fields, and relationships that support ingestion, event exploration, case investigations, analytics, role-based access, and audit logging.

> **Note:** Django provides the base `User` model; Signaled adds role mappings and domain models.

---

## Entities (Tables / Models)

### 1) User (Django Auth)
Represents authenticated users of the system.

**Key fields (typical):**
- `id` (PK)
- `username`
- `email`
- `password_hash`
- `is_active`
- `is_staff`
- `date_joined`

---

### 2) Role
Defines application roles for RBAC.

**Fields:**
- `id` (PK)
- `name` (unique) — `Admin`, `Analyst`, `Viewer`
- `description` (optional)
- `created_at`

---

### 3) UserRole
Maps users to roles (supports 1+ roles per user if desired).

**Fields:**
- `id` (PK)
- `user_id` (FK → `User.id`)
- `role_id` (FK → `Role.id`)
- `created_at`

**Constraints:**
- Unique (`user_id`, `role_id`)

---

### 4) Event
Core event record stored after ingestion/normalization.

**Fields (suggested V1):**
- `id` (PK)
- `occurred_at` (timestamp, indexed)
- `ingested_at` (timestamp)
- `severity` (enum-like string/int, indexed)
- `component` (string, indexed)
- `service` (string, indexed; optional but useful)
- `environment` (string, indexed) — e.g., `prod`, `staging`, `dev`
- `system_id` (string, indexed) — device/system identifier
- `event_type` (string, indexed; optional)
- `message` (text)
- `raw_payload` (jsonb) — original event JSON (optional but helpful)
- `fingerprint` (string, indexed) — hash/signature for dedupe/recurrence grouping (optional V1)

**Notes:**
- Use `raw_payload` to preserve original data for debugging/auditability.
- `fingerprint` enables “top recurring issues” analytics.

---

### 5) Case
Represents an investigation (incident / analysis thread).

**Fields:**
- `id` (PK)
- `title`
- `description` (text)
- `status` (string/choice) — `OPEN`, `INVESTIGATING`, `RESOLVED`
- `priority` (optional)
- `created_by_id` (FK → `User.id`)
- `assigned_to_id` (FK → `User.id`, nullable)
- `created_at` (timestamp)
- `updated_at` (timestamp)

---

### 6) CaseEvent (Join Table)
Many-to-many relationship between **Case** and **Event**.

**Fields:**
- `id` (PK)
- `case_id` (FK → `Case.id`)
- `event_id` (FK → `Event.id`)
- `linked_by_id` (FK → `User.id`)
- `linked_at` (timestamp)

**Constraints:**
- Unique (`case_id`, `event_id`)

---

### 7) CaseNote
Notes/comments added during an investigation.

**Fields:**
- `id` (PK)
- `case_id` (FK → `Case.id`)
- `author_id` (FK → `User.id`)
- `content` (text)
- `created_at` (timestamp)

---

### 8) Tag
Reusable tags for categorizing cases (and potentially events later).

**Fields:**
- `id` (PK)
- `name` (unique)
- `created_at`

---

### 9) CaseTag (Join Table)
Many-to-many relationship between **Case** and **Tag**.

**Fields:**
- `id` (PK)
- `case_id` (FK → `Case.id`)
- `tag_id` (FK → `Tag.id`)
- `created_at`

**Constraints:**
- Unique (`case_id`, `tag_id`)

---

### 10) AuditLog
Append-only audit trail for sensitive actions.

**Fields:**
- `id` (PK)
- `actor_id` (FK → `User.id`)
- `action` (string) — e.g., `CASE_CREATED`, `CASE_UPDATED`, `EVENT_LINKED`, `EXPORT_REQUESTED`
- `target_type` (string) — e.g., `case`, `event`, `user`
- `target_id` (string/int) — identifier of target object
- `metadata` (jsonb) — optional details (diff summary, filters used, etc.)
- `ip_address` (string, optional)
- `user_agent` (string, optional)
- `created_at` (timestamp, indexed)

---

## Relationships (Cardinality)

- **User 1—M Case** (as `created_by`)
- **User 1—M Case** (as `assigned_to`, optional)
- **Case M—M Event** via **CaseEvent**
- **Case 1—M CaseNote**
- **Case M—M Tag** via **CaseTag**
- **User M—M Role** via **UserRole**
- **User 1—M AuditLog** (as `actor`)

---

## Indexing Considerations (Performance)

Recommended indexes for V1 search/filter performance:

### Event
- `occurred_at`
- `severity`
- `component`
- `service` (if used)
- `environment`
- `system_id`
- `event_type` (if used)
- `fingerprint` (if used for recurrence analytics)

> Consider composite indexes later based on real query patterns (e.g., `(environment, occurred_at)`).

### AuditLog
- `created_at`
- `actor_id` (optional)

### CaseEvent
- Index `case_id` and `event_id` (often added automatically by FK/index defaults)
- Uniqueness constraint already supports lookup patterns

---

## Data Model Notes (V1 Decisions)

- **Keep Event schema flexible** using `raw_payload (jsonb)` while still normalizing key fields for fast filtering.
- **Use join tables** (`CaseEvent`, `CaseTag`, `UserRole`) to keep relationships explicit and scalable.
- **AuditLog is append-only** to preserve integrity of sensitive actions; avoid “update/delete” for audit rows except via admin retention policies (V2+).

---

