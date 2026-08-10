# AGENTS.md — FinanceOS

## 1. Purpose

This repository contains **FinanceOS**, a personal and household financial management platform.

The application is designed as a production-grade learning and portfolio project using:

* .NET
* ASP.NET Core
* Angular
* Ionic / Capacitor
* PostgreSQL
* RabbitMQ
* MassTransit
* Redis
* YARP
* Docker
* Azure
* Event-Driven Architecture
* Domain-Driven Design
* CQRS
* Vertical Slice Architecture
* OpenTelemetry

All agents working in this repository MUST respect the architectural, functional, testing, and coding rules defined in this document.

This file has priority over implementation convenience.

Do not bypass these rules simply to make something work faster.

---

# 2. Main Product Goal

FinanceOS allows users and households to manage:

* financial accounts
* expenses
* income
* transfers
* budgets
* categories
* recurring payments
* savings goals
* forecasts
* dashboards
* financial analytics
* notifications

The first MVP focuses on:

1. Authentication
2. Household management
3. Accounts
4. Categories
5. Transactions
6. Budgets
7. Dashboard
8. Notifications

Do not implement future features unless explicitly requested.

---

# 3. Golden Rule

Never generate large amounts of code blindly.

Work incrementally.

For every requested task:

1. inspect the existing repository;
2. understand the current architecture;
3. identify affected modules;
4. propose or internally determine the smallest coherent change;
5. implement it;
6. compile;
7. run relevant tests;
8. fix failures;
9. update documentation if architecture or contracts changed.

Never leave the repository knowingly broken.

---

# 4. Repository Strategy

Prefer a monorepo.

Recommended structure:

```text
financeos/

apps/
  web/
  mobile/

backend/
  gateway/

  services/
    identity/
    finance/
    budget/
    forecast/
    notification/

  building-blocks/

libs/
  frontend/
  contracts/

tests/

docs/

infrastructure/

scripts/
```

Exact directories can evolve when justified.

Do not reorganize the entire repository unless explicitly asked.

---

# 5. Backend Technology

Use:

* current supported .NET version selected by the repository
* ASP.NET Core
* C#
* Entity Framework Core
* PostgreSQL
* MassTransit
* RabbitMQ
* FluentValidation
* OpenTelemetry
* Serilog
* YARP
* Redis where necessary
* Dapper only when beneficial for read-heavy queries

Do not introduce alternative frameworks without a documented reason.

---

# 6. Frontend Technology

Web frontend:

* Angular
* TypeScript strict mode
* standalone components
* Angular Signals
* RxJS
* Angular Material

Mobile:

* Ionic
* Angular
* Capacitor

Prefer shared frontend libraries when business logic or UI can be reused.

Do not duplicate identical domain logic between web and mobile unnecessarily.

---

# 7. Architecture Style

FinanceOS uses a combination of:

* Domain-Driven Design
* Clean Architecture principles
* Vertical Slice Architecture
* Event-Driven Architecture
* CQRS where useful

Do not apply patterns mechanically.

Patterns must solve actual architectural problems.

---

# 8. Microservice Boundaries

Initial services are:

## Identity Service

Responsible for:

* users
* authentication
* households
* household membership
* roles
* preferences

---

## Finance Service

Responsible for:

* accounts
* transactions
* categories
* tags
* recurring transactions during early phases

---

## Budget Service

Responsible for:

* monthly budgets
* budget allocations
* budget consumption
* threshold detection

---

## Forecast Service

Responsible for:

* cash-flow projections
* end-of-month forecasts
* future financial projections

May remain disabled until required by the roadmap.

---

## Notification Service

Responsible for:

* in-app notifications
* email notifications
* push notifications
* notification preferences

---

# 9. Important Microservice Rule

A microservice MUST own its own data.

Never query another service's database directly.

Forbidden example:

```text
Budget Service
   |
   +--> SELECT * FROM finance.transactions
```

Correct alternatives:

* synchronous API call where justified
* asynchronous integration event
* local projection/read model

