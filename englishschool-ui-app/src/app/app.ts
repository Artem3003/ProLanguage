import { Component, signal, computed } from '@angular/core';
import { RouterOutlet, RouterModule, Router, NavigationEnd } from '@angular/router';
import { CartIcon } from './cart-icon/cart-icon';
import { HeaderComponent } from './header/header';
import { FooterComponent } from './footer/footer';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterModule, CartIcon, HeaderComponent, FooterComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('englishschool-ui-app');
  protected readonly isFullScreenRoute = signal(false);

  constructor(private router: Router) {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: NavigationEnd) => {
      this.isFullScreenRoute.set(event.urlAfterRedirects === '/signin' || event.urlAfterRedirects === '/register');
    });
  }
}
