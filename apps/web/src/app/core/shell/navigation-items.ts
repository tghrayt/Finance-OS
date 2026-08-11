import { NavigationItem } from './shell.models';

export const NAVIGATION_ITEMS: ReadonlyArray<NavigationItem> = [
  { section: 'dashboard', label: 'Dashboard', icon: 'dashboard' },
  { section: 'accounts', label: 'Comptes', icon: 'account_balance' },
  { section: 'transactions', label: 'Transactions', icon: 'receipt_long' },
  { section: 'budgets', label: 'Budgets', icon: 'savings' },
  { section: 'notifications', label: 'Alertes', icon: 'notifications_active' },
];