Database-per-service boundaries must remain explicit even when using a single local PostgreSQL instance.

---

# 10. Domain Layer Rules

The Domain project must contain only domain concepts.

Allowed:

* Aggregates
* Entities
* Value Objects
* Domain Events
* Domain Services
* Specifications when necessary
* Domain Exceptions
* Business Rules

Forbidden dependencies:

* Entity Framework Core
* ASP.NET Core
* RabbitMQ
* MassTransit
* Redis
* HTTP clients
* logging infrastructure
* external SDKs

The Domain layer must remain independently testable.

---

# 11. Application Layer Rules

The Application layer contains use cases.

Typical structure:

```text
Features/
  Transactions/
    CreateTransaction/
      Command.cs
      Validator.cs
      Handler.cs
      Response.cs

    GetTransactions/
      Query.cs
      Handler.cs
      Response.cs
```

Prefer feature-oriented organization over giant folders such as:

```text
Commands/
Queries/
Handlers/
DTOs/
```

when vertical slices provide better cohesion.

---

# 12. Infrastructure Layer

Infrastructure contains technical implementations.

Examples:

* EF Core DbContext
* repository implementations
* message publishing
* message consumers infrastructure
* Redis
* HTTP clients
* blob storage
* authentication infrastructure
* external integrations
* OpenTelemetry exporters

Infrastructure must implement abstractions required by Application or Domain.

---

# 13. API Layer

Use ASP.NET Core APIs.

Prefer Minimal APIs unless existing service conventions use controllers.

Endpoints should remain thin.

Endpoints may:

* parse requests
* invoke application use cases
* map responses
* apply authorization
* return appropriate HTTP responses

Endpoints must NOT contain domain/business logic.

---

# 14. CQRS Rules

CQRS means separating write intent from read intent.

Commands mutate state.

Examples:

```text
CreateTransactionCommand
UpdateTransactionCommand
DeleteTransactionCommand
CreateBudgetCommand
AddBudgetAllocationCommand
```

Queries retrieve data.

Examples:

```text
GetTransactionQuery
GetTransactionsQuery
GetMonthlyBudgetQuery
GetDashboardQuery
```

Do not create unnecessary abstraction layers merely to claim CQRS compliance.

---

# 15. Mediation

If the project uses MediatR or an equivalent mediator, follow existing conventions.

Do not introduce MediatR automatically if the repository already has a simpler dispatch mechanism.

Do not hide business logic inside middleware or pipeline behaviors.

Pipeline behaviors are appropriate for concerns such as:

* validation
* logging
* metrics
* authorization
* transaction boundaries

---

# 16. Money

Money is a critical domain concept.

Never use:

```text
float
double
```

for financial values.

Use:

```text
decimal
```

and preferably a Money Value Object.

Example conceptual model:

```text
Money
  Amount
  Currency
```

Money operations must check currency compatibility.

Avoid implicit currency conversions.

---

# 17. Dates and Time

Backend timestamps should be represented and stored in UTC.

Use types appropriate to the scenario.

Prefer:

```text
DateTimeOffset
```

for timestamps.

Use local dates only for domain concepts that intentionally represent calendar dates without time.

The frontend handles display conversion using the user's timezone.

---

# 18. Household Isolation

Household isolation is a mandatory security boundary.

Every relevant financial resource belongs to a:

```text
HouseholdId
```

Examples:

* Account
* Transaction
* Budget
* Category
* SavingsGoal

Never trust a HouseholdId merely because the client sends it.

The authenticated user's membership must be validated.

An authenticated user must never be able to access resources belonging to a household to which they do not belong.

Add tests specifically for this rule.

---

# 19. Authentication

Use industry-standard mechanisms.

Architecture target:

* OAuth 2.0
* OpenID Connect
* JWT access tokens
* refresh token mechanism where relevant

Do not create custom cryptography.

Do not store passwords directly.

Do not log credentials or tokens.

---

# 20. Authorization

Roles:

```text
Owner
Admin
Member
Viewer
```

