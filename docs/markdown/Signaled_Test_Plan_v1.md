# Signaled — Test Plan (V1)

## Version
v1

## Project
Signaled

## Phase
SDLC Planning / Quality Assurance

---

# 1. Purpose

This Test Plan defines the overall testing strategy for the Signaled platform. It outlines the testing scope, objectives, environments, roles, methodologies, and defect management process used to ensure system reliability and correctness.

**Signaled is a full-stack internal investigation and reporting platform designed to ingest, store, analyze, and track operational event data. The system enables users to investigate incidents, link related events to cases, document findings, and analyze operational trends through analytics dashboards.**

This document establishes the quality assurance framework used during development and prior to release.

---

# 2. Testing Objectives

The primary objectives of testing Signaled are:

- Verify core system functionality behaves as expected
- Ensure data ingestion pipelines correctly validate and store event records
- Validate investigation workflows using cases and linked events
- Confirm role-based access control (RBAC) security policies
- Validate API endpoints and data integrity
- Ensure system stability under normal operational conditions

Testing aims to detect defects early and verify that system requirements are met.

---

# 3. Scope of Testing

## In Scope

### Event Ingestion
- CSV dataset uploads
- JSON dataset uploads
- Data validation and normalization
- Rejection handling for malformed records

### Event Explorer
- Event listing
- Filtering by severity, component, environment, and time range
- Pagination and sorting
- Event detail view

### Case Investigation Workflow
- Creating investigation cases
- Updating case metadata
- Linking events to cases
- Case status transitions
- Adding case notes
- Tagging cases

### Analytics & Reporting
- Event trends over time
- Severity distribution
- Top components
- Recurring pattern analysis

### Authentication & Authorization
- JWT Bearer authentication
- Role-based access enforcement
- Protected API endpoints

### Audit Logging
- Logging sensitive operational actions
- Retrieval of audit history

---

## Out of Scope (V1)

The following are not included in the initial testing scope:

- Performance benchmarking at production scale
- Automated penetration testing
- Third‑party integrations
- Distributed ingestion pipelines
- Load testing of large-scale event ingestion

These may be addressed in future versions.

---

# 4. Test Types

## Functional Testing

Verifies system features behave according to specifications.

Examples:

- Event ingestion stores valid records
- Case creation workflow operates correctly
- Event-to-case linking functions properly
- Analytics endpoints return expected results

---

## API Testing

Validates REST API endpoints.

Focus areas:

- Request validation
- Response formatting
- HTTP status codes
- Authorization behavior

Testing tools may include:

- Postman
- Curl
- Automated integration tests

---

## Security Testing

Ensures authentication and authorization behave correctly.

Includes:

- JWT authentication validation
- Role-based access enforcement
- Prevention of unauthorized API access
- Protection of restricted endpoints

---

## Data Validation Testing

Confirms that system data is stored and retrieved correctly.

Includes:

- Database integrity
- Event ingestion normalization
- Case-event relationships
- Tag relationships
- Audit logging consistency

---

# 5. Test Environment

## Development Environment

Used for initial feature testing.

Components:

- ASP.NET Core Web API backend
- SQL Server database
- Local ingestion testing
- React frontend (if applicable)

Typical setup:

Local development workstation.

---

## Test Environment

Used for structured test execution with controlled datasets.

Components:

- Predefined test datasets
- Mock ingestion files
- Controlled user accounts
- Isolated test database

---

# 6. Test Data

Test data will include:

### Valid Event Data

Datasets following the expected ingestion schema.

Example fields:

- occurredAtUtc
- severity
- component
- environment
- systemId
- message

### Invalid Event Data

Datasets designed to trigger validation failures.

Examples:

- Missing required fields
- Invalid severity values
- Malformed timestamps
- Incorrect JSON structure

### Investigation Data

Test scenarios including:

- Linked events
- Tagged cases
- Case notes
- Status transitions

---

# 7. Entry Criteria

Testing begins once the following conditions are met:

- Core API endpoints implemented
- Database schema deployed
- Authentication configured
- Test datasets prepared
- Application builds successfully

---

# 8. Exit Criteria

Testing concludes when:

- All critical test cases pass
- No open high-severity defects remain
- Core investigation workflows are verified
- API endpoints respond with correct results

Remaining minor defects must be documented in the defect log.

---

# 9. Roles & Responsibilities

## Developer

Responsible for:

- Writing application code
- Performing initial validation
- Fixing reported defects

## Tester / QA Role

Responsible for:

- Executing test cases
- Logging defects
- Verifying fixes

For this project environment, the same individual may fulfill both roles.

---

# 10. Defect Management

Defects discovered during testing will be tracked in a **Defect Log spreadsheet**.

Each defect record will include:

- Defect ID
- Severity level
- Steps to reproduce
- Expected result
- Actual result
- Status
- Resolution notes

Severity levels:

- Critical — System unusable
- High — Major functionality broken
- Medium — Partial functionality failure
- Low — Minor issue

---

# 11. Test Deliverables

Testing produces the following artifacts:

- Test Plan (this document)
- Test Cases spreadsheet
- Defect Log spreadsheet
- Test execution notes

These artifacts support traceability and validation of system quality.

---

# 12. Risk Considerations

Potential testing risks include:

- Incomplete test data coverage
- Edge cases in event ingestion
- Authorization rule misconfiguration
- Database relationship inconsistencies

Mitigation strategies include expanded test datasets and targeted API validation.

---

# 13. Next SDLC Artifact

Next document:

**Test Cases (Spreadsheet)**

This document will contain step-by-step validation procedures for system functionality.
