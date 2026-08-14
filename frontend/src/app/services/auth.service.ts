import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs/operators';

export interface AuthResponse {
  accessToken?: string;
  expiresAt?: string | null;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly storageKey = 'todoapp_token';
  private http = inject(HttpClient);

  // Simple reactive state for whether a token exists
  isAuthenticated = signal<boolean>(!!this.getToken());

  // Register: backend returns 201 empty body — don't expect a token here
  register(email: string, password: string) {
    return this.http.post<void>('/api/v1/auth/register', { email, password });
  }

  login(email: string, password: string) {
    return this.http.post<AuthResponse>('/api/v1/auth/login', { email, password }).pipe(
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
