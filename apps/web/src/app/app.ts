import { AsyncPipe, CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { catchError, map, Observable, of, startWith } from 'rxjs';

import {
  FinanceAccount,
  FinanceApiService,
  FinanceCategory,
  FinanceSnapshot,
  FinanceTransaction,
} from './finance/finance-api.service';

const DEMO_HOUSEHOLD_ID = '00000000-0000-0000-0000-000000000000';

interface DashboardMetrics {
  totalBalance: number;
  monthlyIncome: number;
  monthlyExpenses: number;
  netFlow: number;
  activeAccounts: number;
  categoryCount: number;
}

interface DashboardState {
  status: 'loading' | 'ready' | 'error';
  accounts: FinanceAccount[];
  categories: FinanceCategory[];
  transactions: FinanceTransaction[];
  metrics: DashboardMetrics;
  errorMessage?: string;
}

@Component({
  selector: 'app-root',
  imports: [AsyncPipe, CurrencyPipe, DatePipe, DecimalPipe, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly dashboard$: Observable<DashboardState>;
  protected readonly householdId = DEMO_HOUSEHOLD_ID;

  constructor(private readonly financeApi: FinanceApiService) {
    this.dashboard$ = this.financeApi.getSnapshot(this.householdId).pipe(
      map((snapshot) => this.toReadyState(snapshot)),
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

  protected trackAccount(_: number, account: FinanceAccount): string {
    return account.accountId;
  }

  protected trackCategory(_: number, category: FinanceCategory): string {
    return category.categoryId;
  }

  protected trackTransaction(_: number, transaction: FinanceTransaction): string {
    return transaction.transactionId;
  }

  protected categoryName(transaction: FinanceTransaction, categories: FinanceCategory[]): string {
    return categories.find((category) => category.categoryId === transaction.categoryId)?.name ?? 'Non categorise';
  }

  private toReadyState(snapshot: FinanceSnapshot): DashboardState {
    return {
      status: 'ready',
      ...snapshot,
      metrics: this.calculateMetrics(snapshot),
    };
  }

  private toLoadingState(): DashboardState {
    return {
      status: 'loading',
      accounts: [],
      categories: [],
      transactions: [],
      metrics: {
        totalBalance: 0,
        monthlyIncome: 0,
        monthlyExpenses: 0,
        netFlow: 0,
        activeAccounts: 0,
        categoryCount: 0,
      },
    };
  }

  private calculateMetrics(snapshot: FinanceSnapshot): DashboardMetrics {
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
    };
  }

  private sumByType(transactions: FinanceTransaction[], type: string): number {
    return transactions
      .filter((transaction) => transaction.type.toLowerCase() === type.toLowerCase())
      .reduce((total, transaction) => total + transaction.amount, 0);
  }
}
