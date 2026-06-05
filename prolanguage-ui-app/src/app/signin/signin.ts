import { Component, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../services/auth.service';
import { GoogleAuthService } from '../services/google-auth.service';
import { AppleAuthService } from '../services/apple-auth.service';
import { LoginRequest } from '../models/auth.model';

@Component({
  selector: 'app-signin',
  standalone: true,
  imports: [RouterLink, FormsModule, CommonModule],
  templateUrl: './signin.html',
  styleUrl: './signin.scss',
})
export class SigninComponent implements AfterViewInit {
  @ViewChild('googleButton') googleButtonRef!: ElementRef;

  email = '';
  password = '';
  rememberMe = true;
  showPassword = false;
  isLoading = false;
  isAppleLoading = false;
  errorMessage = '';
  private returnUrl = '/courses';

  constructor(
    private authService: AuthService,
    private googleAuthService: GoogleAuthService,
    private appleAuthService: AppleAuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/courses';
  }

  ngAfterViewInit(): void {
    // Initialize Google Sign-In button after view is ready
    setTimeout(() => {
      if (this.googleButtonRef?.nativeElement) {
        this.googleAuthService.initializeGoogleSignIn(
          this.googleButtonRef.nativeElement,
          () => this.onGoogleSuccess(),
          (error) => this.onGoogleError(error)
        );
      }
    }, 100);
  }

  onGoogleSignIn(): void {
    this.errorMessage = '';
    this.googleAuthService.triggerGoogleSignIn(
      this.googleButtonRef.nativeElement,
      (error) => this.onGoogleError(error)
    );
  }

  private onGoogleSuccess(): void {
    this.router.navigate([this.returnUrl]);
  }

  private onGoogleError(error: string): void {
    this.errorMessage = error;
  }

  onAppleSignIn(): void {
    this.isAppleLoading = true;
    this.errorMessage = '';
    this.appleAuthService.signIn(
      () => this.onAppleSuccess(),
      (error) => this.onAppleError(error)
    );
  }

  private onAppleSuccess(): void {
    this.isAppleLoading = false;
    this.router.navigate([this.returnUrl]);
  }

  private onAppleError(error: string): void {
    this.isAppleLoading = false;
    if (error) {
      this.errorMessage = error;
    }
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  onSubmit(): void {
    if (!this.email || !this.password) {
      this.errorMessage = 'Please enter email and password';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const request: LoginRequest = {
      email: this.email,
      password: this.password,
      rememberMe: this.rememberMe
    };

    this.authService.login(request).subscribe({
      next: (response) => {
        this.isLoading = false;
        if (response.isSuccess) {
          this.router.navigate([this.returnUrl]);
        } else {
          this.errorMessage = response.errorMessage || 'Login failed';
        }
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = this.extractErrorMessage(error, 'Login failed');
      }
    });
  }

  private extractErrorMessage(error: any, fallback: string): string {
    // Try to get detailed error message from various response formats
    if (error.error) {
      if (typeof error.error === 'string') {
        return error.error;
      }
      if (error.error.errorMessage) {
        return error.error.errorMessage;
      }
      if (error.error.errors) {
        // Handle validation errors object
        const errors = error.error.errors;
        const messages: string[] = [];
        for (const key of Object.keys(errors)) {
          const fieldErrors = errors[key];
          if (Array.isArray(fieldErrors)) {
            messages.push(...fieldErrors);
          } else if (typeof fieldErrors === 'string') {
            messages.push(fieldErrors);
          }
        }
        if (messages.length > 0) {
          return messages.join('. ');
        }
      }
      if (error.error.title) {
        return error.error.title;
      }
      if (error.error.message) {
        return error.error.message;
      }
    }
    if (error.message) {
      return error.message;
    }
    if (error.statusText && error.statusText !== 'OK') {
      return `${fallback}: ${error.statusText}`;
    }
    return fallback;
  }
}
