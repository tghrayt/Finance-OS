import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { DashboardDataService } from './dashboard-data.service';

describe('DashboardDataService', () => {
  let service: DashboardDataService;
  let http: HttpTestingController;

  beforeEach(() => {
    localStorage.setItem(
      'financeos.auth.session',
      JSON.stringify({
        mode: 'demo',
        email: 'demo@financeos.local',
        householdId: 'household-1',
        accessToken: null,
      }),
    );

    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(DashboardDataService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
    localStorage.clear();
  });

  it('should compose loading and ready dashboard states', () => {
    const states: string[] = [];
    service.load().subscribe((state) => states.push(state.status));

    const now = new Date();
    http.expectOne('/api/v1/finance/accounts?householdId=household-1').flush([]);
    http.expectOne('/api/v1/finance/categories?householdId=household-1').flush([]);
    http.expectOne('/api/v1/finance/transactions?householdId=household-1&page=1&pageSize=8').flush([]);
    http
      .expectOne(`/api/v1/budget/monthly-budgets/current?householdId=household-1&year=${now.getFullYear()}&month=${now.getMonth() + 1}`)
      .flush('Budget was not found.', { status: 404, statusText: 'Not Found' });
    http.expectOne('/api/v1/notification/in-app?householdId=household-1&page=1&pageSize=6').flush([]);

    expect(states).toEqual(['loading', 'ready']);
  });
});
