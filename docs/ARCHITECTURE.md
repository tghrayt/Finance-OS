# Architecture

FinanceOS uses a monorepo with separate backend, frontend, infrastructure and documentation areas.

```text
apps/
  web/                  Angular web application
  mobile/               Ionic/Capacitor placeholder
backend/
  gateway/              YARP API gateway
  services/             Service shells
  building-blocks/      Shared cross-cutting backend foundation
infrastructure/         Local infrastructure configuration
tests/                  Backend foundation tests
docs/                   Architecture and project documentation
```

## Backend foundation

Backend services are ASP.NET Core applications. Phase 0 creates only service shells with:

- `/health`
- `/health/live`
- `/health/ready`
- root diagnostic endpoint
- Serilog console logging
- OpenTelemetry traces and metrics when `OTEL_EXPORTER_OTLP_ENDPOINT` is configured

## Service boundaries

Initial service boundaries are prepared:

- Identity
- Finance
- Budget
- Forecast
- Notification

Each service will own its persistence in later phases. No service may directly query another service database.

## Identity service

Identity has started Phase 1 with explicit Domain and Application projects:

```text
backend/services/identity/
  FinanceOS.Identity.Api/
  FinanceOS.Identity.Application/
  FinanceOS.Identity.Domain/
```

The Domain layer currently owns user, household, membership and role invariants. Authentication provider integration remains intentionally outside the Domain layer and will be added through Application and Infrastructure increments.

## Gateway

The API gateway uses YARP and loads route configuration from `appsettings.json`.

## Frontend foundation

The web app is Angular 21 with standalone components and Angular Material installed. It currently contains a foundation landing shell only.
