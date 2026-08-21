import { AuthProviderOption } from './auth-provider.models';

export const AUTH_PROVIDER_OPTIONS: AuthProviderOption[] = [
  {
    kind: 'email-password',
    icon: 'verified_user',
    label: 'Créer un compte ou se connecter',
    description: 'Email, mot de passe et verification email avant acces a FinanceOS.',
    recommended: true,
  },
];
