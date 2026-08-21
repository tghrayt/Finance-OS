import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';

import type { AuthenticationResult, Configuration, IPublicClientApplication } from '@azure/msal-browser';

import { AUTH_PROVIDER_OPTIONS } from './auth-provider-options';
import { AuthProviderKind, HostedAuthConfig, HostedAuthStatus } from './auth-provider.models';

const DEMO_HOUSEHOLD_ID = '00000000-0000-0000-0000-000000000000';
const STORAGE_KEY = 'financeos.auth.session';
const AUTH_CONFIG_URL = '/config/auth-config.json';

export interface AuthSession {
  mode: 'demo' | 'authenticated';
  email: string;
  householdId: string;
  accessToken: string | null;
}

interface BootstrapCurrentIdentityResponse {
  user: {
    email: string;
  };
  household: {
    householdId: string;
  };
}

@Injectable({ providedIn: 'root' })
export class AuthSessionService {
  private readonly http = inject(HttpClient);
  private readonly currentSession = signal<AuthSession | null>(this.readStoredSession());
  private hostedAuthConfig: HostedAuthConfig | null = null;
  private hostedAuthClient: IPublicClientApplication | null = null;
  private hostedAuthInitialization: Promise<void> | null = null;

  readonly providerOptions = AUTH_PROVIDER_OPTIONS;
  readonly hostedAuthStatus = signal<HostedAuthStatus>({
    enabled: false,
    ready: false,
    message: 'Authentification externe non configuree.',
  });
  readonly session = this.currentSession.asReadonly();

  readonly householdId = signal(this.currentSession()?.householdId ?? DEMO_HOUSEHOLD_ID);

  hasResolvedHousehold(): boolean {
    const session = this.currentSession();

    return session?.mode === 'demo' || this.householdId() !== DEMO_HOUSEHOLD_ID;
  }

  initializeHostedAuth(): Promise<void> {
    this.hostedAuthInitialization ??= this.loadHostedAuth();
    return this.hostedAuthInitialization;
  }

  async signInWithProvider(provider: AuthProviderKind): Promise<void> {
    await this.initializeHostedAuth();

    if (!this.hostedAuthClient || !this.hostedAuthConfig?.enabled) {
      this.hostedAuthStatus.set({
        enabled: false,
        ready: false,
        message: 'Configure Microsoft Entra External ID pour activer cette methode.',
      });
      return;
    }

    await this.hostedAuthClient.loginRedirect({
      scopes: this.hostedAuthConfig.scopes,
      state: provider,
      extraQueryParameters: {
        financeos_auth_provider: provider,
      },
    });
  }

  startDemoSession(email: string): void {
    this.setSession({
      mode: 'demo',
      email,
      householdId: DEMO_HOUSEHOLD_ID,
      accessToken: null,
    });
  }

  clearSession(): void {
    this.currentSession.set(null);
    this.householdId.set(DEMO_HOUSEHOLD_ID);
    localStorage.removeItem(STORAGE_KEY);
    void this.hostedAuthClient?.logoutRedirect({
      postLogoutRedirectUri: this.hostedAuthConfig?.postLogoutRedirectUri || window.location.origin,
    });
  }

  getAccessToken(): string | null {
    return this.currentSession()?.accessToken ?? null;
  }

  private setSession(session: AuthSession): void {
    this.currentSession.set(session);
    this.householdId.set(session.householdId);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
  }

