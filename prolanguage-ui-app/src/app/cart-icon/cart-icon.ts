import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Subscription } from 'rxjs';
import { CartService } from '../services/cart.service';

@Component({
  selector: 'app-cart-icon',
  imports: [CommonModule, RouterModule],
  templateUrl: './cart-icon.html',
  styleUrl: './cart-icon.scss'
})
export class CartIcon implements OnInit, OnDestroy {
  itemCount: number = 0;
  private subscription: Subscription | null = null;

  constructor(private cartService: CartService) {}

  ngOnInit(): void {
    this.subscription = this.cartService.cartItemsCount$.subscribe(count => {
      this.itemCount = count;
    });
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }
}
