import { Injectable, NgZone } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

declare const AppleID: any;

export interface AppleSignInResponse {
  authorization: {
    code: string;
    id_token: string;
    state?: string;
  };
  user?: {
    email: string;
    name?: {
      firstName: string;
      lastName: string;
    };
  };
}

@Injectable({
  providedIn: 'root'
})
export class AppleAuthService {
  private clientId = environment.appleClientId;
  private redirectUri = environment.appleRedirectUri;
  private isInitialized = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private ngZone: NgZone
  ) {}

  /**
   * Initialize Apple Sign-In
   */
  private initialize(): void {
    if (this.isInitialized || typeof AppleID === 'undefined') {
      return;
    }

    AppleID.auth.init({
      clientId: this.clientId,
      scope: 'name email',
      redirectURI: this.redirectUri,
      usePopup: true,
    });

    this.isInitialized = true;
  }

  /**
   * Trigger Apple Sign-In
   * @param onSuccess Callback for successful login
   * @param onError Callback for login error
   */
  async signIn(
    onSuccess: () => void,
    onError: (error: string) => void
  ): Promise<void> {
    if (typeof AppleID === 'undefined') {
      console.error('Apple Sign-In SDK not loaded');
      onError('Apple Sign-In is not available. Please try again later.');
      return;
    }

    this.initialize();

    try {
      const response: AppleSignInResponse = await AppleID.auth.signIn();

      if (!response.authorization?.id_token) {
        this.ngZone.run(() => {
          onError('No token received from Apple.');
        });
        return;
      }

      // Extract user info if available (only on first authorization)
      const firstName = response.user?.name?.firstName;
      const lastName = response.user?.name?.lastName;

      // Send the ID token to our backend
      this.authService.externalLogin({
        provider: 'Apple',
        idToken: response.authorization.id_token,
        firstName: firstName,
        lastName: lastName,
      }).subscribe({
        next: (authResponse) => {
          this.ngZone.run(() => {
            if (authResponse.isSuccess) {
              onSuccess();
            } else {
              onError(authResponse.errorMessage || 'Apple sign-in failed.');
            }
          });
        },
        error: (error) => {
          this.ngZone.run(() => {
            const message = error.error?.errorMessage || error.error?.message || 'Apple sign-in failed.';
            onError(message);
          });
        }
      });
    } catch (error: any) {
      this.ngZone.run(() => {
        // User cancelled or other error
        if (error?.error === 'popup_closed_by_user') {
          // User cancelled - don't show error
          return;
        }
        const message = error?.error || 'Apple sign-in failed.';
        onError(message);
      });
    }
  }
}
