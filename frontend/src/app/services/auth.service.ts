import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs/operators';
import { runtimeConfig } from '../runtime-config';

export interface AuthResponse {
  accessToken?: string;
  expiresAt?: string | null;
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

  // Register: backend returns 201 empty body — don't expect a token here
  register(email: string, password: string) {
    return this.http.post<void>(`${this.base}/register`, { email, password });
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
