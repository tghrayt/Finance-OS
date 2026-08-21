import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { AuthSessionService } from '../core/auth/auth-session.service';

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
  constructor(
    private readonly http: HttpClient,
    private readonly authSession: AuthSessionService,
  ) {}

  getCurrentBudget(year: number, month: number, householdId = this.authSession.householdId()): Observable<MonthlyBudget> {
    const params = new HttpParams()
      .set('householdId', householdId)
      .set('year', year)
      .set('month', month);

    return this.http.get<MonthlyBudget>(`${BUDGET_API_BASE_URL}/monthly-budgets/current`, { params });
  }

  createMonthlyBudget(request: Omit<CreateMonthlyBudgetRequest, 'householdId'>): Observable<MonthlyBudget> {
    return this.http.post<MonthlyBudget>(`${BUDGET_API_BASE_URL}/monthly-budgets`, {
      ...request,
      householdId: this.authSession.householdId(),
    });
  }

  setAllocation(
    budgetId: string,
    categoryId: string,
    request: SetBudgetAllocationRequest,
    householdId = this.authSession.householdId(),
  ): Observable<MonthlyBudget> {
    const params = new HttpParams().set('householdId', householdId);

    return this.http.put<MonthlyBudget>(
      `${BUDGET_API_BASE_URL}/monthly-budgets/${budgetId}/allocations/${categoryId}`,
      request,
      { params },
    );
  }
}
