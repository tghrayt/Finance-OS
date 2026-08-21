import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';

import { App } from './app';
import { routes } from './app.routes';

describe('App', () => {
  let http: HttpTestingController;

  beforeEach(async () => {
    localStorage.clear();

    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter(routes)],
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
  });

  it('should create the app', () => {
    setDemoSession();
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    flushAuthConfig();
    flushFinanceSnapshot();

    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render the finance dashboard title', () => {
    setDemoSession();
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    flushAuthConfig();
    flushFinanceSnapshot();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('Pilotage financier personnel');
  });

  it('should render an empty accounts state when the API returns no accounts', () => {
    setDemoSession();
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    flushAuthConfig();
    flushFinanceSnapshot();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Aucun compte pour ce foyer.');
  });

  function setDemoSession(): void {
    localStorage.setItem(
      'financeos.auth.session',
      JSON.stringify({
        mode: 'demo',
        email: 'demo@financeos.local',
        householdId: '00000000-0000-0000-0000-000000000000',
        accessToken: null,
      }),
    );
  }

  function flushAuthConfig(): void {
    http.expectOne('/config/auth-config.json').flush({
      enabled: false,
      authority: '',
      clientId: '',
      redirectUri: 'http://localhost:4200/',
      postLogoutRedirectUri: 'http://localhost:4200/',
      knownAuthorities: [],
      scopes: ['openid', 'profile', 'email'],
    });
  }

  function flushFinanceSnapshot(): void {
    const now = new Date();
    const year = now.getFullYear();
    const month = now.getMonth() + 1;
    http.expectOne('/api/v1/finance/accounts?householdId=00000000-0000-0000-0000-000000000000').flush([]);
    http.expectOne('/api/v1/finance/categories?householdId=00000000-0000-0000-0000-000000000000').flush([]);
    http
      .expectOne('/api/v1/finance/transactions?householdId=00000000-0000-0000-0000-000000000000&page=1&pageSize=8')
      .flush([]);
    http
      .expectOne(`/api/v1/budget/monthly-budgets/current?householdId=00000000-0000-0000-0000-000000000000&year=${year}&month=${month}`)
      .flush('Budget was not found.', { status: 404, statusText: 'Not Found' });
    http
      .expectOne('/api/v1/notification/in-app?householdId=00000000-0000-0000-0000-000000000000&page=1&pageSize=6')
      .flush([]);
  }
});
