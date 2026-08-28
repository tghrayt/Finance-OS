import { Routes } from '@angular/router';

import { LoginPageComponent } from './features/auth/login-page.component';
import { DashboardShellComponent } from './features/shell/dashboard-shell.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  { path: 'login', component: LoginPageComponent },
  { path: 'dashboard', component: DashboardShellComponent },
  { path: 'accounts', component: DashboardShellComponent },
  { path: 'transactions', component: DashboardShellComponent },
  { path: 'budgets', component: DashboardShellComponent },
  { path: 'categories', component: DashboardShellComponent },
  { path: 'notifications', component: DashboardShellComponent },
  { path: '**', redirectTo: 'dashboard' },
];
