import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { LoginPageComponent } from './features/auth/login-page.component';
import { DashboardShellComponent } from './features/shell/dashboard-shell.component';

describe('DashboardShellComponent', () => {
  let http: HttpTestingController;

  beforeEach(async () => {
    localStorage.clear();

    await TestBed.configureTestingModule({
      imports: [DashboardShellComponent],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter(routes)],
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
  });

  it('should create the app', async () => {
    setDemoSession();
    const fixture = TestBed.createComponent(DashboardShellComponent);
    fixture.detectChanges();
    flushFinanceSnapshot();

    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render the finance dashboard title', async () => {
    setDemoSession();
    const fixture = TestBed.createComponent(DashboardShellComponent);
    fixture.detectChanges();
    flushFinanceSnapshot();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('Pilotage financier personnel');
  });

  it('should render an empty accounts state when the API returns no accounts', async () => {
    setDemoSession();
    const fixture = TestBed.createComponent(DashboardShellComponent);
    fixture.detectChanges();
    flushFinanceSnapshot();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Aucun compte pour ce foyer.');
  });

  it('should keep dashboard content hidden while checking an anonymous session', () => {
    const fixture = TestBed.createComponent(DashboardShellComponent);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.session-gate')).toBeTruthy();
    expect(compiled.textContent).not.toContain('Pilotage financier personnel');
    expect(http.match((request) => request.url.startsWith('/api/v1/')).length).toBe(0);

    flushAuthConfig();
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

describe('LoginPageComponent', () => {
  let http: HttpTestingController;

  beforeEach(async () => {
    localStorage.clear();

    await TestBed.configureTestingModule({
      imports: [LoginPageComponent],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter(routes)],
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
  });

  it('should render a standalone login page without setup copy or demo form', () => {
    const fixture = TestBed.createComponent(LoginPageComponent);
    fixture.detectChanges();
    flushAuthConfig();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Connexion securisee');
    expect(compiled.textContent).not.toContain('Microsoft Entra External ID sera active apres configuration.');
    expect(compiled.textContent).not.toContain('Entrer en mode demo');
  });

  function flushAuthConfig(): void {
    http.expectOne('/config/auth-config.json').flush({
      enabled: true,
      authority: 'https://financeos.ciamlogin.com/',
      clientId: 'fc46120a-447d-4b74-91cd-b9e059dcc60c',
      redirectUri: 'http://localhost:4200/',
      postLogoutRedirectUri: 'http://localhost:4200/',
      knownAuthorities: ['financeos.ciamlogin.com'],
      scopes: ['openid', 'profile', 'email'],
    });
  }
});
