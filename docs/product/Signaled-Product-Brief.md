# Signaled — Product Brief

**Document Status:** Draft  
**Date:** September 12, 2026

---

## 1. Product Overview

Signaled is a secure enterprise event and incident investigation platform used to ingest or create operational events, organize them into investigations or cases, manage the investigation lifecycle, analyze related information, and maintain a complete audit trail of activity and decisions.

At its core, Signaled helps organizations turn important operational events into structured, traceable investigations.

Not every event requires an investigation. Organizations may generate or receive large volumes of operational events, but only certain events may warrant escalation into a formal case. A single investigation may also involve multiple related events.

A simplified lifecycle is:

**Operational event → Review / triage → Investigation / case → Evidence and analysis → Findings / resolution → Audit history**

---

## 2. Who Signaled Serves

Signaled is intended for enterprise and public-sector organizations whose operations generate events or incidents that require structured review, investigation, accountability, and historical traceability.

Potential users and teams include:

- Operations teams
- Quality assurance teams
- Safety teams
- Compliance and risk teams
- Security teams
- Investigators and analysts
- Supervisors and managers
- Government agencies
- Regulated organizations

These teams may need to investigate operational failures, policy violations, safety incidents, security-related events, process failures, system anomalies, human error, or other significant events that require formal follow-up.

---

## 3. Problem Statement

Organizations frequently lack a centralized and consistent process for turning operational events into structured investigations.

Relevant information may be fragmented across systems, teams, spreadsheets, email, logs, and individual employees. This fragmentation can make it difficult to:

- Understand what occurred
- Determine contributing factors or responsibility
- Coordinate investigative work
- Document actions and decisions
- Connect related events or incidents
- Identify recurring patterns
- Preserve a reliable history of what was changed, reviewed, decided, and resolved

Without a structured investigation platform, organizations may struggle to maintain accountability, consistency, transparency, and historical traceability across the investigation lifecycle.

---

## 4. Solution

Signaled provides a centralized, secure platform for managing operational events and turning significant incidents into structured, traceable investigations.

It gives teams a consistent place to:

- Create or ingest event records
- Review and triage operational events
- Open and manage investigations or cases
- Assign responsibility
- Collect notes, evidence, and supporting information
- Track investigative actions and status changes
- Search and analyze related information
- Document findings and resolutions
- Preserve a complete audit history

By consolidating these activities into one system, Signaled improves accountability, consistency, transparency, and organizational learning across the investigation lifecycle.

Signaled is not intended to replace every upstream monitoring, security, or operational system. External systems may detect or generate events, while Signaled serves as the structured environment in which those events are reviewed, investigated, documented, and resolved.

---

## 5. MVP Goals

The MVP should demonstrate a complete, production-minded investigation workflow rather than a basic CRUD application.

A successful MVP should allow an organization to:

1. Create or ingest an operational event.
2. Review and triage the event.
3. Escalate an event into a formal investigation or case when appropriate.
4. Associate one or more relevant events with a case.
5. Assign ownership or responsibility for investigative work.
6. Add notes, evidence, attachments, and supporting information.
7. Track investigative activity and status changes.
8. Search and review related events and cases.
9. Document findings and resolution.
10. Maintain a complete and reliable audit trail.
11. Analyze operational information well enough to support trend or pattern identification.
12. Run the application as a secure, deployed cloud-hosted system with production-minded observability, configuration, and background-processing capabilities.

The MVP is intentionally designed to demonstrate stronger engineering depth than a typical student portfolio project.

---

## 6. MVP Cloud and Engineering Capabilities

The following capabilities are intentionally part of the Signaled MVP because they support real product requirements and reinforce production-minded engineering practices.

### Application and Deployment

- **ASP.NET Core / C#** as the primary backend platform
- **Docker** for containerizing the application
- **Azure Container Registry (ACR)** for storing application container images
- **Azure Container Apps** for hosting the containerized application

### Data and Storage

- **Azure SQL + Entity Framework Core** as the primary transactional system of record
- **Azure Blob Storage** for files such as evidence, attachments, and uploaded source or import files where appropriate

### Asynchronous Processing

- **Azure Service Bus** for reliable asynchronous messaging
- **Retry handling and dead-letter queue processing** for failed messages
- **Azure Functions or a background worker** for asynchronous workloads outside the normal request/response cycle

A representative workflow may look like:

**Incident import → Blob Storage → API publishes message → Service Bus → Function / worker → Validate and process → Azure SQL**

Repeated failures should be handled through retry policies and, when necessary, a dead-letter queue so failures can be inspected and reprocessed rather than silently lost.

### Security and Configuration

- **Azure Key Vault** for secrets and sensitive configuration
- **Azure App Configuration** for centralized application and environment configuration
- Strong authentication and authorization controls appropriate for an internal enterprise application

### Observability and Troubleshooting

- **OpenTelemetry**
- **Application Insights / Azure Monitor**
- **Kusto Query Language (KQL)** for operational analysis and troubleshooting
- **Correlation and distributed tracing** across workflows such as API → Service Bus → background processor

### Intelligent Search

- **Semantic / vector search** for a meaningful investigative use case, such as searching incident narratives, case notes, or other investigative text by meaning rather than exact keywords

The exact Azure service used to implement the vector-search capability will be selected later as part of the architecture design.

---

## 7. MVP Boundaries

The MVP will include the capabilities necessary to support the complete investigation lifecycle and the production-minded Azure features listed above.

The project will avoid adding technologies or services solely to increase the size of the technology stack.

The following are not currently required for the MVP unless future product requirements create a legitimate need:

- Kubernetes / Azure Kubernetes Service (AKS)
- Redis
- Unnecessary microservices
- A generic AI chatbot
- Additional AI features that do not directly support the investigation workflow
- Technologies chosen only because they appear on a certification objective

The architecture should remain driven by product requirements and justified engineering decisions.

---

## 8. Product Principle

Signaled should be designed as a serious enterprise-style investigation platform, not as a demonstration of as many technologies as possible.

Every major feature, service, and architectural decision should be explainable in terms of the problem it solves for the product.

The goal of the MVP is to produce a secure, traceable, well-architected, cloud-hosted application that demonstrates strong software-engineering judgment, implementation depth, and the ability to support a realistic operational investigation workflow.
