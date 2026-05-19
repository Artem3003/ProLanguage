import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, throwError, BehaviorSubject } from 'rxjs';
import { AuthResponse, LoginRequest, RegisterRequest, RefreshTokenRequest, ExternalLoginRequest, User, ForgotPasswordRequest, ResetPasswordRequest, PasswordResetResponse } from '../models/auth.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = 'http://localhost:5100/api/auth';

  private currentUserSubject = new BehaviorSubject<User | null>(this.getUserFromStorage());
  public currentUser$ = this.currentUserSubject.asObservable();

  private isAuthenticatedSignal = signal(this.hasValidToken());
  public isAuthenticated = this.isAuthenticatedSignal.asReadonly();

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    this.checkTokenExpiration();
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, request).pipe(
      tap(response => {
        if (response.isSuccess) {
          this.setSession(response);
        }
      }),
      catchError(error => {
        return throwError(() => error);
      })
    );
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, request).pipe(
      catchError(error => {
        return throwError(() => error);
      })
    );
  }

  externalLogin(request: ExternalLoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/external-login`, request).pipe(
      tap(response => {
        if (response.isSuccess) {
          this.setSession(response);
        }
      }),
      catchError(error => {
        return throwError(() => error);
      })
    );
  }

  refreshToken(): Observable<AuthResponse> {
    const accessToken = localStorage.getItem('accessToken');
    const refreshToken = localStorage.getItem('refreshToken');
    const request: RefreshTokenRequest = {
      accessToken: accessToken || '',
      refreshToken: refreshToken || ''
    };

    return this.http.post<AuthResponse>(`${this.apiUrl}/refresh-token`, request).pipe(
      tap(response => {
        if (response.isSuccess) {
          this.setSession(response);
        }
      }),
      catchError(error => {
        this.logout();
        return throwError(() => error);
      })
    );
  }

  logout(): void {
    if (typeof localStorage !== 'undefined') {
      const refreshToken = localStorage.getItem('refreshToken');

      if (refreshToken) {
        this.http.post(`${this.apiUrl}/logout`, { refreshToken }).subscribe({
          error: () => {}
        });
      }
    }

    this.clearSession();
    this.router.navigate(['/signin']);
  }

  private setSession(response: AuthResponse): void {
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem('accessToken', response.accessToken);
      localStorage.setItem('refreshToken', response.refreshToken);
      localStorage.setItem('tokenExpiration', response.accessTokenExpiration.toString());
    }

    const user: User = {
      userId: response.userId,
      email: response.email,
      firstName: response.firstName,
      lastName: response.lastName,
      roles: response.roles
    };

    if (typeof localStorage !== 'undefined') {
      localStorage.setItem('user', JSON.stringify(user));
    }

    this.currentUserSubject.next(user);
    this.isAuthenticatedSignal.set(true);
  }

  private clearSession(): void {
    if (typeof localStorage !== 'undefined') {
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('tokenExpiration');
      localStorage.removeItem('user');
    }

    this.currentUserSubject.next(null);
    this.isAuthenticatedSignal.set(false);
  }

  getAccessToken(): string | null {
    if (typeof localStorage === 'undefined') return null;
    return localStorage.getItem('accessToken');
  }

  private getUserFromStorage(): User | null {
    if (typeof localStorage === 'undefined') return null;
    const userJson = localStorage.getItem('user');
    return userJson ? JSON.parse(userJson) : null;
  }

  private hasValidToken(): boolean {
    if (typeof localStorage === 'undefined') return false;
    const token = localStorage.getItem('accessToken');
    const expiration = localStorage.getItem('tokenExpiration');

    if (!token || !expiration) {
      return false;
    }

    return new Date(expiration) > new Date();
  }

  private checkTokenExpiration(): void {
    if (!this.hasValidToken()) {
      this.clearSession();
    }
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  hasRole(role: string): boolean {
    const user = this.getCurrentUser();
    return user?.roles?.includes(role) ?? false;
  }

  isInRole(...roles: string[]): boolean {
    const user = this.getCurrentUser();
    return roles.some(role => user?.roles?.includes(role));
  }

  forgotPassword(request: ForgotPasswordRequest): Observable<PasswordResetResponse> {
    return this.http.post<PasswordResetResponse>(`${this.apiUrl}/forgot-password`, request).pipe(
      catchError(error => {
        return throwError(() => error);
      })
    );
  }

  resetPassword(request: ResetPasswordRequest): Observable<PasswordResetResponse> {
    return this.http.post<PasswordResetResponse>(`${this.apiUrl}/reset-password`, request).pipe(
      catchError(error => {
        return throwError(() => error);
      })
    );
  }
}
