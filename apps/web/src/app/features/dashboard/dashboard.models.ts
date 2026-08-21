import { MonthlyBudget } from '../../budget/budget-api.service';
import { FinanceAccount, FinanceCategory, FinanceTransaction } from '../../finance/finance-api.service';
import { InAppNotification } from '../../notification/notification-api.service';

export interface DashboardMetrics {
  totalBalance: number;
  monthlyIncome: number;
  monthlyExpenses: number;
  netFlow: number;
  activeAccounts: number;
  categoryCount: number;
  budgetConsumption: number;
}

export interface CategorySpendingInsight {
  categoryId: string | null;
  name: string;
  amount: number;
  currency: string;
  ratio: number;
}

export interface DashboardState {
  status: 'loading' | 'ready' | 'error';
  accounts: FinanceAccount[];
  categories: FinanceCategory[];
  transactions: FinanceTransaction[];
  budget: MonthlyBudget | null;
  notifications: InAppNotification[];
  metrics: DashboardMetrics;
  categorySpending: CategorySpendingInsight[];
  errorMessage?: string;
}
