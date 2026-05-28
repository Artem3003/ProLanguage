import { Component, signal, computed } from '@angular/core';
import { RouterOutlet, RouterModule, RouterLink, RouterLinkActive, Router, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CartIcon } from './cart-icon/cart-icon';
import { HeaderComponent } from './header/header';
import { FooterComponent } from './footer/footer';
import { AuthService } from './services/auth.service';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterModule, RouterLink, RouterLinkActive, CommonModule, CartIcon, HeaderComponent, FooterComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('prolanguage-ui-app');
  protected readonly isFullScreenRoute = signal(false);
  protected readonly isAdminPanelOpen = signal(false);
  protected readonly flashMessage = signal('');
  private toastTimeoutId: number | null = null;

  constructor(
    private router: Router,
    private authService: AuthService
  ) {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: NavigationEnd) => {
      const urlWithoutQueryParams = event.urlAfterRedirects.split('?')[0];
      this.isFullScreenRoute.set(
        urlWithoutQueryParams === '/signin'
        || urlWithoutQueryParams === '/register'
        || urlWithoutQueryParams === '/ai-chat'
        || urlWithoutQueryParams === '/forgot-password'
        || urlWithoutQueryParams === '/reset-password'
      );
      this.consumeFlashMessage();
    });
  }

  isAdmin(): boolean {
    return this.authService.hasRole('Admin');
  }

  toggleAdminPanel(): void {
    this.isAdminPanelOpen.update(open => !open);
  }

  private consumeFlashMessage(): void {
    if (typeof sessionStorage === 'undefined' || typeof window === 'undefined') {
      return;
    }

    const message = sessionStorage.getItem('flashSuccessMessage');
    if (!message) {
      return;
    }

    sessionStorage.removeItem('flashSuccessMessage');
    this.flashMessage.set(message);

    if (this.toastTimeoutId !== null) {
      window.clearTimeout(this.toastTimeoutId);
    }

    this.toastTimeoutId = window.setTimeout(() => {
      this.flashMessage.set('');
      this.toastTimeoutId = null;
    }, 4000);
  }
}
