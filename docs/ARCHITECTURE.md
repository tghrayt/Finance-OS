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

Initial service boundaries are prepared but not implemented:

- Identity
- Finance
- Budget
- Forecast
- Notification

Each service will own its persistence in later phases. No service may directly query another service database.

## Gateway

The API gateway uses YARP and loads route configuration from `appsettings.json`.

## Frontend foundation

The web app is Angular 21 with standalone components and Angular Material installed. It currently contains a foundation landing shell only.
