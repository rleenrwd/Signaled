### Version: v1

### Project: Signaled

### Phase: SDLC Planning

# Signaled - Architecture Overview (v1)

## Purpose

This document describes the high-level architecture for **Signaled**, including the major components, data flow, key backend modules, and how the frontend interacts with the API. The goal is to provide a clear blueprint for implementation and support future scaling (V2+).

---

## System Summary

**Signaled** is a full-stack internal tool with three major layers:

- **Frontend (React):** UI for ingestion, event exploration, case management, and analytics
- **Backend (Django + DRF):** REST API for ingestion, querying, cases, analytics, auth/RBAC, and auditing
- **Database (PostgreSQL):** Relational storage for events, cases, user roles, and audit logs

---

## Architecture Diagram (Conceptual)

User (Browser)
→ React UI
→ DRF API (Auth + RBAC)
→ PostgreSQL

Key flows:
- Upload CSV/JSON → Ingestion pipeline → Events tables
- Search/filter events → Query API → Indexed lookups
- Cases workflow → Case APIs → Case tables + event links
- Analytics → Aggregation APIs → SQL GROUP BY/time-buckets
- Sensitive actions → Audit middleware/service → Audit log table

---

## Core Components

### Frontend (React)
Primary UI areas (V1):
- **Upload/Ingest:** upload file, show validation results, ingestion summary
- **Event Explorer:** search, filters, sorting, pagination
- **Cases:** create/update cases, link/unlink events, notes/tags, status changes
- **Analytics Dashboard:** KPIs, trends over time, top components/recurring issues

Frontend responsibilities:
- Form validation (basic UX validation)
- Managing auth token/session
- Calling APIs + rendering server results
- Client-side state for filters/pagination (server is source of truth)

### Backend (Django + Django REST Framework)
Primary responsibilities:
- Authentication + role enforcement
- Data ingestion, validation, normalization
- Query APIs (filters, sorting, pagination)
- Case workflow APIs
- Analytics aggregation APIs
- Audit logging for sensitive actions

### Database (PostgreSQL)
Primary responsibilities:
- Strong relational integrity (FKs, constraints)
- Query performance (indexes for common filters)
- Auditability (append-style audit log entries)

---

## Backend Module Layout (Recommended)

Inside `/server/`:

- `config/` (Django project settings)
- `apps/`
  - `accounts/` (users, roles, auth helpers)
  - `events/` (event model + querying endpoints)
  - `ingestion/` (upload validation + normalization + load)
  - `cases/` (case model + linking events + notes/tags)
  - `analytics/` (aggregation endpoints)
  - `audit/` (audit log model + audit service/middleware)

Reasoning:
- Keeps boundaries clear
- Makes it easy to expand features by app
- Supports “internal platform” style architecture

---

## Data Flow

### 1) Ingestion Flow (CSV/JSON Upload → Stored Events)

1. User uploads CSV/JSON via React UI
2. Frontend sends file to DRF endpoint (multipart/form-data or JSON payload)
3. Backend performs:
   - file/type detection (csv/json)
   - schema validation (required fields, types, allowed values)
   - normalization (timestamps, severity mapping, env normalization, trimming)
   - deduping strategy (V1 can be “best-effort”)
4. Backend writes valid events to `events_event` (and related tables if needed)
5. Backend returns ingestion summary:
   - total rows
   - inserted
   - rejected
   - rejection reasons (top N)

**V1 note:** ingestion is synchronous unless file sizes require async later (V2 could use Celery/RQ).

### 2) Event Search + Filter Flow

1. User selects filters in Event Explorer
2. Frontend calls `GET /api/events?...` with query params
3. Backend builds a queryset with filters + ordering
4. DRF returns paginated results:
   - `count`, `next`, `previous`
   - `results[]`

Performance expectations:
- DB indexes on frequently filtered fields (timestamp, severity, component, env, device/system id)
- Pagination required for stable UI and DB load control

### 3) Case Investigation Flow

1. User creates a Case (title, description, status=Open)
2. User links selected events to the case
3. User updates:
   - status transitions (Open → Investigating → Resolved)
   - notes
   - tags
4. Backend enforces role permissions and writes audit entries for sensitive actions

### 4) Analytics Flow

1. Frontend calls analytics endpoints (date range required)
2. Backend runs SQL aggregations:
   - events/day (time bucket)
   - top components (count)
   - severity distribution
   - recurring issues (group by signature fields)
3. Backend returns ready-to-chart payloads

---

## Authentication & Authorization

### Auth (V1)
Start with one approach (recommended: JWT for simplicity):
- Login endpoint returns access token
- Frontend stores token (prefer memory; localStorage only if necessary)
- Token included in `Authorization: Bearer <token>`

### RBAC (Role-Based Access Control)
Roles:
- **Admin:** manage users/roles, view all, export, admin-only endpoints
- **Analyst:** full workflow access (events, cases, analytics)
- **Viewer:** read-only (events + analytics), limited case visibility if desired

Enforcement points:
- DRF permission classes per endpoint
- Object-level checks where needed (e.g., case access rules)

---

## Audit Logging

### What gets audited (V1)
Sensitive actions:
- Viewing/exporting case details (if defined as sensitive)
- Creating/updating/deleting cases
- Linking/unlinking events to cases
- Status changes
- Role/user changes (admin actions)

### How auditing works
- Audit log model: append-only entries
- Central audit helper/service called by views/serializers
- Include:
  - actor user id
  - action type
  - target type/id (case/event/etc.)
  - timestamp
  - metadata (diff summary, endpoint, ip/user agent if desired)

Goal:
- Traceability + accountability similar to internal tools

---

## API Design Principles (V1)

- RESTful resources:
  - `/events`
  - `/cases`
  - `/cases/{id}/events` (link/unlink)
  - `/analytics/*`
  - `/auth/*`
  - `/audit/*` (admin)
- Pagination for list endpoints
- Query parameters for filtering and sorting
- Consistent error shape for validation failures (especially ingestion)

---

## Frontend Architecture (React)

Recommended structure:
- `pages/` (EventExplorerPage, CaseDetailPage, AnalyticsPage, UploadPage)
- `components/` (FilterBar, EventTable, CaseForm, Charts, etc.)
- `api/` (axios/fetch wrappers)
- `auth/` (token handling, route guards)
- `state/` (optional; context or lightweight store)

Key UX principles:
- Filters drive URL query params (shareable + debuggable)
- Server is source of truth for results
- Clear ingestion error reporting (top failures + downloadable rejection report later)

---

## Non-Functional Requirements

### Reliability
- Clear validation and safe failure modes for ingestion
- Defensive handling for malformed CSV/JSON
- Transactional writes for ingestion batches when appropriate

### Performance
- Indexed fields for search filters
- Pagination mandatory
- Analytics endpoints scoped by time range

### Security
- Auth required for all endpoints except login
- RBAC enforced at API layer
- Audit logging for sensitive actions

### Maintainability
- App-based module boundaries
- Service/helper layer for ingestion + auditing
- Consistent serializers and validation rules

---

## Versioning & Future Enhancements (V2+)

Possible next steps after V1:
- Saved views for Event Explorer
- Async ingestion (Celery/RQ) for large datasets
- Export features (CSV export for cases/events)
- Advanced recurring issue detection (signatures/heuristics)
- Real-time/stream ingestion (Kafka-like simulation)
- Richer permissions (team/project scoping)
- Observability (structured logs, metrics)
