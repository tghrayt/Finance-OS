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

## Budget

Budget Service consumes `TransactionCreatedV1` idempotently through its inbox. Expense transactions with a category update the matching monthly budget allocation.

When an allocation crosses meaningful consumption levels, Budget writes events through its transactional outbox:

- `BudgetThresholdReachedV1` for 50%, 75%, 90% and 100%
- `BudgetExceededV1` when consumption moves above 100%

Budget threshold fields:

- `EventId`
- `OccurredAt`
- `CorrelationId`
- `HouseholdId`
- `BudgetId`
- `CategoryId`
- `Threshold`
- `PlannedAmount`
- `ActualAmount`
- `Currency`

Budget exceeded fields:

- `EventId`
- `OccurredAt`
- `CorrelationId`
- `HouseholdId`
- `BudgetId`
- `CategoryId`
- `PlannedAmount`
- `ActualAmount`
- `Currency`

## Notification

Notification Service consumes Budget events idempotently through its inbox:

- `BudgetThresholdReachedV1`
- `BudgetExceededV1`

Each consumed event creates one in-app notification for the related household. Notifications are queryable through `/api/v1/notification/in-app` and can be marked as read through `/api/v1/notification/in-app/{notificationId}/read`.