Authorization should use policies rather than scattered role checks where practical.

Examples:

```text
CanManageHousehold
CanManageBudget
CanCreateTransaction
CanViewFinance
```

---

# 21. Transactions Domain

Supported transaction types:

```text
Expense
Income
Transfer
Refund
```

Financial effects:

Expense:

```text
account balance -= amount
```

Income:

```text
account balance += amount
```

Transfer:

```text
source -= amount
destination += amount
```

Refund:

```text
account balance += amount
```

All transaction amounts must be strictly positive.

The transaction type determines direction.

Avoid representing expenses as arbitrary negative numbers.

---

# 22. Transfers

Transfers require special care.

A transfer must reference:

* source account
* destination account
* amount
* currency
* execution date

Source and destination cannot be the same account.

A transfer should not count as household income or expense in analytics.

---

# 23. Categories

Categories may be:

* system categories
* user-defined categories

Support optional hierarchy:

```text
Food
  Groceries
  Restaurant
```

System categories must not be deleted by normal users.

Custom categories are scoped to a household.

---

# 24. Budget Domain

A monthly budget belongs to:

```text
Household
Year
Month
```

Budget allocations belong to categories.

Example:

```text
Groceries    500
Transport    150
Restaurant   150
Childcare    400
Pets          80
```

Budget consumption is derived from eligible transactions.

Do not use transfers as budget expenses.

---

# 25. Budget Thresholds

Default threshold concepts:

* 50%
* 75%
* 90%
* 100%

Avoid generating duplicate threshold notifications repeatedly.

If consumption moves from:

```text
49% -> 91%
```

the system must have a deterministic policy about threshold events.

Prefer emitting only meaningful state changes rather than duplicate alerts.

---

# 26. Event-Driven Architecture

Use integration events for communication across service boundaries.

Example flow:

```text
Finance Service
    |
    | TransactionCreated
    v
RabbitMQ
    |
    +--> Budget Service
    |
    +--> Forecast Service
    |
    +--> Notification Service
    |
    +--> Dashboard Projection
```

Do not use integration events for communication inside one aggregate.

---

# 27. Domain Events vs Integration Events

Keep the distinction explicit.

Domain Event:

internal to a bounded context.

Example:

```text
TransactionCreatedDomainEvent
```

Integration Event:

published to other services.

Example:

```text
TransactionCreatedV1
```

Do not publish domain entities directly onto the message broker.

---

# 28. Event Contracts

Contracts must be explicit and versionable.

Recommended naming:

```text
TransactionCreatedV1
TransactionUpdatedV1
BudgetExceededV1
```

Every integration event should contain when relevant:

```text
EventId
OccurredAt
CorrelationId
HouseholdId
```

Do not expose internal persistence entities in contracts.

---

# 29. Event Contract Compatibility

Events are public contracts between services.

Never rename or remove fields from a published event without considering backwards compatibility.

Prefer additive changes.

Breaking changes require a new event version.

---

# 30. Outbox Pattern

Messages related to persistent business changes must be published using the transactional Outbox Pattern.

Expected sequence:

```text
BEGIN DB TRANSACTION

update business data

insert OutboxMessage

COMMIT
```

A background publisher then sends messages to the broker.

Never rely on:

```text
SaveChanges()

then

Publish()
```

for critical integration events.

---

# 31. Inbox / Consumer Idempotency

Message consumers must support safe redelivery.

Use idempotency where processing modifies persistent state.

Recommended metadata:

```text
MessageId
ConsumerName
ProcessedAt
```

A redelivered message must not duplicate its effect.

---

# 32. Eventual Consistency

Do not expect cross-service updates to happen atomically.

Example:

```text
Transaction created
       |
       + 0 ms: Finance updated
       |
       + later: Budget projection updated
       |
       + later: Dashboard updated
```

Design APIs and UI accordingly.

---

# 33. Saga Rules

Do not use a Saga for simple CRUD operations.

