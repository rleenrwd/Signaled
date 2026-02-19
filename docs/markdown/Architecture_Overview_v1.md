### Version: v1

### Project: Signaled

### Phase: SDLC Planning

# Signaled - Architecture Overview (v1)

## Purpose

This document describes the high-level architecture for **Signaled**,
including major components, data flow, backend modules, key services,
and frontend interaction patterns. The goal is to provide a clear
implementation blueprint using ASP.NET Core Web API and SQL Server,
while supporting future scalability (V2+).

------------------------------------------------------------------------

## System Summary

**Signaled** is a full-stack internal investigation and reporting
platform composed of three major layers:

-   **Frontend (React):** User interface for ingestion, record
    exploration, structured case management, and reporting dashboards
-   **Backend (ASP.NET Core Web API):** REST API supporting ingestion,
    querying, workflow management, reporting endpoints, authentication,
    role-based access control, and audit logging
-   **Database (SQL Server):** Relational storage for operational
    records, investigation cases, user roles, and audit history

------------------------------------------------------------------------

## Architecture Diagram (Conceptual)

User (Browser) → React UI → ASP.NET Core Web API (JWT Auth +
Authorization Policies) → SQL Server

Key flows:

-   Upload CSV/JSON → IngestionService → Event tables
-   Search/filter records → EventsService → Indexed queries
-   Case workflow → CasesService → Case tables + event relationships
-   Reporting → AnalyticsService → Aggregation queries (GROUP BY / time
    buckets)
-   Sensitive actions → AuditService → AuditLog table

------------------------------------------------------------------------

## Core Components

### Frontend (React)

Primary UI areas (V1):

-   **Upload / Ingest:** Upload file, display validation results,
    ingestion summary
-   **Event Explorer:** Search, filters, sorting, pagination
-   **Cases:** Create/update cases, link/unlink records, notes/tags,
    status transitions
-   **Reporting Dashboard:** KPIs, trends over time, recurring incident
    analysis

Frontend responsibilities:

-   Basic UX validation
-   Managing authentication token/session
-   Calling backend APIs
-   Rendering paginated server results
-   Maintaining filter/pagination state (server remains source of truth)

------------------------------------------------------------------------

## Backend (ASP.NET Core Web API)

Primary responsibilities:

-   Authentication and role enforcement (JWT + Identity)
-   Dataset ingestion, validation, normalization
-   Record query APIs (filters, sorting, pagination)
-   Case workflow management APIs
-   Reporting aggregation endpoints
-   Audit logging for sensitive actions

Architecture layers:

-   Controllers (HTTP endpoints)
-   Services (business logic layer)
-   Data Layer (Entity Framework Core DbContext)
-   Middleware (error handling and auditing hooks)
-   Authorization Policies (RBAC enforcement)

------------------------------------------------------------------------

## Database (SQL Server)

Primary responsibilities:

-   Strong relational integrity (foreign key constraints)
-   Query performance (indexes on common filters)
-   Auditability (append-only audit log entries)
-   Transactional writes for ingestion batches

ORM:

-   Entity Framework Core (Code-First Migrations)

------------------------------------------------------------------------

## Backend Project Structure (Recommended)

Inside `/Server/Signaled.Api/`:

Controllers/ Services/ Models/ DTOs/ Data/ Middleware/ Auth/

Logical service areas:

-   AccountsService (users, roles, authentication helpers)
-   EventsService (record storage + querying logic)
-   IngestionService (upload validation + normalization + load)
-   CasesService (case lifecycle + linking records + notes/tags)
-   AnalyticsService (reporting and aggregation logic)
-   AuditService (audit logging logic)

Reasoning:

-   Clear separation of concerns
-   Structured maintainability
-   Supports feature expansion without controller bloat

------------------------------------------------------------------------

## Data Flow

### 1) Ingestion Flow (CSV/JSON Upload → Stored Records)

