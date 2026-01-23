import { Component } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../services/auth.service';
import { RegisterRequest } from '../models/auth.model';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [RouterLink, FormsModule, CommonModule],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class RegisterComponent {
  firstName = '';
  lastName = '';
  email = '';
  phone = '';
  password = '';
  repeatPassword = '';
  showPassword = false;
  showRepeatPassword = false;
  isLoading = false;
  errorMessage = '';
  showSuccessPopup = false;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  onGoogleSignIn(): void {
    console.log('Google Sign In clicked');
  }

  onAppleSignIn(): void {
    console.log('Apple Sign In clicked');
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
        this.errorMessage = this.extractErrorMessage(error, 'Registration failed');
      }
    });
  }

  closeSuccessPopup(): void {
    this.showSuccessPopup = false;
    this.router.navigate(['/signin']);
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
