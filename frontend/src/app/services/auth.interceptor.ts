import { HttpInterceptorFn, HttpRequest, HttpHandlerFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';
import { Router } from '@angular/router';

export const authInterceptor: HttpInterceptorFn = (req: HttpRequest<any>, next: HttpHandlerFn) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const token = auth.getToken();
  const authReq = token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;

  return next(authReq).pipe(
    catchError((err) => {
      // A 401 from /auth/login or /auth/register is the expected outcome
      // of bad credentials or a rejected registration — not a sign the
      // user's session expired, since they were never logged in for this
      // request in the first place. Treating it as one used to fire
      // auth.logout() + a redirect back to /login on every failed login
      // attempt (harmless on its own, since /login is already where the
      // user is) but it's the wrong signal, and worth keeping separate
      // from the real "an authenticated request's token was rejected"
      // case below.
      const isAuthRequest = /\/auth\/(login|register)(\?|$)/.test(req.url);
      if (err?.status === 401 && !isAuthRequest) {
        // Clear token and redirect to login for auth-required routes
        auth.logout();
        // Navigation on error — best-effort
        try { router.navigate(['/login']); } catch {}
      }
      return throwError(() => err);
    })
  );
};
