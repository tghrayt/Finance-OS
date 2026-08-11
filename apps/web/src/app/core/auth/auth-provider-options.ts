import { AuthProviderOption } from './auth-provider.models';

export const AUTH_PROVIDER_OPTIONS: AuthProviderOption[] = [
  {
    kind: 'email-password',
    icon: 'verified_user',
    label: 'Email + mot de passe',
    description: 'Compte securise avec verification email avant utilisation.',
    recommended: true,
  },
  {
    kind: 'email-code',
    icon: 'mark_email_read',
    label: 'Code par email',
    description: 'Connexion rapide avec un code temporaire envoye par email.',
  },
  {
    kind: 'google',
    icon: 'account_circle',
    label: 'Continuer avec Google',
    description: 'Utiliser un compte Google existant comme identite FinanceOS.',
  },
  {
    kind: 'microsoft',
    icon: 'business_center',
    label: 'Continuer avec Microsoft',
    description: 'Utiliser un compte Microsoft personnel ou professionnel.',
  },
];