Use orchestration/state-machine patterns only for workflows spanning multiple durable steps and requiring compensation.

Possible future use case:

```text
TransferRequested
SourceDebited
DestinationCredited
TransferCompleted
```

If such a workflow is implemented, failure and compensation paths must be explicitly tested.

---

# 34. RabbitMQ and MassTransit

RabbitMQ is the default local message broker.

MassTransit is the default abstraction.

Production may use Azure Service Bus.

Business logic must not depend directly on RabbitMQ-specific concepts when avoidable.

---

# 35. API Gateway

Use YARP for the API gateway.

Expected responsibilities:

* routing
* authentication integration
* correlation identifiers
* rate limiting where applicable
* observability

Do not put core business logic in the gateway.

---

# 36. API Design

APIs should be versioned.

Example:

```text
/api/v1/accounts
/api/v1/transactions
/api/v1/budgets
```

Use consistent REST conventions unless an endpoint represents an explicit command/action.

---

# 37. HTTP Semantics

Use HTTP status codes correctly.

Typical expectations:

```text
200 OK
201 Created
204 No Content
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
422 Unprocessable Entity when appropriate
500 Internal Server Error
```

Do not return `200 OK` for every outcome.

---

# 38. Problem Details

Use RFC 7807 Problem Details for API errors.

Validation errors must be machine-readable.

Avoid custom inconsistent response formats per endpoint.

---

# 39. Validation

Use FluentValidation or the validation mechanism already established.

Validate commands at application boundaries.

Examples:

```text
Amount > 0
Currency required
TransactionDate required
AccountId required
```

Business invariants must still live in the domain layer.

Input validation does not replace domain validation.

---

# 40. Database

Default database:

```text
PostgreSQL
```

Use EF Core migrations.

Migrations must be committed to source control.

Do not enable automatic destructive schema recreation outside test/dev contexts.

---

# 41. Database Naming

Use consistent conventions.

Prefer:

```text
snake_case
```

for PostgreSQL identifiers if already configured.

Do not mix naming styles arbitrarily.

---

# 42. Entity Framework Core

Use EF Core for transactional persistence.

Guidelines:

* use explicit configurations
* define indexes deliberately
* avoid unnecessary eager loading
* use `AsNoTracking()` for read-only queries
* use cancellation tokens
* avoid N+1 query patterns
* inspect generated SQL for critical paths

---

# 43. Dapper

Use Dapper only where it provides clear benefits.

Good candidates:

* dashboards
* reporting
* analytics
* complex read models

Do not duplicate EF Core persistence logic unnecessarily.

---

# 44. Pagination

Collection endpoints must support pagination when datasets can grow.

Recommended response concepts:

```text
items
page
pageSize
totalCount
totalPages
```

Apply limits to page size.

Do not expose unlimited list endpoints.

---

# 45. Filtering and Sorting

When appropriate, support:

```text
search
from
to
category
account
type
sort
direction
```

Never dynamically interpolate raw sort/filter values into SQL.

---

# 46. Redis

Use Redis intentionally.

Appropriate uses:

* cached projections
* distributed cache
* temporary data
* rate-limit state
* distributed coordination if necessary

Redis is not a primary source of truth for financial records.

---

# 47. Frontend Architecture

Angular should be organized by domain feature.

Recommended:

```text
src/app/

core/
shared/

features/
  dashboard/
  accounts/
  transactions/
  budgets/
  notifications/
  settings/
```

Do not create large generic services containing unrelated responsibilities.

---

# 48. Angular Components

Use standalone components.

Prefer smart/container components at feature boundaries and reusable presentational components where useful.

Avoid overly large components.

Extract logic when a component becomes difficult to understand or test.

---

# 49. Angular Signals

Prefer Signals for local reactive state.

Use RxJS for asynchronous streams and event composition when appropriate.

Do not convert everything to RxJS.

Do not convert everything to Signals.

Use the model best suited to the use case.

---

# 50. State Management

Do not add a large global store by default.

Start with:

* Signals
* feature services
* RxJS

