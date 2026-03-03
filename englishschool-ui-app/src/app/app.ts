import { Component, signal, computed } from '@angular/core';
import { RouterOutlet, RouterModule, Router, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CartIcon } from './cart-icon/cart-icon';
import { HeaderComponent } from './header/header';
import { FooterComponent } from './footer/footer';
import { AuthService } from './services/auth.service';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterModule, CommonModule, CartIcon, HeaderComponent, FooterComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('englishschool-ui-app');
  protected readonly isFullScreenRoute = signal(false);
  protected readonly isAdminPanelOpen = signal(false);

  constructor(
    private router: Router,
    private authService: AuthService
  ) {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: NavigationEnd) => {
      this.isFullScreenRoute.set(event.urlAfterRedirects === '/signin' || event.urlAfterRedirects === '/register');
    });
  }

  isAdmin(): boolean {
    return this.authService.hasRole('Admin');
  }

  toggleAdminPanel(): void {
    this.isAdminPanelOpen.update(open => !open);
  }
}
