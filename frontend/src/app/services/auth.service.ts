import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs/operators';

export interface AuthResponse {
  token: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly storageKey = 'todoapp_token';
  private http = new HttpClient((null as any)); // placeholder for typing in environments without DI tooling

  // Simple reactive state for whether a token exists
  isAuthenticated = signal<boolean>(!!this.getToken());

  constructor(http: HttpClient) {
    this.http = http;
  }

  register(username: string, password: string) {
    return this.http.post<AuthResponse>('/api/v1/auth/register', { username, password }).pipe(
      tap(res => this.setToken(res?.token))
    );
  }

  login(username: string, password: string) {
    return this.http.post<AuthResponse>('/api/v1/auth/login', { username, password }).pipe(
      tap(res => this.setToken(res?.token))
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
