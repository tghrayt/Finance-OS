# ADR-006: Use OIDC and JWT Authentication

## Status
Accepted

## Context
FinanceOS needs authentication for a responsive Angular web app and a future Ionic/Capacitor mobile app. The platform must support household isolation, role-based permissions and secure API access without custom cryptography.

## Decision
Use OAuth 2.0 and OpenID Connect as the authentication architecture, with JWT access tokens for API authorization and refresh tokens where relevant for client sessions.

The first implementation should keep the Identity service compatible with an external OIDC provider or a standards-based identity server. FinanceOS must not store passwords directly or introduce custom token cryptography.

## Consequences
Backend APIs will be designed around bearer token validation, authenticated user identifiers and policy-based authorization. Web and mobile clients can share the same authentication model while keeping platform-specific login UX.
