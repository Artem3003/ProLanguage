import { Component, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../services/auth.service';
import { GoogleAuthService } from '../services/google-auth.service';
import { AppleAuthService } from '../services/apple-auth.service';
import { RegisterRequest } from '../models/auth.model';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [RouterLink, FormsModule, CommonModule],
  templateUrl: './register.html',
  styleUrl: './register.scss',
})
export class RegisterComponent implements AfterViewInit {
  @ViewChild('googleButton') googleButtonRef!: ElementRef;

  firstName = '';
  lastName = '';
  email = '';
  phone = '';
  password = '';
  repeatPassword = '';
  showPassword = false;
  showRepeatPassword = false;
  isLoading = false;
  isAppleLoading = false;
  errorMessage = '';
  fieldErrors: Record<string, string[]> = {};
  showSuccessPopup = false;

  constructor(
    private authService: AuthService,
    private googleAuthService: GoogleAuthService,
    private appleAuthService: AppleAuthService,
    private router: Router
  ) {}

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
    this.router.navigate(['/courses']);
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
    this.router.navigate(['/courses']);
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

  toggleRepeatPasswordVisibility(): void {
    this.showRepeatPassword = !this.showRepeatPassword;
  }

  onSubmit(): void {
    if (!this.firstName || !this.email || !this.password || !this.repeatPassword) {
      this.errorMessage = 'Please fill in all fields';
      return;
    }

    if (this.password !== this.repeatPassword) {
      this.errorMessage = 'Passwords do not match';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.fieldErrors = {};

    const request: RegisterRequest = {
      email: this.email,
      password: this.password,
      confirmPassword: this.repeatPassword,
      firstName: this.firstName,
      lastName: this.lastName,
      phoneNumber: this.phone || undefined
    };

    this.authService.register(request).subscribe({
      next: (response) => {
        this.isLoading = false;
        if (response.isSuccess) {
          this.showSuccessPopup = true;
        } else {
          this.errorMessage = response.errorMessage || 'Registration failed';
        }
      },
      error: (error) => {
        this.isLoading = false;
        this.processErrors(error, 'Registration failed');
      }
    });
  }

  closeSuccessPopup(): void {
    this.showSuccessPopup = false;
    this.router.navigate(['/signin']);
  }

  private processErrors(error: any, fallback: string): void {
    this.errorMessage = '';
    this.fieldErrors = {};

    if (error.error) {
      if (typeof error.error === 'string') {
        this.errorMessage = error.error;
        return;
      }
      if (error.error.errorMessage) {
        this.errorMessage = error.error.errorMessage;
        return;
      }
      if (error.error.errors) {
        const errors = error.error.errors;
        for (const key of Object.keys(errors)) {
          const lowerKey = key.toLowerCase();
          const fieldErrs = errors[key];
          this.fieldErrors[lowerKey] = Array.isArray(fieldErrs) ? fieldErrs : [fieldErrs];
        }
        return;
      }
      if (error.error.title) {
        this.errorMessage = error.error.title;
        return;
      }
      if (error.error.message) {
        this.errorMessage = error.error.message;
        return;
      }
    }
    if (error.message) {
      this.errorMessage = error.message;
      return;
    }
    if (error.statusText && error.statusText !== 'OK') {
      this.errorMessage = `${fallback}: ${error.statusText}`;
      return;
    }
    this.errorMessage = fallback;
  }
}
