# Development

## Backend

```powershell
dotnet restore FinanceOS.slnx
dotnet build FinanceOS.slnx
dotnet test FinanceOS.slnx
```

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
