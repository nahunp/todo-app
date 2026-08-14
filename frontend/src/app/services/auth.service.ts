import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs/operators';
import { runtimeConfig } from '../runtime-config';

export interface AuthResponse {
  accessToken?: string;
  expiresAt?: string | null;
}

// Matches WebApi's PasswordPolicyResponse exactly (AuthEndpoints.cs) —
// fetched, not hardcoded, so the frontend's requirements checklist can
// never drift from what the backend actually enforces.
export interface PasswordPolicy {
  requiredLength: number;
  requireDigit: boolean;
  requireLowercase: boolean;
  requireUppercase: boolean;
  requireNonAlphanumeric: boolean;
  requiredUniqueChars: number;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly storageKey = 'todoapp_token';
  private http = inject(HttpClient);
  // Getter, not a cached field — runtimeConfig.apiBaseUrl is resolved by
  // main.ts before bootstrap, but reading it lazily here removes any
  // dependency on construction order being right.
  private get base() { return `${runtimeConfig.apiBaseUrl}/api/v1/auth`; }

  // Simple reactive state for whether a token exists
  isAuthenticated = signal<boolean>(!!this.getToken());

  // Register: backend returns 201 empty body — don't expect a token here.
  // captchaToken comes from the Turnstile widget on the register form —
  // backend rejects the request (400) without a token that verifies.
  register(email: string, password: string, captchaToken: string) {
    return this.http.post<void>(`${this.base}/register`, { email, password, captchaToken });
  }

  getPasswordPolicy() {
    return this.http.get<PasswordPolicy>(`${this.base}/password-policy`);
  }

  login(email: string, password: string) {
    return this.http.post<AuthResponse>(`${this.base}/login`, { email, password }).pipe(
      tap(res => this.setToken(res?.accessToken))
    );
  }

  logout() {
    localStorage.removeItem(this.storageKey);
    this.isAuthenticated.set(false);
  }

  private setToken(token?: string | null) {
    if (token) {
      localStorage.setItem(this.storageKey, token);
      this.isAuthenticated.set(true);
    } else {
      localStorage.removeItem(this.storageKey);
      this.isAuthenticated.set(false);
    }
  }

  getToken(): string | null {
    return localStorage.getItem(this.storageKey);
  }
}
