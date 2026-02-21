
# Signaled — API Specification (V1)

## Version
v1

## Project
Signaled

## Phase
SDLC Planning

---

# Purpose

This document defines the complete REST API surface for Signaled V1, including:
- Endpoints and routing
- Authentication and authorization (RBAC)
- Request and response DTOs
- Filtering, sorting, pagination standards
- Validation rules
- Audit logging requirements

This specification is written to align with an ASP.NET Core Web API + SQL Server + Entity Framework Core backend.

---

# Conventions

## Base Route
/api

## Authentication
JWT Bearer Token

Header:
Authorization: Bearer <token>

## Roles
- Admin – Full system access
- Analyst – Operational access (ingestion, events, cases, analytics)
- Viewer – Read-only access (events + analytics)

---

# Standard Response Format

## Success
{
  "success": true,
  "data": {},
  "message": null,
  "meta": null
}

## Error
{
  "success": false,
  "message": "Error description",
  "errors": [
    { "field": "fieldName", "issue": "description" }
  ]
}

---

# Pagination Standard

Query Parameters:
- page (default 1)
- pageSize (default 25, max 200)

Response meta.pagination:
- page
- pageSize
- totalCount
- totalPages
- hasNext
- hasPrevious

---

# 1. Authentication

## POST /api/auth/login

Request Body:
- email (required)
- password (required)

Response:
- accessToken
- expiresInSeconds
- user { id, email, roles }

Status Codes:
- 200 OK
- 400 Bad Request
- 401 Unauthorized

Audit:
- AUTH_LOGIN_SUCCESS
- AUTH_LOGIN_FAILURE

---

## GET /api/auth/me

Returns authenticated user.

Access:
Admin, Analyst, Viewer

---

# 2. Ingestion

## POST /api/ingestion/upload

Uploads CSV or JSON dataset.

Request:
- multipart/form-data
- file (required)

Validation:
- Allowed types: .csv, .json
- Max size (define in config)
- Required fields per record:
  - occurredAtUtc
  - severity
  - component
  - environment
  - systemId
  - message

Response:
- ingestionId
- totalRecords
- inserted
- rejected
- rejectedSamples[]

Status Codes:
- 200 OK
- 400 Bad Request
- 403 Forbidden
- 413 Payload Too Large

Audit:
- EVENT_INGESTION_STARTED
- EVENT_INGESTION_COMPLETED
- EVENT_INGESTION_FAILED

---

# 3. Events

## GET /api/events

Query Parameters:
- page
- pageSize
- startUtc
- endUtc
- severity
- component
- service
- environment
- systemId
- eventType
- sortBy (occurredAtUtc, severity, component)
- sortDir (asc, desc)

Response:
- paginated EventDTO[]

---

## GET /api/events/{eventId}

Returns full EventDetailDTO including rawPayload and fingerprint.

---

# 4. Cases

## POST /api/cases

Request Body:
- title (required, max 200)
- description (optional, max 5000)
- priority (optional)
- assignedToUserId (optional)
- initialTagIds (optional)

Audit:
- CASE_CREATED

---

## GET /api/cases

Query Parameters:
- page
- pageSize
- status
- assignedToUserId
- search
- sortBy (createdAtUtc, updatedAtUtc, status)
- sortDir

---

## GET /api/cases/{caseId}

Returns CaseDetailDTO including:
- linkedEvents[]
- notes[]
- tags[]

---

## PUT /api/cases/{caseId}

Updates editable case fields.

Audit:
- CASE_UPDATED

---

## PUT /api/cases/{caseId}/status

Request:
- status (OPEN, INVESTIGATING, RESOLVED)

Allowed transitions:
- OPEN → INVESTIGATING
- INVESTIGATING → RESOLVED
- Optional reopen allowed

Audit:
- CASE_STATUS_CHANGED

---

## POST /api/cases/{caseId}/events

Request:
- eventIds[]

Audit:
- EVENT_LINKED_TO_CASE

---

## DELETE /api/cases/{caseId}/events/{eventId}

Audit:
- EVENT_UNLINKED_FROM_CASE

---

## POST /api/cases/{caseId}/notes

Request:
- content (required)

Audit:
- CASE_NOTE_ADDED

---

# 5. Tags

## GET /api/tags

Returns TagDTO[]

---

## POST /api/tags

Request:
- name (required, unique)

Status:
- 201 Created
- 409 Conflict

---

## PUT /api/cases/{caseId}/tags

Request:
- tagIds[]

Audit:
- CASE_TAGS_UPDATED

---

# 6. Analytics

All analytics require:
- startUtc
- endUtc

## GET /api/analytics/events-over-time

Optional:
- bucket (hour, day, week)

Returns time-bucketed counts.

---

## GET /api/analytics/severity-distribution

Returns grouped counts by severity.

---

## GET /api/analytics/top-components

Query:
- limit (default 10, max 50)

Returns most frequent components.

---

## GET /api/analytics/recurring-patterns

Returns grouped recurring issues based on fingerprint or grouping logic.

---

# 7. Audit Logs (Admin Only)

## GET /api/audit

Query:
- page
- pageSize
- actorUserId
- actionType
- targetType
- startUtc
- endUtc

Returns paginated AuditLogDTO[].

---

## GET /api/audit/{auditId}

Returns single audit record.

---

# Validation Rules

## Case
- Title required
- Max 200 chars

## Notes
- Max 5000 chars

## Tag
- Unique (case-insensitive recommended)

## Ingestion
- File required
- Required event fields present
- Valid enum values enforced

---

# Status Codes Summary

200 OK  
201 Created  
204 No Content  
400 Bad Request  
401 Unauthorized  
403 Forbidden  
404 Not Found  
409 Conflict  
413 Payload Too Large  
500 Internal Server Error  

---

# Audit Coverage (Minimum)

- Login success/failure
- Ingestion start/complete/fail
- Case create/update/status change/delete
- Event link/unlink
- Note added
- Tags updated
- User role changes

---