Use NgRx Signal Store if feature complexity justifies it.

The need for global state must be demonstrated by actual application behavior.

---

# 51. HTTP Layer

Centralize:

* API base URL
* authentication token handling
* error handling
* correlation ID propagation where possible

Use typed API clients or clear feature-level data services.

---

# 52. Angular Routing

Use lazy-loaded routes for major features.

Protect authenticated routes.

Apply authorization checks when routes require household permissions.

Frontend authorization is only UX protection.

Backend authorization remains mandatory.

---

# 53. UI and Design

Use Angular Material as the base component library unless the repository specifies another.

Application requirements:

* responsive
* desktop friendly
* mobile friendly
* accessible
* light mode
* dark mode

Financial numbers must be visually clear.

Negative/positive states should not depend solely on color.

---

# 54. Mobile

Use Ionic and Capacitor.

Priority mobile flows:

1. login
2. dashboard
3. add expense
4. add income
5. account overview
6. budget progress
7. notifications

Do not attempt to replicate every advanced desktop feature immediately.

---

# 55. Shared Frontend Code

Share appropriate code between Angular web and Ionic mobile.

Candidates:

* domain models
* API clients
* authentication
* validation helpers
* formatters
* state logic
* design tokens
* selected UI components

Do not force sharing when platform UX needs different implementations.

---

# 56. Real-Time Updates

Use SignalR only for scenarios where real-time feedback adds value.

Examples:

* budget updated
* notification received
* dashboard projection refreshed

The system must continue to work correctly without an active SignalR connection.

SignalR is not the source of truth.

---

# 57. Observability

Every backend service must expose telemetry.

Use OpenTelemetry for:

* traces
* metrics
* logs integration where appropriate

Trace propagation should work across:

```text
HTTP
message broker
background workers
```

---

# 58. Correlation

Maintain correlation identifiers across distributed flows.

Expected concepts:

```text
TraceId
CorrelationId
MessageId
CausationId when useful
```

A TransactionCreated flow should be traceable across services.

---

# 59. Logging

Use structured logging.

Prefer:

```text
logger.LogInformation(
    "Transaction {TransactionId} created for household {HouseholdId}",
    transactionId,
    householdId);
```

Do not use:

```text
logger.LogInformation($"Transaction {transactionId}...");
```

when structured logging can be used.

---

# 60. Sensitive Data Logging

Never log:

* passwords
* access tokens
* refresh tokens
* authentication secrets
* private keys
* full payment credentials

Avoid logging unnecessary personally identifiable information.

---

# 61. Health Checks

Services should expose:

```text
/health
/health/live
/health/ready
```

Readiness may check critical dependencies.

Liveness should not become dependent on every external service.

---

# 62. Docker

Every deployable backend service should have a Dockerfile.

Local infrastructure should be runnable via Docker Compose or equivalent repository tooling.

Expected local components may include:

```text
PostgreSQL
RabbitMQ
Redis
Seq
OpenTelemetry Collector
backend services
gateway
```

Frontend dev servers do not need to run in Docker unless useful.

---

# 63. Docker Rules

Use multi-stage builds.

Do not include development secrets in images.

Use `.dockerignore`.

Run containers as non-root when practical.

Keep images minimal.

---

# 64. Configuration

Use environment-based configuration.

Examples:

```text
appsettings.json
appsettings.Development.json
environment variables
```

Never hardcode production secrets.

Production secrets should ultimately be compatible with Azure Key Vault.

---

# 65. Local Development

The repository must provide a simple documented startup workflow.

Goal:

```text
git clone
configuration
docker compose up
run backend
run frontend
```

Avoid undocumented manual dependency setup.

---

# 66. Testing Philosophy

Tests must validate behavior, not implementation trivia.

Required test categories:

* unit tests
* domain tests
* integration tests
* architecture tests
* API tests where useful
* frontend tests
* end-to-end tests for critical flows

---

# 67. Unit Tests

Use:

* xUnit
* FluentAssertions or the project's chosen assertion library
* NSubstitute or Moq only when necessary

