import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { forkJoin, Observable } from 'rxjs';

const FINANCE_API_BASE_URL = '/api/v1/finance';

export interface FinanceAccount {
  accountId: string;
  householdId: string;
  name: string;
  type: string;
  currentBalance: number;
  currency: string;
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

@Injectable({ providedIn: 'root' })
export class FinanceApiService {
  constructor(private readonly http: HttpClient) {}

  getSnapshot(householdId: string): Observable<FinanceSnapshot> {
    const householdParams = new HttpParams().set('householdId', householdId);

    return forkJoin({
      accounts: this.http.get<FinanceAccount[]>(`${FINANCE_API_BASE_URL}/accounts`, { params: householdParams }),
      categories: this.http.get<FinanceCategory[]>(`${FINANCE_API_BASE_URL}/categories`, { params: householdParams }),
      transactions: this.http.get<FinanceTransaction[]>(`${FINANCE_API_BASE_URL}/transactions`, {
        params: householdParams.set('page', 1).set('pageSize', 8),
      }),
    });
  }

  createAccount(request: CreateAccountRequest): Observable<FinanceAccount> {
    return this.http.post<FinanceAccount>(`${FINANCE_API_BASE_URL}/accounts`, request);
  }

  createTransaction(request: CreateTransactionRequest): Observable<FinanceTransaction> {
    return this.http.post<FinanceTransaction>(`${FINANCE_API_BASE_URL}/transactions`, request);
  }

  createCategory(request: CreateCategoryRequest): Observable<FinanceCategory> {
    return this.http.post<FinanceCategory>(`${FINANCE_API_BASE_URL}/categories`, request);
  }
}
