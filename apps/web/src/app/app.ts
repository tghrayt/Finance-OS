import { AsyncPipe, CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterOutlet } from '@angular/router';
import { catchError, finalize, map, Observable, of, shareReplay, startWith, Subject, switchMap } from 'rxjs';

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
  imports: [AsyncPipe, CurrencyPipe, DatePipe, DecimalPipe, ReactiveFormsModule, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly dashboard$: Observable<DashboardState>;
  protected readonly householdId = DEMO_HOUSEHOLD_ID;
  protected readonly accountTypes = ['Checking', 'Savings', 'Cash', 'CreditCard', 'Investment', 'Other'];
  protected readonly transactionTypes = ['Expense', 'Income'];
  protected actionStatus: 'idle' | 'saving' = 'idle';
  protected actionMessage = '';
  private readonly financeApi = inject(FinanceApiService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly refreshDashboard$ = new Subject<void>();

  protected readonly accountForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(120)]],
    type: ['Checking', Validators.required],
    initialBalance: [0, Validators.required],
    currency: ['EUR', [Validators.required, Validators.minLength(3), Validators.maxLength(3)]],
    institutionName: [''],
  });

  protected readonly transactionForm = this.formBuilder.nonNullable.group({
    accountId: ['', Validators.required],
    type: ['Expense', Validators.required],
    amount: [0, [Validators.required, Validators.min(0.01)]],
    currency: ['EUR', [Validators.required, Validators.minLength(3), Validators.maxLength(3)]],
    categoryId: [''],
    merchant: [''],
    description: [''],
    transactionDate: [new Date().toISOString().slice(0, 10), Validators.required],
  });

  constructor() {
    this.dashboard$ = this.refreshDashboard$.pipe(
      startWith(undefined),
      switchMap(() =>
        this.financeApi.getSnapshot(this.householdId).pipe(
          map((snapshot) => this.toReadyState(snapshot)),
          startWith(this.toLoadingState()),
          catchError(() =>
            of({
              ...this.toLoadingState(),
              status: 'error' as const,
              errorMessage: 'Impossible de charger les donnees finance pour le moment.',
            }),
          ),
        ),
      ),
      shareReplay({ bufferSize: 1, refCount: true }),
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

  protected createAccount(): void {
    if (this.accountForm.invalid || this.actionStatus === 'saving') {
      this.accountForm.markAllAsTouched();
      return;
    }

    const value = this.accountForm.getRawValue();
    this.actionStatus = 'saving';
    this.actionMessage = '';

    this.financeApi
      .createAccount({
        householdId: this.householdId,
        name: value.name.trim(),
        type: value.type,
        initialBalance: Number(value.initialBalance),
        currency: value.currency.toUpperCase(),
        institutionName: this.emptyToNull(value.institutionName),
      })
      .pipe(finalize(() => (this.actionStatus = 'idle')))
      .subscribe({
        next: (account) => {
          this.actionMessage = `Compte "${account.name}" cree.`;
          this.transactionForm.patchValue({
            accountId: account.accountId,
            currency: account.currency,
          });
          this.accountForm.reset({
            name: '',
            type: 'Checking',
            initialBalance: 0,
            currency: account.currency,
            institutionName: '',
          });
          this.refreshDashboard$.next();
        },
        error: () => {
          this.actionMessage = 'Creation du compte impossible pour le moment.';
        },
      });
  }

  protected createTransaction(): void {
    if (this.transactionForm.invalid || this.actionStatus === 'saving') {
      this.transactionForm.markAllAsTouched();
      return;
    }

    const value = this.transactionForm.getRawValue();
    this.actionStatus = 'saving';
    this.actionMessage = '';

    this.financeApi
      .createTransaction({
        householdId: this.householdId,
        accountId: value.accountId,
        destinationAccountId: null,
        type: value.type,
        amount: Number(value.amount),
        currency: value.currency.toUpperCase(),
        categoryId: this.emptyToNull(value.categoryId),
        merchant: this.emptyToNull(value.merchant),
        description: this.emptyToNull(value.description),
        transactionDate: value.transactionDate,
        correlationId: null,
      })
      .pipe(finalize(() => (this.actionStatus = 'idle')))
      .subscribe({
        next: () => {
          this.actionMessage = 'Transaction enregistree.';
          this.transactionForm.patchValue({
            amount: 0,
            merchant: '',
            description: '',
            transactionDate: new Date().toISOString().slice(0, 10),
          });
          this.refreshDashboard$.next();
        },
        error: () => {
          this.actionMessage = 'Creation de la transaction impossible pour le moment.';
        },
      });
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

  private emptyToNull(value: string): string | null {
    const trimmed = value.trim();
    return trimmed.length === 0 ? null : trimmed;
  }
}
