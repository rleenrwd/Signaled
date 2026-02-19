### Version: v1

### Project: Signaled

### Phase: SDLC Planning

# ADR-001 --- Technology Stack Pivot to C# / ASP.NET Core

## Status

Accepted

------------------------------------------------------------------------

## Context

Signaled was originally planned as a Python/Django + PostgreSQL backend
with a React frontend. During planning, the project scope and near-term
career objective were clarified: secure an entry-level role in the
Sacramento region, where many public-sector and enterprise environments
heavily use C# and the .NET ecosystem.

The project also targets enterprise patterns such as role-based access
control (RBAC), audit logging, relational data modeling, and structured
workflow management. These patterns align naturally with ASP.NET Core,
SQL Server, and ASP.NET Core Identity.

------------------------------------------------------------------------

## Decision

The backend stack will be implemented using:

-   C#
-   ASP.NET Core Web API
-   Entity Framework Core
-   SQL Server
-   ASP.NET Core Identity with JWT authentication
-   Policy-based authorization for RBAC

React remains the frontend framework.

------------------------------------------------------------------------

## Rationale

This decision was made to better align the implementation with:

-   Local job market demand (Sacramento public-sector and enterprise
    environments)
-   The .NET ecosystem's strong support for structured internal systems
-   First-party authentication and authorization via ASP.NET Core
    Identity
-   SQL Server compatibility and common enterprise database patterns
-   Maintainable layered architecture and dependency injection
-   Resume and portfolio positioning for state-aligned roles

------------------------------------------------------------------------

## Alternatives Considered

### 1) Python/Django + PostgreSQL

**Pros:** - Rapid development for ingestion workflows - Strong ecosystem
for data processing

**Cons:** - Less aligned with Sacramento/state technology stacks -
Additional custom scaffolding required for enterprise-style auth and
auditing patterns

### 2) Node/Express + MongoDB

**Pros:** - Existing experience from previous MERN projects - Fast
iteration speed

**Cons:** - Less natural fit for relational case workflows and
structured audit history - Less aligned with common public-sector stacks

### 3) Java/Spring

**Pros:** - Strong enterprise alignment - Mature ecosystem

**Cons:** - Higher ramp cost compared to current .NET momentum - Less
practical for near-term job alignment strategy

------------------------------------------------------------------------

## Consequences

### Positive Outcomes

-   Improved alignment with state and enterprise developer roles
-   Reduced complexity for auth/RBAC implementation
-   Stronger enterprise credibility of the project
-   Consistency across architecture and implementation layers

### Tradeoffs / Risks

-   Requires ramp-up in .NET patterns and EF Core configuration
-   Required updating existing planning documents for stack consistency
-   SQL Server JSON handling differs from PostgreSQL jsonb behavior

------------------------------------------------------------------------

## Follow-Up Actions

-   Update Product Brief, Architecture Overview, and ERD to reflect .NET
    stack (Completed)
-   Define API Specification aligned with controller/service structure
-   Implement EF Core migrations and initial schema
-   Create Test Plan, Test Cases, and Defect Log aligned with V1 scope
