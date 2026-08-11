# Security

FinanceOS security requirements include:

- OAuth2 / OpenID Connect authentication
- JWT validation
- household isolation
- policy-based authorization
- no secrets in source control
- RFC 7807 problem details for API errors
- secure logging without tokens, passwords or private financial details

Phase 0 only establishes the project structure. Authentication and authorization begin in Phase 1.

Phase 1 authentication targets OpenID Connect with JWT access tokens and refresh tokens suitable for both Angular web and Ionic mobile clients. FinanceOS must not implement custom password storage or custom cryptography.

Identity APIs are prepared for JWT bearer validation through:

- `Authentication:Jwt:RequireAuthorization`
- `Authentication:Jwt:Authority`
- `Authentication:Jwt:Audience`

Until a real OIDC provider is configured, local development may disable endpoint authorization, but household membership mutations still validate the actor's membership and role inside the target household.

## Frontend session

The Angular web app contains a temporary demo session shell so the dashboard can be exercised before OIDC is wired end to end.

- Demo mode stores only the selected email and the demo household id in browser storage.
- Demo mode does not create or validate credentials.
- The HTTP auth interceptor is ready to attach a bearer token when the real OIDC/JWT flow provides one.
- Backend authorization and household membership validation remain the mandatory security boundary.
