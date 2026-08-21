import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';

import { AuthProviderKind } from '../../core/auth/auth-provider.models';
import { AuthSessionService } from '../../core/auth/auth-session.service';

@Component({
  selector: 'app-login-page',
  templateUrl: './login-page.component.html',
  styleUrl: './login-page.component.scss',
})
export class LoginPageComponent {
  protected readonly authProviders = inject(AuthSessionService).providerOptions;
  private readonly authSession = inject(AuthSessionService);
  private readonly router = inject(Router);

  constructor() {
    void this.initializeLogin();
  }

  protected async continueWithProvider(provider: AuthProviderKind): Promise<void> {
    await this.authSession.signInWithProvider(provider);
  }

  private async initializeLogin(): Promise<void> {
    await this.authSession.initializeHostedAuth();

    if (this.authSession.session()) {
      await this.router.navigate(['/dashboard']);
    }
  }
}

