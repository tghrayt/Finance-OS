import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';

import { AuthSessionService } from './auth-session.service';

describe('AuthSessionService', () => {
  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
  });

  it('should persist a demo session', () => {
    const service = TestBed.inject(AuthSessionService);

    service.startDemoSession('demo@financeos.local');

    expect(service.session()?.email).toBe('demo@financeos.local');
    expect(service.session()?.mode).toBe('demo');
    expect(service.householdId()).toBe('00000000-0000-0000-0000-000000000000');
  });

  it('should clear the active session', () => {
    const service = TestBed.inject(AuthSessionService);
    service.startDemoSession('demo@financeos.local');

    service.clearSession();

    expect(service.session()).toBeNull();
    expect(localStorage.getItem('financeos.auth.session')).toBeNull();
  });
});
