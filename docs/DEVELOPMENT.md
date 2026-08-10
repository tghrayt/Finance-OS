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

## Web app

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