Do not mock Value Objects or simple domain objects.

Prefer testing the real domain behavior.

---

# 68. Integration Tests

Use Testcontainers whenever practical.

Possible containers:

```text
PostgreSQL
RabbitMQ
Redis
```

Integration tests should be reproducible on a clean machine.

Do not require developers to manually start a test database.

---

# 69. Architecture Tests

Create tests for important dependency rules.

Examples:

```text
Domain does not reference Infrastructure

Domain does not reference API

Application does not reference Infrastructure implementation details

Microservice A does not reference Microservice B's persistence layer
```

---

# 70. End-to-End Testing

Use Playwright for critical browser flows.

First required E2E scenario:

```text
Login
Create household
Create account
Create category
Create budget
Create expense
Verify dashboard
Verify budget consumption
```

Keep E2E coverage focused on high-value journeys.

---

# 71. Test Data

Test fixtures must be deterministic.

Do not depend on production data.

Do not hardcode tests around current dates unless using an injected clock/time abstraction.

---

# 72. Time Abstraction

For business logic depending on the current time, prefer an injectable time provider.

This enables deterministic testing.

Do not scatter:

```csharp
DateTime.UtcNow
```

through domain/application logic.

---

# 73. Security Tests

Add tests for:

* accessing another household
* insufficient role
* unauthenticated access
* invalid token where integration scope permits
* forged HouseholdId
* archived/inactive resource access where applicable

---

# 74. Performance

Avoid premature optimization.

However:

* paginate growing collections
* index frequent query fields
* avoid N+1 SQL
* avoid loading entire transaction histories unnecessarily
* use projections for dashboards
* cache only when justified

Measure before adding complex optimizations.

---

# 75. Dashboard Read Model

The dashboard must not require expensive cross-service joins on every page load.

Prefer projection/read models.

Example:

```text
MonthlyFinanceSummary

HouseholdId
Year
Month
Income
Expenses
Savings
SavingsRate
CurrentBalance
```

Update projections from integration events.

---

# 76. Projection Rebuild

Whenever practical, projections should be rebuildable.

Do not make business source data depend exclusively on projection tables.

Read models are derived data.

---

# 77. CI

Every pull request should eventually run:

```text
restore/install
format/lint checks
build
unit tests
integration tests where feasible
frontend tests
architecture tests
```

Do not merge knowingly broken code.

---

# 78. CD

Deployment targets may use Azure.

Target architecture may include:

* Azure Container Apps
* Azure Service Bus
* Azure Database for PostgreSQL
* Azure Cache for Redis
* Azure Key Vault
* Application Insights
* Azure Blob Storage

Do not implement cloud deployment before local architecture is stable unless explicitly requested.

---

# 79. Formatting

Backend:

follow `.editorconfig`.

Frontend:

use repository ESLint and formatter configuration.

Do not manually introduce a formatting style inconsistent with the repository.

---

# 80. C# Rules

Use modern C#.

Enable nullable reference types.

Prefer:

* records for immutable DTOs/events
* sealed classes where inheritance is not intended
* async APIs for I/O
* cancellation tokens
* dependency injection
* explicit domain behavior

Avoid:

* public mutable fields
* service locator
* unnecessary static state
* giant God services
* primitive obsession for important domain concepts

---

# 81. Async Rules

Do not use:

```text
.Result
.Wait()
```

on asynchronous code.

Use async/await end-to-end.

Pass CancellationToken through I/O operations where practical.

---

# 82. Exception Handling

Exceptions should represent exceptional conditions.

Do not use exceptions as regular control flow.

Map known errors to meaningful API responses.

Unexpected errors should be logged and returned as generic server errors without leaking internal information.

---

# 83. Naming

Use explicit business-oriented names.

Good:

```text
CreateTransaction
BudgetAllocation
SavingsGoal
HouseholdMembership
```

Avoid vague names:

```text
Manager
Helper
Utils
Processor
CommonService
```

