import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, tap, retry, timer } from 'rxjs';
import {
  CartItem,
  Order,
  OrderDetail,
  PaymentMethodsResponse,
  PaymentRequest,
  PaymentResponse
} from '../models/cart.model';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private baseUrl = 'http://localhost:5201';

  // BehaviorSubject to track cart items count
  private cartItemsCountSubject = new BehaviorSubject<number>(0);
  cartItemsCount$ = this.cartItemsCountSubject.asObservable();

  constructor(private http: HttpClient) {
    this.refreshCartCount();
  }

  // Add course to cart
  addToCart(courseId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/courses/${courseId}/buy`, {}).pipe(
      tap(() => this.refreshCartCount())
    );
  }

  // Remove course from cart
  removeFromCart(courseId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/orders/cart/${courseId}`).pipe(
      tap(() => this.refreshCartCount())
    );
  }

  // Get cart items
  getCart(): Observable<CartItem[]> {
    return this.http.get<CartItem[]>(`${this.baseUrl}/orders/cart`).pipe(
      tap(items => this.cartItemsCountSubject.next(items.length))
    );
  }

  // Get all orders (paid and cancelled)
  getOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.baseUrl}/orders`);
  }

  // Get order by ID
  getOrderById(id: string): Observable<Order> {
    return this.http.get<Order>(`${this.baseUrl}/orders/${id}`);
  }

  // Get order details
  getOrderDetails(id: string): Observable<OrderDetail[]> {
    return this.http.get<OrderDetail[]>(`${this.baseUrl}/orders/${id}/details`);
  }

  // Get payment methods
  getPaymentMethods(): Observable<PaymentMethodsResponse> {
    return this.http.get<PaymentMethodsResponse>(`${this.baseUrl}/orders/payment-methods`);
  }

  // Process payment with retry logic for fault tolerance
  processPayment(paymentRequest: PaymentRequest): Observable<PaymentResponse | Blob> {
    // For Bank payments, expect PDF blob response
    if (paymentRequest.method === 'Bank') {
      return this.http.post(`${this.baseUrl}/orders/payment`, paymentRequest, {
        responseType: 'blob'
      }).pipe(
        retry({
          count: 3,
          delay: (error, retryCount) => {
            console.log(`Payment retry attempt ${retryCount}`);
            return timer(1000 * retryCount);
          }
        }),
        tap(() => this.refreshCartCount())
      );
    }

    // For other payments, expect JSON response
    return this.http.post<PaymentResponse>(`${this.baseUrl}/orders/payment`, paymentRequest).pipe(
      retry({
        count: 3,
        delay: (error, retryCount) => {
          console.log(`Payment retry attempt ${retryCount}`);
          return timer(1000 * retryCount);
        }
      }),
      tap(() => this.refreshCartCount())
    );
  }

  // Refresh cart count
  refreshCartCount(): void {
    this.http.get<CartItem[]>(`${this.baseUrl}/orders/cart`).subscribe({
      next: (items) => this.cartItemsCountSubject.next(items?.length || 0),
      error: () => this.cartItemsCountSubject.next(0)
    });
  }

  // Calculate total price
  calculateTotal(items: CartItem[]): number {
    return items.reduce((total, item) => {
      const discountedPrice = item.price * (1 - item.discount / 100);
      return total + (discountedPrice * item.quantity);
    }, 0);
  }
}
