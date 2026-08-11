import { AsyncPipe, CurrencyPipe, DatePipe, DecimalPipe, PercentPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { filter, finalize, Observable, shareReplay, startWith, Subject, switchMap } from 'rxjs';

import { BudgetApiService, MonthlyBudget } from './budget/budget-api.service';
import { AuthSessionService } from './core/auth/auth-session.service';
import { DashboardDataService } from './features/dashboard/dashboard-data.service';
import { DashboardState } from './features/dashboard/dashboard.models';
import {
  FinanceAccount,
  FinanceApiService,
  FinanceCategory,
  FinanceTransaction,
} from './finance/finance-api.service';
import { InAppNotification, NotificationApiService } from './notification/notification-api.service';

type AppSection = 'dashboard' | 'accounts' | 'transactions' | 'budgets' | 'notifications';
type CreationModal = 'account' | 'category' | 'budget' | 'allocation' | 'transaction';

@Component({
  selector: 'app-root',
  imports: [AsyncPipe, CurrencyPipe, DatePipe, DecimalPipe, PercentPipe, ReactiveFormsModule, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly dashboard$: Observable<DashboardState>;
  protected readonly session = inject(AuthSessionService).session;
  protected readonly navItems: ReadonlyArray<{ section: AppSection; label: string; icon: string }> = [
    { section: 'dashboard', label: 'Dashboard', icon: 'dashboard' },
    { section: 'accounts', label: 'Comptes', icon: 'account_balance' },
    { section: 'transactions', label: 'Transactions', icon: 'receipt_long' },
    { section: 'budgets', label: 'Budgets', icon: 'savings' },
    { section: 'notifications', label: 'Alertes', icon: 'notifications_active' },
  ];
  protected readonly accountTypes = ['Checking', 'Savings', 'Cash', 'CreditCard', 'Investment', 'Other'];
  protected readonly transactionTypes = ['Expense', 'Income'];
  protected activeSection: AppSection = 'dashboard';
  protected activeModal: CreationModal | null = null;
  protected demoAccessGranted = !!this.session();
  protected actionStatus: 'idle' | 'saving' = 'idle';
  protected actionMessage = '';
  private readonly financeApi = inject(FinanceApiService);
  private readonly budgetApi = inject(BudgetApiService);
  private readonly notificationApi = inject(NotificationApiService);
  private readonly dashboardData = inject(DashboardDataService);
  private readonly authSession = inject(AuthSessionService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly router = inject(Router);
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

  protected readonly categoryForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(120)]],
    icon: ['label', [Validators.required, Validators.maxLength(64)]],
  });

  protected readonly budgetForm = this.formBuilder.nonNullable.group({
    totalBudget: [1500, [Validators.required, Validators.min(0.01)]],
    currency: ['EUR', [Validators.required, Validators.minLength(3), Validators.maxLength(3)]],
  });

  protected readonly allocationForm = this.formBuilder.nonNullable.group({
    categoryId: ['', Validators.required],
    plannedAmount: [100, [Validators.required, Validators.min(0.01)]],
  });

  protected readonly accessForm = this.formBuilder.nonNullable.group({
    email: ['demo@financeos.local', [Validators.required, Validators.email]],
  });

  constructor() {
    this.syncActiveSection(this.router.url);
    this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe((event) => this.syncActiveSection(event.urlAfterRedirects));

    this.dashboard$ = this.refreshDashboard$.pipe(
      startWith(undefined),
      switchMap(() => this.dashboardData.load(this.householdId)),
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

  protected trackNotification(_: number, notification: InAppNotification): string {
    return notification.notificationId;
  }

  protected enterDemoAccess(): void {
    if (this.accessForm.invalid) {
      this.accessForm.markAllAsTouched();
      return;
    }

    this.authSession.startDemoSession(this.accessForm.getRawValue().email.trim());
    this.demoAccessGranted = true;
  }

  protected signOut(): void {
    this.authSession.clearSession();
    this.demoAccessGranted = false;
    this.activeModal = null;
    this.setActiveSection('dashboard');
  }

  protected setActiveSection(section: AppSection): void {
    this.activeSection = section;
    void this.router.navigate([`/${section}`]);
  }

  protected openModal(modal: CreationModal): void {
    this.actionMessage = '';
    this.activeModal = modal;
  }

  protected closeModal(): void {
    if (this.actionStatus === 'saving') {
      return;
    }

    this.activeModal = null;
  }

  protected categoryName(transaction: FinanceTransaction, categories: FinanceCategory[]): string {
    return categories.find((category) => category.categoryId === transaction.categoryId)?.name ?? 'Non categorise';
  }

  protected allocationCategoryName(allocation: { categoryId: string }, categories: FinanceCategory[]): string {
    return categories.find((category) => category.categoryId === allocation.categoryId)?.name ?? 'Categorie';
  }

  protected get householdId(): string {
    return this.authSession.householdId();
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
          this.closeModal();
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
          this.closeModal();
          this.refreshDashboard$.next();
        },
        error: () => {
          this.actionMessage = 'Creation de la transaction impossible pour le moment.';
        },
      });
  }

  protected createCategory(): void {
    if (this.categoryForm.invalid || this.actionStatus === 'saving') {
      this.categoryForm.markAllAsTouched();
      return;
    }

    const value = this.categoryForm.getRawValue();
    this.actionStatus = 'saving';
    this.actionMessage = '';

    this.financeApi
      .createCategory({
        householdId: this.householdId,
        name: value.name.trim(),
        parentCategoryId: null,
        icon: value.icon.trim(),
      })
      .pipe(finalize(() => (this.actionStatus = 'idle')))
      .subscribe({
        next: (category) => {
          this.actionMessage = `Categorie "${category.name}" creee.`;
          this.transactionForm.patchValue({ categoryId: category.categoryId });
          this.categoryForm.reset({ name: '', icon: 'label' });
          this.closeModal();
          this.refreshDashboard$.next();
        },
        error: () => {
          this.actionMessage = 'Creation de la categorie impossible pour le moment.';
        },
      });
  }

  protected createBudget(): void {
    if (this.budgetForm.invalid || this.actionStatus === 'saving') {
      this.budgetForm.markAllAsTouched();
      return;
    }

    const value = this.budgetForm.getRawValue();
    this.actionStatus = 'saving';
    this.actionMessage = '';
    const now = new Date();

    this.budgetApi
      .createMonthlyBudget({
        householdId: this.householdId,
        year: now.getFullYear(),
        month: now.getMonth() + 1,
        totalBudget: Number(value.totalBudget),
        currency: value.currency.toUpperCase(),
      })
      .pipe(finalize(() => (this.actionStatus = 'idle')))
      .subscribe({
        next: () => {
          this.actionMessage = 'Budget mensuel cree.';
          this.closeModal();
          this.refreshDashboard$.next();
        },
        error: () => {
          this.actionMessage = 'Creation du budget impossible pour le moment.';
        },
      });
  }

  protected setBudgetAllocation(budget: MonthlyBudget | null): void {
    if (!budget || this.allocationForm.invalid || this.actionStatus === 'saving') {
      this.allocationForm.markAllAsTouched();
      return;
    }

    const value = this.allocationForm.getRawValue();
    this.actionStatus = 'saving';
    this.actionMessage = '';

    this.budgetApi
      .setAllocation(budget.budgetId, value.categoryId, { plannedAmount: Number(value.plannedAmount) })
      .pipe(finalize(() => (this.actionStatus = 'idle')))
      .subscribe({
        next: () => {
          this.actionMessage = 'Allocation budgetaire mise a jour.';
          this.closeModal();
          this.refreshDashboard$.next();
        },
        error: () => {
          this.actionMessage = "Mise a jour de l'allocation impossible pour le moment.";
        },
      });
  }

  protected markNotificationRead(notification: InAppNotification): void {
    if (notification.readAt || this.actionStatus === 'saving') {
      return;
    }

    this.actionStatus = 'saving';
    this.actionMessage = '';

    this.notificationApi
      .markAsRead(this.householdId, notification.notificationId)
      .pipe(finalize(() => (this.actionStatus = 'idle')))
      .subscribe({
        next: () => {
          this.actionMessage = 'Alerte marquee comme lue.';
          this.refreshDashboard$.next();
        },
        error: () => {
          this.actionMessage = "Mise a jour de l'alerte impossible pour le moment.";
        },
      });
  }

  private emptyToNull(value: string): string | null {
    const trimmed = value.trim();
    return trimmed.length === 0 ? null : trimmed;
  }

  private syncActiveSection(url: string): void {
    const section = url.split('?')[0].split('#')[0].replace('/', '') as AppSection;
    this.activeSection = this.navItems.some((item) => item.section === section) ? section : 'dashboard';
  }
}
