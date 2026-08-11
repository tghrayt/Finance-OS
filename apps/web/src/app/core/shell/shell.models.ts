export type AppSection = 'dashboard' | 'accounts' | 'transactions' | 'budgets' | 'notifications';

export type CreationModal = 'account' | 'category' | 'budget' | 'allocation' | 'transaction';

export interface NavigationItem {
  section: AppSection;
  label: string;
  icon: string;
}
