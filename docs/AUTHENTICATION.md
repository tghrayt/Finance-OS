# FinanceOS Authentication

FinanceOS uses OpenID Connect with Microsoft Entra External ID for customer authentication.
The application must never store customer passwords. Passwords, email verification,
and account recovery are handled by the identity provider.

## Sign-in Methods

The active production entry point is:

- Email + password: recommended for long-term FinanceOS accounts. Email verification is configured in the Entra sign-up/sign-in user flow.

The following methods remain planned for a later phase and are intentionally hidden in the web UI for now:

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
3. A SPA app registration for the Angular web app.
4. A protected API app registration for FinanceOS backend scopes.

Do not enable Google, Microsoft or email one-time passcode in the FinanceOS user flow yet. They can be added later without changing the FinanceOS identity architecture.

Microsoft setup references:

- External tenant and customer user flows: <https://learn.microsoft.com/en-us/entra/external-id/customers/overview-customers-ciam>
- Protected API scopes: <https://learn.microsoft.com/en-us/entra/identity-platform/scenario-protected-web-api-expose-scopes>

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

## Kubernetes Activation

After the Entra values are available, update the public web configuration:

```bash
sudo kubectl -n financeos create configmap financeos-web-auth-config \
  --from-literal=auth-config.json='{
    "enabled": true,
    "authority": "https://<tenant-subdomain>.ciamlogin.com/<tenant-id-or-domain>/<user-flow-name>",
    "clientId": "<spa-application-client-id>",
    "redirectUri": "https://financeos.51-210-40-78.sslip.io/",
    "postLogoutRedirectUri": "https://financeos.51-210-40-78.sslip.io/",
    "knownAuthorities": ["<tenant-subdomain>.ciamlogin.com"],
    "scopes": ["openid", "profile", "email", "api://<api-application-client-id>/financeos.access"]
  }' \
  --dry-run=client -o yaml | sudo kubectl apply -f -
```

Then update backend JWT validation:

```bash
sudo kubectl -n financeos create configmap financeos-auth-config \
  --from-literal=jwt-authority='https://<tenant-subdomain>.ciamlogin.com/<tenant-id-or-domain>/<user-flow-name>' \
  --from-literal=jwt-audience='api://<api-application-client-id>' \
  --dry-run=client -o yaml | sudo kubectl apply -f -
```

Restart the affected deployments:

```bash
sudo kubectl -n financeos rollout restart deployment/financeos-web
sudo kubectl -n financeos rollout restart deployment/financeos-identity-api
sudo kubectl -n financeos rollout status deployment/financeos-web
sudo kubectl -n financeos rollout status deployment/financeos-identity-api
```

When Google or Microsoft sign-in is added later, keep their client secrets only inside the Entra portal identity provider configuration. They must not be stored in FinanceOS source code, GitHub repository variables or frontend config.
