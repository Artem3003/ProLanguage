import { Injectable, NgZone } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

declare const google: any;

export interface GoogleUser {
  credential: string;
}

@Injectable({
  providedIn: 'root'
})
export class GoogleAuthService {
  private clientId = environment.googleClientId;

  constructor(
    private authService: AuthService,
    private router: Router,
    private ngZone: NgZone
  ) {}

  /**
   * Initialize Google Sign-In for a button element
   * @param buttonElement The button element to attach Google Sign-In
   * @param onSuccess Callback for successful login
   * @param onError Callback for login error
   */
  initializeGoogleSignIn(
    buttonElement: HTMLElement,
    onSuccess: () => void,
    onError: (error: string) => void
  ): void {
    if (typeof google === 'undefined') {
      console.error('Google Identity Services not loaded');
      onError('Google Sign-In is not available. Please try again later.');
      return;
    }

    google.accounts.id.initialize({
      client_id: this.clientId,
      callback: (response: GoogleUser) => {
        this.handleCredentialResponse(response, onSuccess, onError);
      },
      auto_select: false,
      cancel_on_tap_outside: true,
    });

    google.accounts.id.renderButton(buttonElement, {
      type: 'standard',
      theme: 'outline',
      size: 'large',
      text: 'continue_with',
      shape: 'rectangular',
      width: 400,
    });
  }

  /**
   * Trigger Google One Tap prompt
   * @param onSuccess Callback for successful login
   * @param onError Callback for login error
   */
  promptGoogleSignIn(
    onSuccess: () => void,
    onError: (error: string) => void
  ): void {
    if (typeof google === 'undefined') {
      console.error('Google Identity Services not loaded');
      onError('Google Sign-In is not available. Please try again later.');
      return;
    }

    google.accounts.id.initialize({
      client_id: this.clientId,
      callback: (response: GoogleUser) => {
        this.handleCredentialResponse(response, onSuccess, onError);
      },
    });

    google.accounts.id.prompt((notification: any) => {
      if (notification.isNotDisplayed() || notification.isSkippedMoment()) {
        // Fallback: use popup flow
        this.signInWithPopup(onSuccess, onError);
      }
    });
  }

  /**
   * Sign in using OAuth 2.0 popup flow
   */
  private signInWithPopup(
    onSuccess: () => void,
    onError: (error: string) => void
  ): void {
    const client = google.accounts.oauth2.initTokenClient({
      client_id: this.clientId,
      scope: 'email profile openid',
      callback: async (tokenResponse: any) => {
        if (tokenResponse.access_token) {
          // Get ID token using the access token
          try {
            const response = await fetch('https://www.googleapis.com/oauth2/v3/userinfo', {
              headers: { Authorization: `Bearer ${tokenResponse.access_token}` }
            });
            const userInfo = await response.json();

            // For popup flow, we use the access_token approach
            // Note: This is less secure than ID token flow, consider using ID token
            this.ngZone.run(() => {
              onError('Please use the Google button instead of popup.');
            });
          } catch (error) {
            this.ngZone.run(() => {
              onError('Failed to get user information from Google.');
            });
          }
        }
      },
    });

    client.requestAccessToken();
  }

  /**
   * Handle the credential response from Google
   */
  private handleCredentialResponse(
    response: GoogleUser,
    onSuccess: () => void,
    onError: (error: string) => void
  ): void {
    if (!response.credential) {
      this.ngZone.run(() => {
        onError('No credential received from Google.');
      });
      return;
    }

    // Send the ID token to our backend
    this.authService.externalLogin({
      provider: 'Google',
      idToken: response.credential
    }).subscribe({
      next: (authResponse) => {
        this.ngZone.run(() => {
          if (authResponse.isSuccess) {
            onSuccess();
          } else {
            onError(authResponse.errorMessage || 'Google sign-in failed.');
          }
        });
      },
      error: (error) => {
        this.ngZone.run(() => {
          const message = error.error?.errorMessage || error.error?.message || 'Google sign-in failed.';
          onError(message);
        });
      }
    });
  }
}
