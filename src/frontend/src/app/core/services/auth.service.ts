import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { ApiEnvelope, AuthTokens, AuthenticatedUser, CreateUserRequest, LoginRequest, UserRole } from './auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly httpClient = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly accessTokenKey = 'ims_access_token';
  private readonly refreshTokenKey = 'ims_refresh_token';
  private readonly userKey = 'ims_user';

  private readonly apiBaseUrl = '/api';

  private readonly currentUserSignal = signal<AuthenticatedUser | null>(this.readStoredUser());

  readonly currentUser = computed(() => this.currentUserSignal());
  readonly isAuthenticated = computed(() => this.currentUserSignal() !== null);
  readonly currentRole = computed(() => this.currentUserSignal()?.role ?? null);

  async login(request: LoginRequest): Promise<void> {
    const response = await firstValueFrom(
      this.httpClient.post<ApiEnvelope<AuthTokens>>(`${this.apiBaseUrl}/auth/login`, request)
    );

    this.persistSession(response.data);
  }

  async loadCurrentUser(): Promise<void> {
    if (!this.getAccessToken()) {
      return;
    }

    try {
      const response = await firstValueFrom(
        this.httpClient.get<ApiEnvelope<AuthenticatedUser>>(`${this.apiBaseUrl}/auth/me`)
      );

      this.currentUserSignal.set(response.data);
      localStorage.setItem(this.userKey, JSON.stringify(response.data));
    } catch {
      await this.logout(false);
    }
  }

  async refreshSession(): Promise<boolean> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      return false;
    }

    try {
      const response = await firstValueFrom(
        this.httpClient.post<ApiEnvelope<AuthTokens>>(`${this.apiBaseUrl}/auth/refresh`, { refreshToken })
      );

      this.persistSession(response.data);
      return true;
    } catch {
      await this.logout(false);
      return false;
    }
  }

  async logout(redirectToLogin = true): Promise<void> {
    const refreshToken = this.getRefreshToken();

    if (refreshToken) {
      try {
        await firstValueFrom(this.httpClient.post(`${this.apiBaseUrl}/auth/logout`, { refreshToken }));
      } catch {
        // Ignore logout API failures and clear local session state.
      }
    }

    this.clearSession();

    if (redirectToLogin) {
      await this.router.navigate(['/auth/login']);
    }
  }

  canAccessRole(allowedRoles: UserRole[]): boolean {
    const role = this.currentRole();
    return !!role && allowedRoles.includes(role);
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.accessTokenKey);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.refreshTokenKey);
  }

  async createUser(request: CreateUserRequest): Promise<AuthenticatedUser> {
    const response = await firstValueFrom(
      this.httpClient.post<ApiEnvelope<AuthenticatedUser>>(`${this.apiBaseUrl}/users`, request)
    );

    return response.data;
  }

  private persistSession(authTokens: AuthTokens): void {
    localStorage.setItem(this.accessTokenKey, authTokens.accessToken);
    localStorage.setItem(this.refreshTokenKey, authTokens.refreshToken);
    localStorage.setItem(this.userKey, JSON.stringify(authTokens.user));
    this.currentUserSignal.set(authTokens.user);
  }

  private clearSession(): void {
    localStorage.removeItem(this.accessTokenKey);
    localStorage.removeItem(this.refreshTokenKey);
    localStorage.removeItem(this.userKey);
    this.currentUserSignal.set(null);
  }

  private readStoredUser(): AuthenticatedUser | null {
    const rawValue = localStorage.getItem(this.userKey);
    if (!rawValue) {
      return null;
    }

    try {
      return JSON.parse(rawValue) as AuthenticatedUser;
    } catch {
      localStorage.removeItem(this.userKey);
      return null;
    }
  }
}
