import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, forkJoin, map, Observable, of, startWith, throwError } from 'rxjs';

import { BudgetApiService, MonthlyBudget } from '../../budget/budget-api.service';
import { FinanceApiService, FinanceSnapshot, FinanceTransaction } from '../../finance/finance-api.service';
import { NotificationApiService } from '../../notification/notification-api.service';
import { CategorySpendingInsight, DashboardMetrics, DashboardState } from './dashboard.models';

@Injectable({ providedIn: 'root' })
export class DashboardDataService {
  constructor(
    private readonly financeApi: FinanceApiService,
    private readonly budgetApi: BudgetApiService,
    private readonly notificationApi: NotificationApiService,
  ) {}

  load(): Observable<DashboardState> {
    const now = new Date();

    return forkJoin({
      finance: this.financeApi.getSnapshot(),
      budget: this.budgetApi
        .getCurrentBudget(now.getFullYear(), now.getMonth() + 1)
        .pipe(catchError((error: unknown) => (this.isNotFound(error) ? of(null) : throwError(() => error)))),
      notifications: this.notificationApi
        .getInAppNotifications()
        .pipe(catchError(() => of([]))),
    }).pipe(
      map(({ finance, budget, notifications }) => ({
        status: 'ready' as const,
        ...finance,
        budget,
        notifications,
        metrics: this.calculateMetrics(finance, budget),
        categorySpending: this.calculateCategorySpending(finance),
      })),
      startWith(this.toLoadingState()),
      catchError(() =>
        of({
          ...this.toLoadingState(),
          status: 'error' as const,
          errorMessage: 'Impossible de charger les donnees finance pour le moment.',
        }),
      ),
    );
  }

  private toLoadingState(): DashboardState {
    return {
      status: 'loading',
      accounts: [],
      categories: [],
      transactions: [],
      budget: null,
      notifications: [],
      categorySpending: [],
      metrics: {
        totalBalance: 0,
        monthlyIncome: 0,
        monthlyExpenses: 0,
        netFlow: 0,
        activeAccounts: 0,
        categoryCount: 0,
        budgetConsumption: 0,
      },
    };
  }

  private calculateMetrics(snapshot: FinanceSnapshot, budget: MonthlyBudget | null): DashboardMetrics {
    const now = new Date();
    const currentMonth = now.getMonth();
    const currentYear = now.getFullYear();
    const monthlyTransactions = snapshot.transactions.filter((transaction) => {
      const transactionDate = new Date(`${transaction.transactionDate}T00:00:00`);
      return transactionDate.getMonth() === currentMonth && transactionDate.getFullYear() === currentYear;
    });
    const monthlyIncome = this.sumByType(monthlyTransactions, 'Income');
    const monthlyExpenses = this.sumByType(monthlyTransactions, 'Expense');

    return {
      totalBalance: snapshot.accounts.reduce((total, account) => total + account.currentBalance, 0),
      monthlyIncome,
      monthlyExpenses,
      netFlow: monthlyIncome - monthlyExpenses,
      activeAccounts: snapshot.accounts.filter((account) => account.isActive).length,
      categoryCount: snapshot.categories.length,
      budgetConsumption: budget?.consumptionRatio ?? 0,
    };
  }

  private sumByType(transactions: FinanceTransaction[], type: string): number {
    return transactions
      .filter((transaction) => transaction.type.toLowerCase() === type.toLowerCase())
      .reduce((total, transaction) => total + transaction.amount, 0);
  }

  private calculateCategorySpending(snapshot: FinanceSnapshot): CategorySpendingInsight[] {
    const expenses = snapshot.transactions.filter((transaction) => transaction.type.toLowerCase() === 'expense');
    const totalExpenses = expenses.reduce((total, transaction) => total + transaction.amount, 0);
    const spending = new Map<string, CategorySpendingInsight>();

    for (const transaction of expenses) {
      const key = transaction.categoryId ?? 'uncategorized';
      const category = snapshot.categories.find((item) => item.categoryId === transaction.categoryId);
      const existing = spending.get(key);

      spending.set(key, {
        categoryId: transaction.categoryId,
        name: category?.name ?? 'Non categorise',
        amount: (existing?.amount ?? 0) + transaction.amount,
        currency: transaction.currency,
        ratio: 0,
      });
    }

    return Array.from(spending.values())
      .sort((left, right) => right.amount - left.amount)
      .slice(0, 5)
      .map((item) => ({
        ...item,
        ratio: totalExpenses > 0 ? item.amount / totalExpenses : 0,
      }));
  }

  private isNotFound(error: unknown): boolean {
    return error instanceof HttpErrorResponse && error.status === 404;
  }
}
