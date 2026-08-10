# ADR-002: Use RabbitMQ and MassTransit Locally

## Status
Accepted

## Context
FinanceOS targets event-driven communication between services.

## Decision
Use RabbitMQ as the local broker. MassTransit will be introduced when messaging behavior begins.

## Consequences
Phase 0 provides RabbitMQ infrastructure only. No business event is published yet.
