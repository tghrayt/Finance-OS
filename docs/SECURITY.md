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
