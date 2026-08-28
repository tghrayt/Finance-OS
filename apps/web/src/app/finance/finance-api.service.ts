import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { forkJoin, Observable } from 'rxjs';

import { AuthSessionService } from '../core/auth/auth-session.service';

const FINANCE_API_BASE_URL = '/api/v1/finance';

export interface FinanceAccount {
  accountId: string;
  householdId: string;
  name: string;
  type: string;
  currentBalance: number;
  currency: string;
  institutionName: string;
  isActive: boolean;
}

export interface FinanceCategory {
  categoryId: string;
  householdId: string;
  name: string;
  parentCategoryId: string | null;
  icon: string;
  isSystem: boolean;
}

export interface FinanceTransaction {
  transactionId: string;
  householdId: string;
  accountId: string;
  destinationAccountId: string | null;
  type: string;
  amount: number;
  currency: string;
  categoryId: string | null;
  transactionDate: string;
}

export interface FinanceSnapshot {
  accounts: FinanceAccount[];
  categories: FinanceCategory[];
  transactions: FinanceTransaction[];
}

export interface CreateAccountRequest {
  householdId: string;
  name: string;
  type: string;
  initialBalance: number;
  currency: string;
  institutionName: string | null;
}

export interface UpdateAccountRequest {
  householdId: string;
  name: string;
  type: string;
  institutionName: string | null;
}

export interface CreateTransactionRequest {
  householdId: string;
  accountId: string;
  destinationAccountId: string | null;
  type: string;
  amount: number;
  currency: string;
  categoryId: string | null;
  merchant: string | null;
  description: string | null;
  transactionDate: string;
  correlationId: string | null;
}

export interface CreateCategoryRequest {
  householdId: string;
  name: string;
  parentCategoryId: string | null;
  icon: string | null;
}

export interface UpdateCategoryRequest {
  householdId: string;
  name: string;
  icon: string | null;
}

@Injectable({ providedIn: 'root' })
export class FinanceApiService {
  constructor(
    private readonly http: HttpClient,
    private readonly authSession: AuthSessionService,
  ) {}

  getSnapshot(householdId = this.authSession.householdId()): Observable<FinanceSnapshot> {
    const householdParams = new HttpParams().set('householdId', householdId);

    return forkJoin({
      accounts: this.http.get<FinanceAccount[]>(`${FINANCE_API_BASE_URL}/accounts`, { params: householdParams }),
      categories: this.http.get<FinanceCategory[]>(`${FINANCE_API_BASE_URL}/categories`, { params: householdParams }),
      transactions: this.http.get<FinanceTransaction[]>(`${FINANCE_API_BASE_URL}/transactions`, {
        params: householdParams.set('page', 1).set('pageSize', 8),
      }),
    });
  }

  createAccount(request: Omit<CreateAccountRequest, 'householdId'>): Observable<FinanceAccount> {
    return this.http.post<FinanceAccount>(
      `${FINANCE_API_BASE_URL}/accounts`,
      this.withHousehold<CreateAccountRequest>(request),
    );
  }

  updateAccount(accountId: string, request: Omit<UpdateAccountRequest, 'householdId'>): Observable<FinanceAccount> {
    return this.http.put<FinanceAccount>(
      `${FINANCE_API_BASE_URL}/accounts/${accountId}`,
      this.withHousehold<UpdateAccountRequest>(request),
    );
  }

  createTransaction(request: Omit<CreateTransactionRequest, 'householdId'>): Observable<FinanceTransaction> {
    return this.http.post<FinanceTransaction>(
      `${FINANCE_API_BASE_URL}/transactions`,
      this.withHousehold<CreateTransactionRequest>(request),
    );
  }

  createCategory(request: Omit<CreateCategoryRequest, 'householdId'>): Observable<FinanceCategory> {
    return this.http.post<FinanceCategory>(
      `${FINANCE_API_BASE_URL}/categories`,
      this.withHousehold<CreateCategoryRequest>(request),
    );
  }

  updateCategory(categoryId: string, request: Omit<UpdateCategoryRequest, 'householdId'>): Observable<FinanceCategory> {
    return this.http.put<FinanceCategory>(
      `${FINANCE_API_BASE_URL}/categories/${categoryId}`,
      this.withHousehold<UpdateCategoryRequest>(request),
    );
  }

  archiveAccount(accountId: string, householdId = this.authSession.householdId()): Observable<FinanceAccount> {
    const params = new HttpParams().set('householdId', householdId);

    return this.http.delete<FinanceAccount>(`${FINANCE_API_BASE_URL}/accounts/${accountId}`, { params });
  }

  private withHousehold<T extends { householdId: string }>(request: Omit<T, 'householdId'>): T {
    return {
      ...request,
      householdId: this.authSession.householdId(),
    } as T;
  }
}
