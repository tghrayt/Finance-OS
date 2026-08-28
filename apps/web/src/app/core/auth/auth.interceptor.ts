import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { from, switchMap } from 'rxjs';

import { AuthSessionService } from './auth-session.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const authSession = inject(AuthSessionService);
  const router = inject(Router);

  if (!request.url.startsWith('/api/') || request.headers.has('Authorization')) {
    return next(request);
  }

  if (!authSession.getAccessToken()) {
    return next(request);
  }

  return from(authSession.getAccessTokenForApi()).pipe(
    switchMap((accessToken) => {
      if (!accessToken) {
        void router.navigate(['/login']);
        return next(request);
      }

      return next(
        request.clone({
          setHeaders: {
            Authorization: `Bearer ${accessToken}`,
          },
        }),
      );
    }),
  );
};
