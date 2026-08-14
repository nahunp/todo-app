import { HttpInterceptorFn, HttpRequest, HttpHandler } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';
import { Router } from '@angular/router';

export const authInterceptor: HttpInterceptorFn = (req: HttpRequest<any>, next: HttpHandler) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const token = auth.getToken();
  const authReq = token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;

  return next(authReq).pipe(
    catchError((err) => {
      if (err?.status === 401) {
        // Clear token and redirect to login for auth-required routes
        auth.logout();
        // Navigation on error — best-effort
        try { router.navigate(['/login']); } catch {}
      }
      return throwError(() => err);
    })
  );
};