  private async loadHostedAuth(): Promise<void> {
    let config: HostedAuthConfig;

    try {
      config = await firstValueFrom(this.http.get<HostedAuthConfig>(AUTH_CONFIG_URL));
    } catch {
      this.hostedAuthStatus.set({
        enabled: false,
        ready: false,
        message: 'Configuration auth introuvable. Le mode demo reste disponible.',
      });
      return;
    }

    this.hostedAuthConfig = this.normalizeHostedAuthConfig(config);

    if (!this.hostedAuthConfig.enabled) {
      this.hostedAuthStatus.set({
        enabled: false,
        ready: false,
        message: '',
      });
      return;
    }

    if (!this.hostedAuthConfig.authority || !this.hostedAuthConfig.clientId) {
      this.hostedAuthStatus.set({
        enabled: true,
        ready: false,
        message: 'Authority et clientId sont requis pour activer Entra.',
      });
      return;
    }

    const { PublicClientApplication } = await import('@azure/msal-browser');
    this.hostedAuthClient = new PublicClientApplication(this.buildMsalConfiguration(this.hostedAuthConfig));
    await this.hostedAuthClient.initialize();
    const redirectResult = await this.hostedAuthClient.handleRedirectPromise();
    await this.captureAuthenticatedSession(redirectResult);

    this.hostedAuthStatus.set({
      enabled: true,
      ready: true,
      message: 'Microsoft Entra External ID pret.',
    });
  }

  private async captureAuthenticatedSession(result: AuthenticationResult | null): Promise<void> {
    const account = result?.account ?? this.hostedAuthClient?.getAllAccounts()[0] ?? null;
    if (!account || !this.hostedAuthClient || !this.hostedAuthConfig?.enabled) {
      return;
    }

    this.hostedAuthClient.setActiveAccount(account);
    const accessToken = result?.accessToken || (await this.acquireAccessToken()).accessToken;
    if (!accessToken) {
      return;
    }

    const identity = await this.bootstrapCurrentIdentity(accessToken);

    this.setSession({
      mode: 'authenticated',
      email: identity.user.email || account.username,
      householdId: identity.household.householdId,
      accessToken,
    });
  }

  private async acquireAccessToken(): Promise<AuthenticationResult> {
    const account = this.hostedAuthClient?.getActiveAccount() ?? this.hostedAuthClient?.getAllAccounts()[0];
    if (!this.hostedAuthClient || !this.hostedAuthConfig || !account) {
      throw new Error('Hosted authentication account is missing.');
    }

    return await this.hostedAuthClient.acquireTokenSilent({
      account,
      scopes: this.hostedAuthConfig.scopes,
    });
  }

  private async bootstrapCurrentIdentity(accessToken: string): Promise<BootstrapCurrentIdentityResponse> {
    return await firstValueFrom(
      this.http.post<BootstrapCurrentIdentityResponse>(
        '/api/v1/identity/me/bootstrap',
        {},
        {
          headers: {
            Authorization: `Bearer ${accessToken}`,
          },
        },
      ),
    );
  }

  private normalizeHostedAuthConfig(config: Partial<HostedAuthConfig>): HostedAuthConfig {
    return {
      enabled: config.enabled ?? false,
      authority: config.authority ?? '',
      clientId: config.clientId ?? '',
      redirectUri: config.redirectUri || window.location.origin,
      postLogoutRedirectUri: config.postLogoutRedirectUri || window.location.origin,
      knownAuthorities: config.knownAuthorities ?? [],
      scopes: config.scopes?.length ? config.scopes : ['openid', 'profile', 'email'],
    };
  }

  private buildMsalConfiguration(config: HostedAuthConfig): Configuration {
    return {
      auth: {
        authority: config.authority,
        clientId: config.clientId,
        redirectUri: config.redirectUri,
        postLogoutRedirectUri: config.postLogoutRedirectUri,
        knownAuthorities: config.knownAuthorities,
      },
      cache: {
        cacheLocation: 'localStorage',
      },
    };
  }

  private readStoredSession(): AuthSession | null {
    const rawSession = localStorage.getItem(STORAGE_KEY);
    if (!rawSession) {
      return null;
    }

    try {
      const session = JSON.parse(rawSession) as Partial<AuthSession>;
      if (!session.email || !session.householdId || (session.mode !== 'demo' && session.mode !== 'authenticated')) {
        return null;
      }

      return {
        mode: session.mode,
        email: session.email,
        householdId: session.householdId,
        accessToken: session.accessToken ?? null,
      };
    } catch {
      return null;
    }
  }
}
