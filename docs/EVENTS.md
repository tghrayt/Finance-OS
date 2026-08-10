# Events

FinanceOS uses explicit versioned integration events between services.

Event contracts live in `backend/contracts/FinanceOS.Contracts` and must include relevant metadata such as:

- EventId
- OccurredAt
- CorrelationId
- HouseholdId

Domain events and integration events must remain distinct.

## Finance

`TransactionCreatedV1` is written by Finance Service through the transactional outbox when a transaction is persisted.

Consumers such as Budget, Forecast, Notification and future Dashboard projections must treat this event as eventually consistent and idempotent.

Fields:

- `EventId`
- `OccurredAt`
- `CorrelationId`
- `TransactionId`
- `HouseholdId`
- `AccountId`
- `DestinationAccountId`
- `Type`
- `Amount`
- `Currency`
- `CategoryId`
- `TransactionDate`
