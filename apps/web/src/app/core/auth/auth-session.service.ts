import { Injectable, signal } from '@angular/core';

const DEMO_HOUSEHOLD_ID = '00000000-0000-0000-0000-000000000000';
const STORAGE_KEY = 'financeos.auth.session';

export interface AuthSession {
  mode: 'demo' | 'authenticated';
  email: string;
  householdId: string;
  accessToken: string | null;
}

@Injectable({ providedIn: 'root' })
export class AuthSessionService {
  private readonly currentSession = signal<AuthSession | null>(this.readStoredSession());

  readonly session = this.currentSession.asReadonly();

  readonly householdId = signal(this.currentSession()?.householdId ?? DEMO_HOUSEHOLD_ID);

  startDemoSession(email: string): void {
    this.setSession({
      mode: 'demo',
      email,
      householdId: DEMO_HOUSEHOLD_ID,
      accessToken: null,
    });
  }

  clearSession(): void {
    this.currentSession.set(null);
    this.householdId.set(DEMO_HOUSEHOLD_ID);
    localStorage.removeItem(STORAGE_KEY);
  }

  getAccessToken(): string | null {
    return this.currentSession()?.accessToken ?? null;
  }

  private setSession(session: AuthSession): void {
    this.currentSession.set(session);
    this.householdId.set(session.householdId);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
  }

  private readStoredSession(): AuthSession | null {
    const rawSession = localStorage.getItem(STORAGE_KEY);
    if (!rawSession) {
      return null;
    }

    try {
      const session = JSON.parse(rawSession) as Partial<AuthSession>;
      if (!session.email || !session.householdId || (session.mode !== 'demo' && session.mode !== 'authenticated')) {
        return null;
      }

      return {
        mode: session.mode,
        email: session.email,
        householdId: session.householdId,
        accessToken: session.accessToken ?? null,
      };
    } catch {
      return null;
    }
  }
}
