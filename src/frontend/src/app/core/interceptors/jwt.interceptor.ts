import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, from, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const jwtInterceptor: HttpInterceptorFn = (request, next) => {
  const authService = inject(AuthService);

  const accessToken = authService.getAccessToken();
  const isAuthEndpoint = request.url.includes('/api/auth/login') || request.url.includes('/api/auth/refresh');

  const authRequest = !accessToken || isAuthEndpoint
    ? request
    : request.clone({ setHeaders: { Authorization: `Bearer ${accessToken}` } });

  return next(authRequest).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse) || error.status !== 401 || isAuthEndpoint) {
        return throwError(() => error);
      }

      return from(authService.refreshSession()).pipe(
        switchMap((didRefreshSucceed) => {
          if (!didRefreshSucceed) {
            return throwError(() => error);
          }

          const refreshedAccessToken = authService.getAccessToken();
          if (!refreshedAccessToken) {
            return throwError(() => error);
          }

          const retriedRequest = request.clone({
            setHeaders: {
              Authorization: `Bearer ${refreshedAccessToken}`
            }
          });

          return next(retriedRequest);
        })
      );
    })
  );
};
