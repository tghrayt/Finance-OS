import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, forkJoin, map, Observable, of, startWith, throwError } from 'rxjs';

import { BudgetApiService, MonthlyBudget } from '../../budget/budget-api.service';
import { FinanceApiService, FinanceSnapshot, FinanceTransaction } from '../../finance/finance-api.service';
import { NotificationApiService } from '../../notification/notification-api.service';
import { DashboardMetrics, DashboardState } from './dashboard.models';

@Injectable({ providedIn: 'root' })
export class DashboardDataService {
  constructor(
    private readonly financeApi: FinanceApiService,
    private readonly budgetApi: BudgetApiService,
    private readonly notificationApi: NotificationApiService,
  ) {}

  load(householdId: string): Observable<DashboardState> {
    const now = new Date();

    return forkJoin({
      finance: this.financeApi.getSnapshot(householdId),
      budget: this.budgetApi
        .getCurrentBudget(householdId, now.getFullYear(), now.getMonth() + 1)
        .pipe(catchError((error: unknown) => (this.isNotFound(error) ? of(null) : throwError(() => error)))),
      notifications: this.notificationApi
        .getInAppNotifications(householdId)
        .pipe(catchError(() => of([]))),
    }).pipe(
      map(({ finance, budget, notifications }) => ({
        status: 'ready' as const,
        ...finance,
        budget,
        notifications,
        metrics: this.calculateMetrics(finance, budget),
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

  private isNotFound(error: unknown): boolean {
    return error instanceof HttpErrorResponse && error.status === 404;
  }
}