1.  User uploads CSV/JSON via React UI
2.  Frontend sends file to POST /api/ingestion
3.  Backend performs:
    -   File type detection
    -   Schema validation (required fields, types, allowed values)
    -   Normalization (timestamp formatting, severity mapping, trimming)
    -   Deduplication strategy (best-effort in V1)
4.  Backend writes valid records using EF Core within a transaction
5.  Backend returns ingestion summary:
    -   Total rows
    -   Inserted
    -   Rejected
    -   Rejection reasons

V1: Synchronous ingestion
V2: Background processing via HostedService or job scheduler

------------------------------------------------------------------------

### 2) Record Search + Filter Flow

1.  User selects filters in Event Explorer
2.  Frontend calls GET /api/events with query parameters
3.  Controller passes filters to EventsService
4.  LINQ queries applied with pagination
5.  API returns paginated response:
    -   count
    -   page
    -   pageSize
    -   results\[\]

Performance expectations:

-   Indexed fields: timestamp, severity, component, environment, system
    ID
-   Pagination required for UI stability and database load control

------------------------------------------------------------------------

### 3) Case Investigation Flow

1.  User creates a Case (title, description, status=OPEN)
2.  User links selected records to the case (many-to-many)
3.  User updates:
    -   Status transitions (OPEN → INVESTIGATING → RESOLVED)
    -   Notes
    -   Tags
4.  Backend enforces authorization policies
5.  AuditService logs sensitive actions

------------------------------------------------------------------------

### 4) Reporting Flow

1.  Frontend calls reporting endpoints (date range required)
2.  Backend runs SQL aggregation queries:
    -   Records per day (time bucket)
    -   Top components
    -   Severity distribution
    -   Recurring incident patterns (group by fingerprint/signature)
3.  API returns chart-ready payloads

------------------------------------------------------------------------

## Authentication & Authorization

### Authentication (V1)

-   JWT-based authentication
-   Login endpoint returns access token
-   Token included in Authorization header

### Role-Based Access Control (RBAC)

Roles:

-   Admin
-   Analyst
-   Viewer

Enforcement:

-   ASP.NET Core \[Authorize\] attributes
-   Policy-based authorization
-   Resource-level checks where required

------------------------------------------------------------------------

## Audit Logging

### Audited Actions (V1)

-   Creating/updating/deleting cases
-   Linking/unlinking records
-   Status changes
-   Role/user modifications
-   Sensitive exports (if implemented)

### Audit Implementation

AuditLog table (append-only):

-   UserId
-   ActionType
-   TargetType
-   TargetId
-   Timestamp
-   Metadata (diff summary, endpoint, optional IP)

Goal:

-   Traceability
-   Accountability
-   Structured internal governance standards

------------------------------------------------------------------------

## API Design Principles (V1)

-   RESTful resources:
    -   /api/events
    -   /api/cases
    -   /api/cases/{id}/events
    -   /api/analytics
    -   /api/auth
    -   /api/audit (admin)
-   Pagination for list endpoints
-   Query parameters for filtering and sorting
-   Consistent error response structure
-   DTOs separate from EF entities

------------------------------------------------------------------------

## Non-Functional Requirements

### Reliability

-   Clear validation and safe ingestion failure modes
-   Defensive handling for malformed CSV/JSON
-   Transactional writes for ingestion batches

### Performance

-   Indexed search fields
-   Pagination mandatory
-   Reporting endpoints scoped by time range

### Security

-   Authentication required for all endpoints except login
-   RBAC enforced at API layer
-   Audit logging for sensitive actions

### Maintainability

-   Layered architecture
-   Service-based business logic
-   Consistent DTO mapping
-   EF Core migrations versioned in source control

------------------------------------------------------------------------

## Versioning & Future Enhancements (V2+)

-   Saved views for Event Explorer
-   Async ingestion pipeline
-   Export features (CSV export for cases/events)
-   Advanced recurring pattern detection
-   Real-time ingestion simulation
-   Richer permission scoping (teams/projects)
-   Observability (structured logs, metrics, tracing)
