# ADR-005: Database per Service Boundary

## Status
Accepted

## Context
Microservices need explicit ownership and isolation.

## Decision
Each service owns its data and must not query another service database directly.

## Consequences
Cross-service data needs synchronous APIs, integration events, or local projections.
