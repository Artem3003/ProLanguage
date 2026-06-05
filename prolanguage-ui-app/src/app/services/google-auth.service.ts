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
    // The GIS script is loaded async/defer, so it may not be ready yet when the
    // view initializes. Wait for it instead of failing on the first attempt.
    this.whenGoogleReady(
      () => this.renderButton(buttonElement, onSuccess, onError),
      () => {
        console.error('Google Identity Services failed to load');
        onError('Google Sign-In is not available. Please try again later.');
      }
    );
  }

  /**
   * Render the official Google button. It is overlaid transparently on top of the
   * custom button (see component templates/styles) so a genuine user click reaches
   * Google directly — programmatically clicking a hidden button is rejected by FedCM.
   */
  private renderButton(
    buttonElement: HTMLElement,
    onSuccess: () => void,
    onError: (error: string) => void
  ): void {
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
   * Invoke onReady once the asynchronously-loaded GIS library is available,
   * or onTimeout if it never loads within the polling window.
   */
  private whenGoogleReady(onReady: () => void, onTimeout: () => void): void {
    const isReady = (): boolean =>
      typeof google !== 'undefined' && !!google.accounts?.id;

    if (isReady()) {
      onReady();
      return;
    }

    const intervalMs = 100;
    const timeoutMs = 5000;
    let waited = 0;
    const timer = setInterval(() => {
      if (isReady()) {
        clearInterval(timer);
        onReady();
      } else if ((waited += intervalMs) >= timeoutMs) {
        clearInterval(timer);
        onTimeout();
      }
    }, intervalMs);
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
