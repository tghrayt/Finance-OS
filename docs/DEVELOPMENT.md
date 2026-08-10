# Development

## Backend

```powershell
dotnet restore FinanceOS.slnx
dotnet build FinanceOS.slnx
dotnet test FinanceOS.slnx
```

## Identity database

Identity persistence uses EF Core with PostgreSQL and the `identity` schema.

Generate a migration:

```powershell
dotnet ef migrations add MigrationName --project backend\services\identity\FinanceOS.Identity.Infrastructure\FinanceOS.Identity.Infrastructure.csproj --startup-project backend\services\identity\FinanceOS.Identity.Api\FinanceOS.Identity.Api.csproj --context IdentityDbContext --output-dir Persistence\Migrations
```

Apply migrations locally:

```powershell
dotnet ef database update --project backend\services\identity\FinanceOS.Identity.Infrastructure\FinanceOS.Identity.Infrastructure.csproj --startup-project backend\services\identity\FinanceOS.Identity.Api\FinanceOS.Identity.Api.csproj --context IdentityDbContext
```

In local development and Docker Compose, Identity migrations are applied on startup when:

```text
Identity:ApplyMigrationsOnStartup=true
```

Keep this disabled in production unless a deployment step explicitly controls it.

Identity readiness checks validate PostgreSQL connectivity through `/health/ready`. Liveness stays independent through `/health/live`.

## Identity API

Phase 1 exposes the first Identity endpoints:

```text
POST /api/v1/identity/users
GET /api/v1/identity/users/{userId}
GET /api/v1/identity/users/me
PUT /api/v1/identity/users/{userId}/profile
POST /api/v1/identity/households
GET /api/v1/identity/households/{householdId}
GET /api/v1/identity/households/current?userId={userId}
POST /api/v1/identity/households/{householdId}/members?actorUserId={actorUserId}
PUT /api/v1/identity/households/{householdId}/members/{userId}/role?actorUserId={actorUserId}
DELETE /api/v1/identity/households/{householdId}/members/{userId}?actorUserId={actorUserId}
```

The `userId` and `actorUserId` query parameters are temporary development bridges until JWT authentication provides the authenticated user id.
Household member management is still checked against the target household: only Owner and Admin members can add members, change roles or remove members.

Local Docker Compose disables endpoint authorization with:

```text
Authentication__Jwt__RequireAuthorization=false
```

Production should set:

```text
Authentication__Jwt__RequireAuthorization=true
Authentication__Jwt__Authority=https://your-oidc-provider
Authentication__Jwt__Audience=financeos-api
```

## Web app

## Finance database and API

Finance persistence uses EF Core with PostgreSQL and the `finance` schema.

Generate a migration:

```powershell
dotnet ef migrations add MigrationName --project backend\services\finance\FinanceOS.Finance.Infrastructure\FinanceOS.Finance.Infrastructure.csproj --startup-project backend\services\finance\FinanceOS.Finance.Api\FinanceOS.Finance.Api.csproj --context FinanceDbContext --output-dir Persistence\Migrations
```

Phase 2 exposes:

```text
POST /api/v1/finance/accounts
GET /api/v1/finance/accounts?householdId={householdId}
POST /api/v1/finance/categories
GET /api/v1/finance/categories?householdId={householdId}
POST /api/v1/finance/transactions
GET /api/v1/finance/transactions?householdId={householdId}&page=1&pageSize=50
```

Phase 3 writes `TransactionCreatedV1` to `finance.outbox_messages` in the same database transaction. The background outbox publisher publishes pending events to RabbitMQ through MassTransit.

```powershell
npm install
npm --workspace apps/web run build
```

## Docker infrastructure

```powershell
docker compose up -d postgres rabbitmq redis seq otel-collector
```

## Phase discipline

Phase 0 must not include domain entities, business workflows, persistence migrations, authentication flows or finance features. Those start in later phases.
