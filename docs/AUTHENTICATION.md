# FinanceOS Authentication

FinanceOS uses OpenID Connect with Microsoft Entra External ID for customer authentication.
The application must never store customer passwords. Passwords, email verification,
Google sign-in and Microsoft sign-in are handled by the identity provider.

## Sign-in Methods

The web app presents four entry points:

- Email + password: recommended for long-term FinanceOS accounts. Email verification is configured in the Entra sign-up/sign-in user flow.
- Email code: passwordless access with a one-time passcode sent by email.
- Google: social sign-in through a Google identity provider connected to the Entra external tenant.
- Microsoft: sign-in with a Microsoft personal account or a Microsoft Entra work/school identity provider, depending on the tenant configuration.

All methods must resolve to one FinanceOS user profile. The backend should use the external subject claim as the stable identity key and then attach that identity to a household.

## Runtime Web Configuration

The Angular app loads public auth settings from:

```text
/config/auth-config.json
```

This file contains no secrets:

```json
{
  "enabled": true,
  "authority": "https://<tenant-subdomain>.ciamlogin.com/<tenant-id-or-domain>/<user-flow-name>",
  "clientId": "<spa-application-client-id>",
  "redirectUri": "https://financeos.51-210-40-78.sslip.io/",
  "postLogoutRedirectUri": "https://financeos.51-210-40-78.sslip.io/",
  "knownAuthorities": ["<tenant-subdomain>.ciamlogin.com"],
  "scopes": ["openid", "profile", "email", "api://<api-application-client-id>/financeos.access"]
}
```

In Kubernetes, the file is served from the `financeos-web-auth-config` ConfigMap.
Until real values are available, `enabled` remains `false` and the app keeps the explicit demo access.

## Entra External ID Setup

Create an external tenant, then configure:

1. A sign-up/sign-in user flow with local account email + password.
2. Email verification before account completion.
3. Email one-time passcode if passwordless access is desired.
4. Google as a social identity provider.
5. Microsoft or Microsoft Entra as an identity provider.
6. A SPA app registration for the Angular web app.
7. A protected API app registration for FinanceOS backend scopes.

Production redirect URI:

```text
https://financeos.51-210-40-78.sslip.io/
```

Local redirect URI:

```text
http://localhost:4200/
```

## Backend JWT Values

Backend services validate bearer tokens with:

```text
Authentication__Jwt__Authority
Authentication__Jwt__Audience
Authentication__Jwt__RequireAuthorization=true
```

Do not set `RequireAuthorization=false` in production. While the frontend can be prepared before Azure values are created, real API protection must wait until Authority and Audience are configured.

