import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

const BUDGET_API_BASE_URL = '/api/v1/budget';

export interface BudgetAllocation {
  allocationId: string;
  categoryId: string;
  plannedAmount: number;
  actualAmount: number;
  currency: string;
  consumptionRatio: number;
}

export interface MonthlyBudget {
  budgetId: string;
  householdId: string;
  year: number;
  month: number;
  totalBudget: number;
  actualAmount: number;
  currency: string;
  consumptionRatio: number;
  allocations: BudgetAllocation[];
}

export interface CreateMonthlyBudgetRequest {
  householdId: string;
  year: number;
  month: number;
  totalBudget: number;
  currency: string;
}

export interface SetBudgetAllocationRequest {
  plannedAmount: number;
}

@Injectable({ providedIn: 'root' })
export class BudgetApiService {
  constructor(private readonly http: HttpClient) {}

  getCurrentBudget(householdId: string, year: number, month: number): Observable<MonthlyBudget> {
    const params = new HttpParams()
      .set('householdId', householdId)
      .set('year', year)
      .set('month', month);

    return this.http.get<MonthlyBudget>(`${BUDGET_API_BASE_URL}/monthly-budgets/current`, { params });
  }

  createMonthlyBudget(request: CreateMonthlyBudgetRequest): Observable<MonthlyBudget> {
    return this.http.post<MonthlyBudget>(`${BUDGET_API_BASE_URL}/monthly-budgets`, request);
  }

  setAllocation(budgetId: string, categoryId: string, request: SetBudgetAllocationRequest): Observable<MonthlyBudget> {
    return this.http.put<MonthlyBudget>(`${BUDGET_API_BASE_URL}/monthly-budgets/${budgetId}/allocations/${categoryId}`, request);
  }
}
