import { Routes } from '@angular/router';

import { SectionRouteComponent } from './core/routing/section-route.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  { path: 'dashboard', component: SectionRouteComponent },
  { path: 'accounts', component: SectionRouteComponent },
  { path: 'transactions', component: SectionRouteComponent },
  { path: 'budgets', component: SectionRouteComponent },
  { path: 'notifications', component: SectionRouteComponent },
  { path: '**', redirectTo: 'dashboard' },
];
