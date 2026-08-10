# ADR-001: Use PostgreSQL

## Status
Accepted

## Context
FinanceOS needs reliable relational persistence for financial records and service-owned databases.

## Decision
Use PostgreSQL as the default local and production relational database.

## Consequences
Each microservice will own its logical database. EF Core migrations will be committed when persistence is introduced.
