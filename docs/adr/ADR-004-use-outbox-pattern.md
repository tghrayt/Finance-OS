# ADR-004: Use Outbox Pattern

## Status
Accepted

## Context
Financial changes and integration events must not become inconsistent when persistence succeeds but broker publishing fails.

## Decision
Use the transactional Outbox Pattern for business changes that publish integration events.

## Consequences
Outbox tables and publishers will be added when the first event-producing vertical slice is implemented.
