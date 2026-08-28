export type AuthProviderKind = 'email-password' | 'email-code' | 'google' | 'microsoft';

export interface AuthProviderOption {
  kind: AuthProviderKind;
  icon: string;
  label: string;
  description: string;
  recommended?: boolean;
}

export interface HostedAuthConfig {
  enabled: boolean;
  authority: string;
  clientId: string;
  redirectUri: string;
  silentRedirectUri: string;
  postLogoutRedirectUri: string;
  knownAuthorities: string[];
  scopes: string[];
}

export interface HostedAuthStatus {
  enabled: boolean;
  ready: boolean;
  message: string;
}