unless the responsibility is genuinely clear.

---

# 84. Comments

Do not add comments that merely restate obvious code.

Use comments for:

* business rationale
* non-obvious constraints
* architectural trade-offs
* unusual edge cases

Prefer expressive code.

---

# 85. Dependency Management

Before adding a new NuGet/npm package:

1. verify whether the problem can be solved using existing dependencies;
2. confirm the library is maintained and appropriate;
3. avoid redundant packages;
4. use a current compatible stable release;
5. document important architectural dependencies.

Do not add dependencies casually.

---

# 86. Package Versions

Do not invent package versions.

When adding dependencies, use versions compatible with the project's selected framework/runtime.

Update lockfiles where applicable.

---

# 87. Documentation

Important architecture must be documented.

Expected documents:

```text
README.md
docs/ARCHITECTURE.md
docs/DEVELOPMENT.md
docs/EVENTS.md
docs/SECURITY.md
docs/adr/
```

Update documentation when a change makes it inaccurate.

---

# 88. ADR

Create Architecture Decision Records for important decisions.

Examples:

```text
ADR-001-use-postgresql.md
ADR-002-use-rabbitmq-masstransit.md
ADR-003-use-angular-ionic.md
ADR-004-use-outbox-pattern.md
ADR-005-database-per-service.md
```

Do not create ADRs for trivial code changes.

---

# 89. Mermaid Diagrams

Prefer Mermaid for diagrams committed as text.

Useful diagrams:

* system context
* container architecture
* transaction flow
* event flow
* budget update flow
* deployment architecture

Keep diagrams synchronized with actual architecture.

---

# 90. Git Changes

Keep changes scoped.

Do not modify unrelated files.

Do not reformat the entire repository during a feature implementation.

Do not rename broad areas without need.

Generated artifacts must not be committed unless repository policy requires them.

---

# 91. Before Editing Code

Before making significant changes:

* inspect nearby code
* identify existing conventions
* inspect tests
* inspect dependency injection
* inspect configuration
* inspect database mappings
* inspect existing shared abstractions

Reuse established patterns when they are sound.

---

# 92. When Requirements Are Ambiguous

Prefer the simplest implementation consistent with:

* this file
* existing architecture
* current product scope

Do not introduce speculative features.

Document any meaningful assumption in the implementation summary.

---

# 93. Refactoring Rule

Refactor when necessary to implement the requested feature cleanly.

Do not perform large unrelated refactors.

If substantial architectural debt blocks correct implementation, make the minimum structural correction required.

---

# 94. No Fake Implementations

Do not silently implement production features using fake in-memory infrastructure unless explicitly creating a prototype.

Examples of unacceptable shortcuts:

* fake authentication presented as finished auth
* in-memory event broker presented as RabbitMQ implementation
* local arrays replacing persistence without explanation
* mocked APIs wired into the production application

Test doubles belong in tests.

---

# 95. No Placeholder Business Logic

Avoid committing behavior such as:

```text
TODO: calculate budget later
return 0;
```

when the task claims the functionality is completed.

If something is intentionally unfinished, clearly document it.

---

# 96. Build Before Completion

Before considering a coding task completed:

Backend changes:

```text
dotnet build
```

and relevant tests must succeed.

Frontend changes:

run the repository's Angular build/lint/test commands.

Do not report success without verifying available checks.

---

# 97. Failure Handling

If a build or test fails:

1. inspect the actual failure;
2. identify whether it was introduced by the change;
3. fix issues caused by the change;
4. rerun tests.

Do not disable failing tests simply to obtain a green build.

---

# 98. Migration Rule

When persistence schema changes:

* create a migration
* verify migration generation
* ensure the app starts against a clean database
* consider existing data compatibility

Never manually edit production schema as part of application code.

---

# 99. Seed Data

Seed data may include:

* default categories
* local demo data

Production seed logic must be safe and idempotent.

Do not automatically seed fake financial transactions for real users.

---

# 100. First Vertical Slice

The first complete vertical slice must prove the architecture.

