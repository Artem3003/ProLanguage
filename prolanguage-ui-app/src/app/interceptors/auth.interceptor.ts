import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

let isRefreshing = false;

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getAccessToken();

  // Skip auth header for public auth endpoints (login, register, refresh-token)
  const publicAuthEndpoints = ['/api/auth/login', '/api/auth/register', '/api/auth/refresh-token'];
  const isPublicAuthEndpoint = publicAuthEndpoints.some(endpoint => req.url.includes(endpoint));

  if (isPublicAuthEndpoint) {
    return next(req);
  }

  let authReq = req;
  if (token) {
    authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      // Only attempt refresh for 401 errors on Auth API endpoints
      const isAuthApiRequest = req.url.includes('/api/auth') ||
                               req.url.includes('/api/users') ||
                               req.url.includes('/api/roles');

      if (error.status === 401 && !isRefreshing && isAuthApiRequest) {
        isRefreshing = true;

        return authService.refreshToken().pipe(
          switchMap(response => {
            isRefreshing = false;
            if (response.isSuccess) {
              const newToken = authService.getAccessToken();
              const newReq = req.clone({
                setHeaders: {
                  Authorization: `Bearer ${newToken}`
                }
              });
              return next(newReq);
            }
            return throwError(() => error);
          }),
          catchError(refreshError => {
            isRefreshing = false;
            authService.logout();
            return throwError(() => refreshError);
          })
        );
      }

      return throwError(() => error);
    })
  );
};
