import { AsyncPipe, CurrencyPipe, DatePipe, DecimalPipe, PercentPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { NavigationEnd, Router } from '@angular/router';
import { filter, finalize, Observable, ReplaySubject, shareReplay, switchMap } from 'rxjs';

import { BudgetApiService, MonthlyBudget } from '../../budget/budget-api.service';
import { AuthSessionService } from '../../core/auth/auth-session.service';
import { NAVIGATION_ITEMS } from '../../core/shell/navigation-items';
import { AppSection, CreationModal } from '../../core/shell/shell.models';
import { DashboardDataService } from '../dashboard/dashboard-data.service';
import { DashboardState } from '../dashboard/dashboard.models';
import {
  FinanceAccount,
  FinanceApiService,
  FinanceCategory,
  FinanceTransaction,
} from '../../finance/finance-api.service';
import { InAppNotification, NotificationApiService } from '../../notification/notification-api.service';

@Component({
  selector: 'app-dashboard-shell',
  imports: [AsyncPipe, CurrencyPipe, DatePipe, DecimalPipe, PercentPipe, ReactiveFormsModule],
  templateUrl: './dashboard-shell.component.html',
  styleUrl: './dashboard-shell.component.scss',
})
export class DashboardShellComponent {
  protected readonly dashboard$: Observable<DashboardState>;
  protected readonly navItems = NAVIGATION_ITEMS;
  protected readonly accountTypes = ['Checking', 'Savings', 'Cash', 'CreditCard', 'Investment', 'Other'];
  protected readonly transactionTypes = ['Expense', 'Income'];
  protected activeSection: AppSection = 'dashboard';
  protected activeModal: CreationModal | null = null;
  protected editingAccount: FinanceAccount | null = null;
  protected editingCategory: FinanceCategory | null = null;
  protected readonly authReady = signal(false);
  protected actionStatus: 'idle' | 'saving' = 'idle';
  protected actionMessage = '';
  private readonly financeApi = inject(FinanceApiService);
  private readonly budgetApi = inject(BudgetApiService);
  private readonly notificationApi = inject(NotificationApiService);
  private readonly dashboardData = inject(DashboardDataService);
  private readonly authSession = inject(AuthSessionService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly refreshDashboard$ = new ReplaySubject<void>(1);

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

  protected readonly accountEditForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(120)]],
    type: ['Checking', Validators.required],
    institutionName: [''],
  });

  protected readonly categoryEditForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(120)]],
    icon: ['label', [Validators.required, Validators.maxLength(64)]],
  });

  constructor() {
    this.syncActiveSection(this.router.url);
    this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe((event) => this.syncActiveSection(event.urlAfterRedirects));

    this.dashboard$ = this.refreshDashboard$.pipe(
      switchMap(() => this.dashboardData.load()),
      shareReplay({ bufferSize: 1, refCount: true }),
    );

    if (this.authSession.session() && this.authSession.hasResolvedHousehold()) {
      this.authReady.set(true);
      this.refreshDashboard$.next();
    } else {
      void this.ensureAuthenticated();
    }
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

  protected signOut(): void {
    this.authSession.clearSession();
    this.dismissModal();
    void this.router.navigate(['/login']);
  }

  protected setActiveSection(section: AppSection): void {
    this.activeSection = section;
    void this.router.navigate([`/${section}`]);
  }

  protected openModal(modal: CreationModal): void {
    this.actionMessage = '';
    this.activeModal = modal;
  }

  protected openAccountEditor(account: FinanceAccount): void {
    this.editingAccount = account;
    this.accountEditForm.reset({
      name: account.name,
      type: account.type,
      institutionName: account.institutionName,
    });
    this.openModal('edit-account');
  }

  protected openCategoryEditor(category: FinanceCategory): void {
    this.editingCategory = category;
    this.categoryEditForm.reset({
      name: category.name,
      icon: category.icon || 'label',
    });
    this.openModal('edit-category');
  }

  protected closeModal(): void {
    if (this.actionStatus === 'saving') {
      return;
    }

    this.dismissModal();
  }

  protected categoryName(transaction: FinanceTransaction, categories: FinanceCategory[]): string {
    return categories.find((category) => category.categoryId === transaction.categoryId)?.name ?? 'Non categorise';
  }

  protected allocationCategoryName(allocation: { categoryId: string }, categories: FinanceCategory[]): string {
    return categories.find((category) => category.categoryId === allocation.categoryId)?.name ?? 'Categorie';
  }

  protected pageTitle(): string {
    switch (this.activeSection) {
      case 'accounts':
        return 'Comptes financiers';
      case 'transactions':
        return 'Transactions';
      case 'budgets':
        return 'Budget mensuel';
      case 'categories':
        return 'Categories';
      case 'notifications':
        return 'Alertes budget';
      default:
        return 'Pilotage financier personnel';
    }
  }

  protected pageEyebrow(): string {
    return this.activeSection === 'dashboard' ? 'FinanceOS' : 'Espace de gestion';
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
          this.dismissModal();
          this.refreshDashboard$.next();
        },
        error: (error: unknown) => {
          this.actionMessage = this.describeActionError('Creation du compte impossible', error);
        },
      });
  }

  protected archiveAccount(account: FinanceAccount): void {
    if (this.actionStatus === 'saving' || !account.isActive) {
      return;
    }

    this.actionStatus = 'saving';
    this.actionMessage = '';

    this.financeApi
      .archiveAccount(account.accountId)
      .pipe(finalize(() => (this.actionStatus = 'idle')))
      .subscribe({
        next: () => {
          this.actionMessage = `Compte "${account.name}" archive.`;
          this.refreshDashboard$.next();
        },
        error: (error: unknown) => {
          this.actionMessage = this.describeActionError("Archivage du compte impossible", error);
        },
      });
  }

  protected updateAccount(): void {
    if (!this.editingAccount || this.accountEditForm.invalid || this.actionStatus === 'saving') {
      this.accountEditForm.markAllAsTouched();
      return;
    }

    const value = this.accountEditForm.getRawValue();
    this.actionStatus = 'saving';
    this.actionMessage = '';

    this.financeApi
      .updateAccount(this.editingAccount.accountId, {
        name: value.name.trim(),
        type: value.type,
        institutionName: this.emptyToNull(value.institutionName),
      })
      .pipe(finalize(() => (this.actionStatus = 'idle')))
      .subscribe({
        next: (account) => {
          this.actionMessage = `Compte "${account.name}" mis a jour.`;
          this.dismissModal();
          this.refreshDashboard$.next();
        },
        error: (error: unknown) => {
          this.actionMessage = this.describeActionError('Modification du compte impossible', error);
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
          this.dismissModal();
          this.refreshDashboard$.next();
        },
        error: (error: unknown) => {
          this.actionMessage = this.describeActionError('Creation de la transaction impossible', error);
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
          this.dismissModal();
          this.refreshDashboard$.next();
        },
        error: (error: unknown) => {
          this.actionMessage = this.describeActionError('Creation de la categorie impossible', error);
        },
      });
  }

  protected updateCategory(): void {
    if (!this.editingCategory || this.categoryEditForm.invalid || this.actionStatus === 'saving') {
      this.categoryEditForm.markAllAsTouched();
      return;
    }

    const value = this.categoryEditForm.getRawValue();
    this.actionStatus = 'saving';
    this.actionMessage = '';

    this.financeApi
      .updateCategory(this.editingCategory.categoryId, {
        name: value.name.trim(),
        icon: value.icon.trim(),
      })
      .pipe(finalize(() => (this.actionStatus = 'idle')))
      .subscribe({
        next: (category) => {
          this.actionMessage = `Categorie "${category.name}" mise a jour.`;
          this.dismissModal();
          this.refreshDashboard$.next();
        },
        error: (error: unknown) => {
          this.actionMessage = this.describeActionError('Modification de la categorie impossible', error);
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
        year: now.getFullYear(),
        month: now.getMonth() + 1,
        totalBudget: Number(value.totalBudget),
        currency: value.currency.toUpperCase(),
      })
      .pipe(finalize(() => (this.actionStatus = 'idle')))
      .subscribe({
        next: () => {
          this.actionMessage = 'Budget mensuel cree.';
          this.dismissModal();
          this.refreshDashboard$.next();
        },
        error: (error: unknown) => {
          this.actionMessage = this.describeActionError('Creation du budget impossible', error);
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
          this.dismissModal();
          this.refreshDashboard$.next();
        },
        error: (error: unknown) => {
          this.actionMessage = this.describeActionError("Mise a jour de l'allocation impossible", error);
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
      .markAsRead(notification.notificationId)
      .pipe(finalize(() => (this.actionStatus = 'idle')))
      .subscribe({
        next: () => {
          this.actionMessage = 'Alerte marquee comme lue.';
          this.refreshDashboard$.next();
        },
        error: (error: unknown) => {
          this.actionMessage = this.describeActionError("Mise a jour de l'alerte impossible", error);
        },
      });
  }

  private async ensureAuthenticated(): Promise<void> {
    await this.authSession.initializeHostedAuth();

    if (!this.authSession.session()) {
      await this.router.navigate(['/login']);
      return;
    }

    this.authReady.set(true);
    this.refreshDashboard$.next();
  }

  private emptyToNull(value: string): string | null {
    const trimmed = value.trim();
    return trimmed.length === 0 ? null : trimmed;
  }

  private describeActionError(prefix: string, error: unknown): string {
    if (!(error instanceof HttpErrorResponse)) {
      return `${prefix}.`;
    }

    const details = this.extractProblemDetails(error.error);
    const status = error.status > 0 ? `HTTP ${error.status}` : 'Erreur reseau';

    return details ? `${prefix} (${status}) : ${details}` : `${prefix} (${status}).`;
  }

  private extractProblemDetails(error: unknown): string | null {
    if (!error) {
      return null;
    }

    if (typeof error === 'string') {
      return error;
    }

    if (typeof error === 'object') {
      const problem = error as { detail?: unknown; title?: unknown; message?: unknown };
      const value = problem.detail ?? problem.title ?? problem.message;

      return typeof value === 'string' ? value : null;
    }

    return null;
  }

  private dismissModal(): void {
    this.activeModal = null;
    this.editingAccount = null;
    this.editingCategory = null;
  }

  private syncActiveSection(url: string): void {
    const section = url.split('?')[0].split('#')[0].replace('/', '') as AppSection;
    this.activeSection = this.navItems.some((item) => item.section === section) ? section : 'dashboard';
  }
}