Scenario:

```text
User authenticated
        |
        v
Create household
        |
        v
Create account
        |
        v
Create category
        |
        v
Create monthly budget
        |
        v
Create expense
        |
        v
Finance Service persists transaction
        |
        v
Outbox stores TransactionCreatedV1
        |
        v
MassTransit publishes event
        |
        v
Budget Service consumes event
        |
        v
Budget read model updates
        |
        v
Dashboard reflects the expense
```

This scenario must have automated test coverage.

---

# 101. Preferred Implementation Order

Unless instructed otherwise, build the product in this order.

## Phase 0 — Foundation

* repository structure
* backend solution
* frontend workspace
* Docker Compose
* PostgreSQL
* RabbitMQ
* observability basics
* coding standards
* CI skeleton

## Phase 1 — Identity

* authentication
* user
* household
* membership
* roles

## Phase 2 — Finance Core

* accounts
* categories
* transactions
* transfers

## Phase 3 — Messaging

* MassTransit
* RabbitMQ
* integration contracts
* Outbox
* idempotent consumers

## Phase 4 — Budgets

* monthly budget
* allocations
* consumption
* thresholds

## Phase 5 — Dashboard

* financial summaries
* budget summary
* transaction overview
* event-driven projections

## Phase 6 — Notifications

* in-app notifications
* budget alerts

## Phase 7 — Recurring Payments

* recurring expenses
* recurring income
* due dates

## Phase 8 — Forecast

* end-of-month projection
* 30-day projection
* 90-day projection

## Phase 9 — Mobile

* Ionic app
* core finance flows
* notifications

## Phase 10 — Cloud

* Azure
* production Service Bus
* Container Apps
* managed PostgreSQL
* Key Vault
* Application Insights

Do not skip foundational phases by generating advanced features prematurely.

---

# 102. Definition of Done

A feature is considered complete only if applicable criteria are satisfied:

* requirements implemented
* code compiles
* relevant tests pass
* input validation exists
* domain rules are protected
* authorization is enforced
* errors use standard response formats
* persistence migration exists when needed
* events use correct contracts
* Outbox is respected where required
* consumers are idempotent
* observability is included
* documentation is updated
* no known regression was introduced

---

# 103. Codex Task Execution Format

For non-trivial tasks, operate using this sequence.

## Step 1 — Inspect

Read relevant files and understand the current implementation.

## Step 2 — Scope

Identify exactly what needs changing.

## Step 3 — Implement

Make the smallest coherent production-ready change.

## Step 4 — Verify

Build and run relevant tests.

## Step 5 — Report

Summarize:

* files changed
* architectural decisions
* tests executed
* remaining limitations

Do not provide a completion claim if verification failed.

---

# 104. When Starting From an Empty Repository

If the repository is empty, do not build every microservice immediately.

Start with foundation only.

Create:

```text
FinanceOS.sln

backend/
apps/
docs/
infrastructure/
```

Then establish:

* build
* test
* Docker
* conventions

After foundation works, implement one service at a time.

---

# 105. Initial Codex Assignment

When instructed to start the project, the initial assignment should be limited to:

```text
Create the FinanceOS repository foundation according to AGENTS.md.

Requirements:

- create the .NET backend solution structure
- create Angular web workspace
- prepare Ionic mobile placeholder structure only if useful
- create initial service directories
- configure PostgreSQL
- configure RabbitMQ
- configure Docker Compose
- add OpenTelemetry foundation
- add Serilog foundation
- create basic health checks
- add README
- add ARCHITECTURE.md
- create initial ADRs
- ensure everything builds

Do NOT implement business features yet.

At the end:

- run backend build
- run frontend build
- report the exact commands executed
- report any unresolved issue
```

---

# 106. Core Principle

FinanceOS must remain understandable.

Prefer:

```text
simple + explicit + tested
```

over:

```text
clever + abstract + difficult to maintain
```

Architecture exists to support the product.

Do not turn the project into a collection of patterns with no business justification.
