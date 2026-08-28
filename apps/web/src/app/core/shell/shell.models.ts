export type AppSection = 'dashboard' | 'accounts' | 'transactions' | 'budgets' | 'categories' | 'notifications';

export type CreationModal = 'account' | 'category' | 'budget' | 'allocation' | 'transaction' | 'edit-account' | 'edit-category';

export interface NavigationItem {
  section: AppSection;
  label: string;
  icon: string;
}
