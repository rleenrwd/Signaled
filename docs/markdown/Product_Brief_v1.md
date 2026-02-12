### Version: v1

### Project: Signaled

### Phase: SDLC Planning

# Signaled — Product Brief (V1)

## Project Overview

**Signaled** is a full-stack internal engineering investigation and analytics platform designed to ingest, store, analyze, and track system event data.

The application simulates the type of internal tools used by engineering organizations to monitor systems, investigate incidents, and identify recurring issues across services, components, and environments.

This project is intentionally aligned with real-world engineering workflows commonly found in large-scale technology companies, focusing on data pipelines, internal dashboards, case management, and secure auditability.

---

## Problem Statement

Engineering teams often rely on fragmented tools to:

- View system events  
- Investigate failures  
- Track incidents  
- Analyze recurring issues  
- Share investigation context  

Without a centralized system:

- Event data is difficult to explore  
- Investigations lack structure  
- Historical insights are hard to extract  
- Accountability and audit trails are limited  

**Signaled** addresses this by creating a single platform for ingesting, investigating, and analyzing system events.

---

## Product Vision

Build an internal engineering tool that enables teams to:

- Ingest event datasets  
- Search and filter event data  
- Investigate issues through structured cases  
- Track investigation progress  
- Identify trends and recurring failures  
- Maintain auditability and access control  

The goal is to mirror real internal engineering platforms used for reliability, privacy, and service operations work.

---

## Target Users

### Primary Users

**Engineers / Analysts**
- Investigate incidents  
- Filter and analyze event data  
- Create and manage cases  
- Track investigation status  

### Secondary Users

**Admins**
- Manage system access  
- Monitor platform usage  
- Review sensitive activity logs  

**Viewers**
- Read-only access to events and dashboards  

---

## Core Features (V1 Scope)

### 1) Event Data Ingestion

- Upload CSV/JSON datasets  
- Validate and normalize incoming data  
- Store structured event records in PostgreSQL  

This simulates real ingestion pipelines in a simplified first version.

---

### 2) Event Explorer (Search + Filter)

Users can explore event data using filters such as:

- Time range  
- Severity level  
- Component / service name  
- Environment (prod, staging, dev)  
- Device or system ID  

Includes:

- Sorting  
- Pagination  
- Fast query-based filtering  

---

### 3) Investigation Workflow

Users can create structured investigations using **Cases**:

- Create a case  
- Link events to a case  
- Track status:
  - Open  
  - Investigating  
  - Resolved  
- Add notes  
- Add tags  

This introduces workflow structure similar to internal incident tracking tools.

---

### 4) Analytics Dashboard

High-level insights including:

- Events per day  
- Error trends over time  
- Top affected components  
- Recurring issue patterns  

Focus is on simple but meaningful engineering metrics.

---

### 5) Security & Auditability

- Role-based access:
  - Admin  
  - Analyst  
  - Viewer  

- Audit logs for sensitive actions:
  - Case updates  
  - Data exports  
  - Investigation edits  

This reflects real internal compliance and accountability needs.

---

## Technical Stack (Locked)

### Backend
- Python  
- Django  
- Django REST Framework  

### Database
- PostgreSQL  

### Frontend
- React  

### Authentication
- JWT or session-based (initially simple, expandable later)

---

## V1 Non-Goals (Out of Scope)

These are intentionally excluded from the first version:

- Real-time streaming ingestion  
- Saved views  
- Notifications  
- Machine learning predictions  
- External integrations  
- Advanced role policies  

These can be introduced in future versions.

---

## Success Criteria

V1 will be considered successful when users can:

- Upload event datasets  
- Search and filter events  
- Create and manage investigation cases  
- Link events to cases  
- View analytics trends  
- Access the system based on roles  
- Maintain audit visibility  

---

## Why This Project Exists

Signaled is designed to demonstrate:

- Backend API design  
- Data modeling  
- Ingestion workflows  
- SQL-driven querying  
- Investigation tooling  
- Secure internal platforms  
- Analytics visualization  
- Full SDLC execution  

It intentionally reflects real engineering environments that rely on internal tools to manage system reliability and investigations.
