### Version: v1

### Project: Signaled

### Phase: SDLC Planning

# Signaled --- Product Brief (V1)

## Project Overview

**Signaled** is a full-stack internal investigation and reporting
platform designed to ingest, store, analyze, and track operational event
data.

The system models structured internal platforms used by organizations
for operational oversight, incident review, structured case management,
and audit accountability across services and environments.

This project is intentionally aligned with enterprise environments that
rely on secure APIs, relational data modeling, role-based access
control, and audit logging to support governance, reporting, and
internal system oversight.

------------------------------------------------------------------------

## Problem Statement

Organizations often rely on fragmented systems to:

-   Review operational records 
-   Investigate incidents 
-   Track structured investigations 
-   Analyze recurring issues 
-   Maintain accountability and traceability

Without a centralized platform:

-   Records are difficult to search and filter 
-   Investigations lack structured workflow 
-   Historical reporting is limited 
-   Audit visibility is reduced

**Signaled** addresses this by providing a unified system for ingestion,
investigation, and reporting within a secure, role-governed environment.

------------------------------------------------------------------------

## Product Vision

Deliver a secure, structured internal platform that enables users to:

-   Ingest operational datasets 
-   Search and filter event records 
-   Create structured investigation cases 
-   Track case status and workflow progression 
-   Identify trends and recurring incident patterns 
-   Maintain role-based access and audit visibility

The goal is to reflect real internal systems used for operational
oversight and structured investigation management.

------------------------------------------------------------------------

## Target Users

### Primary Users --- Analysts

-   Investigate operational incidents 
-   Filter and analyze event records 
-   Create and manage investigation cases 
-   Track case status progression

### Secondary Users --- Administrators

-   Manage user access and role assignments 
-   Review audit history 
-   Monitor sensitive activity

### View-Only Users --- Viewers

-   Access read-only dashboards and records

------------------------------------------------------------------------

## Core Features (V1 Scope)

### 1) Dataset Ingestion

-   Upload CSV/JSON datasets 
-   Validate and normalize incoming records 
-   Persist structured records in SQL Server

------------------------------------------------------------------------

### 2) Event Explorer (Search + Filter)

Users can explore operational records using filters such as:

-   Time range 
-   Severity level 
-   Component or service 
-   Environment (prod, staging, dev) 
-   System or device identifier

Includes:

-   Sorting 
-   Pagination 
-   Indexed SQL-based filtering for performance

------------------------------------------------------------------------

### 3) Investigation Workflow (Case Management)

Users can create and manage structured investigation cases:

-   Create a case 
-   Link records to a case 
-   Track status:
    -   OPEN 
    -   INVESTIGATING 
    -   RESOLVED 
-   Add notes 
-   Add tags

------------------------------------------------------------------------

### 4) Reporting & Analytics Dashboard

Provides operational insights including:

-   Records per day 
-   Severity distribution 
-   Top affected components 
-   Recurring incident patterns

All reporting is generated via aggregated SQL queries exposed through
secure API endpoints.

------------------------------------------------------------------------

### 5) Security & Auditability

-   Role-Based Access Control (RBAC):
    -   Admin 
    -   Analyst 
    -   Viewer
-   Audit logging for sensitive actions:
    -   Case updates 
    -   Record linking/unlinking 
    -   Administrative changes

Authentication implemented using ASP.NET Core Identity with JWT-based
authentication and policy-based authorization.

------------------------------------------------------------------------

## Technical Stack (V1)

### Backend

-   C#
-   ASP.NET Core Web API
-   Entity Framework Core

### Database

-   SQL Server

### Frontend

-   React

### Authentication & Authorization

-   ASP.NET Core Identity
-   JWT-based authentication
-   Policy-based authorization

------------------------------------------------------------------------

## V1 Non-Goals (Out of Scope)

-   Real-time streaming ingestion 
-   Saved views or presets 
-   Notification systems 
-   Machine learning analytics 
-   External integrations 
-   Advanced permission scoping

------------------------------------------------------------------------

## Success Criteria

V1 will be considered successful when users can:

-   Upload operational datasets 
-   Search and filter records 
-   Create and manage investigation cases 
-   Link records to cases 
-   View reporting trends 
-   Access the system based on assigned roles 
-   Maintain audit visibility for sensitive actions

------------------------------------------------------------------------

## Project Objective

Signaled is designed to demonstrate:

-   Secure API development using ASP.NET Core 
-   Structured relational data modeling with EF Core 
-   Workflow-driven case management 
-   SQL-based reporting and aggregation 
-   Role-based governance and audit logging 
-   Complete SDLC documentation and execution
