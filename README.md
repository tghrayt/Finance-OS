# FinanceOS

FinanceOS is a personal and household financial management platform built as a production-grade learning and portfolio project.

## Phase 0 scope

This repository currently contains foundation only:

- .NET solution and backend service shells
- Angular web application shell
- Ionic mobile placeholder
- Docker Compose local infrastructure
- PostgreSQL, RabbitMQ, Redis, Seq and OpenTelemetry Collector definitions
- logging, health checks and OpenTelemetry foundation
- documentation, CI skeleton and VM deployment skeleton

No business feature is implemented in Phase 0.

## Requirements

- .NET SDK 10.x
- Node.js 20.19.x or newer compatible with Angular 21
- npm 11.x
- Docker Desktop for local infrastructure

## Common commands

```powershell
dotnet build FinanceOS.slnx
dotnet test FinanceOS.slnx
npm --workspace apps/web run build
npm run build
```

## Local infrastructure

```powershell
docker compose up -d postgres rabbitmq redis seq otel-collector
```

The application services can also be started through Docker Compose after images are built.

## Deployment target

Production deployment is designed to be automated from GitHub Actions to a private virtual machine over SSH. The VM deployment workflow is intentionally only a Phase 0 skeleton until the VM host, user, SSH key and runtime paths are configured.
