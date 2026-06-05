import { Injectable, NgZone } from '@angular/core';
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
    private ngZone: NgZone
  ) {}

  /**
   * Initialize and render the official Google Identity Services
   * "Sign in with Google" button into the given element.
   * Uses the ID-token credential flow (no popup / One Tap).
   * @param buttonElement The element the Google button is rendered into
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
   * Programmatically trigger the hidden official Google button so a custom-styled
   * button can reuse the working ID-token flow (no OAuth popup fallback).
   * Forwarding the click within the user's gesture keeps it a trusted interaction.
   * @param containerElement The element the Google button was rendered into
   * @param onError Callback if the Google button has not rendered yet
   */
  triggerGoogleSignIn(
    containerElement: HTMLElement,
    onError: (error: string) => void
  ): void {
    const googleButton =
      containerElement.querySelector<HTMLElement>('div[role="button"]') ??
      containerElement.querySelector<HTMLElement>('[role="button"]') ??
      (containerElement.firstElementChild as HTMLElement | null);

    if (!googleButton) {
      console.error('Google Sign-In button is not rendered yet');
      onError('Google Sign-In is not ready yet. Please try again in a moment.');
      return;
    }

    googleButton.click();
  }

  /**
   * Handle the credential (ID token) response from the Google button.
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
