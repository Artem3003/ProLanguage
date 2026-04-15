import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CartItem, PaymentMethod, VisaPaymentModel } from '../models/cart.model';
import { Course } from '../models/course.model';
import { CartService } from '../services/cart.service';
import { CourseService } from '../services/course.service';

@Component({
  selector: 'app-cart',
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './cart.html',
  styleUrl: './cart.scss'
})
export class Cart implements OnInit {
  cartItems: CartItem[] = [];
  courses: Map<string, Course> = new Map();
  paymentMethods: PaymentMethod[] = [];
  selectedPaymentMethod: string = '';
  loading: boolean = false;
  error: string = '';
  success: string = '';
  showPaymentModal: boolean = false;
  processingPayment: boolean = false;

  // Visa payment form
  visaForm: VisaPaymentModel = {
    holder: '',
    cardNumber: '',
    monthExpire: 1,
    yearExpire: new Date().getFullYear(),
    cvv2: 0
  };

  constructor(
    private cartService: CartService,
    private courseService: CourseService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadCart();
    this.loadPaymentMethods();
  }

  loadCart(): void {
    this.loading = true;
    this.error = '';
    this.cartService.getCart().subscribe({
      next: (items) => {
        this.cartItems = items;
        this.loadCourseDetails();
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading cart', err);
        this.error = 'Failed to load cart';
        this.loading = false;
      }
    });
  }

  loadCourseDetails(): void {
    this.cartItems.forEach(item => {
      if (!this.courses.has(item.courseId)) {
        this.courseService.getCourseById(item.courseId).subscribe({
          next: (course) => this.courses.set(item.courseId, course),
          error: (err) => console.error(`Error loading course ${item.courseId}`, err)
        });
      }
    });
  }

  loadPaymentMethods(): void {
    this.cartService.getPaymentMethods().subscribe({
      next: (response) => {
        this.paymentMethods = response.paymentMethods;
        if (this.paymentMethods.length > 0) {
          this.selectedPaymentMethod = this.paymentMethods[0].title;
        }
      },
      error: (err) => console.error('Error loading payment methods', err)
    });
  }

  getCourseName(courseId: string): string {
    return this.courses.get(courseId)?.title || 'Loading...';
  }

  getCourseDescription(courseId: string): string {
    return this.courses.get(courseId)?.description || '';
  }

  getItemTotal(item: CartItem): number {
    const discountedPrice = item.price * (1 - item.discount / 100);
    return discountedPrice * item.quantity;
  }

  getCartTotal(): number {
    return this.cartService.calculateTotal(this.cartItems);
  }

  removeItem(courseId: string): void {
    this.cartService.removeFromCart(courseId).subscribe({
      next: () => {
        this.cartItems = this.cartItems.filter(item => item.courseId !== courseId);
        this.success = 'Item removed from cart';
        setTimeout(() => this.success = '', 3000);
      },
      error: (err) => {
        console.error('Error removing item', err);
        this.error = 'Failed to remove item';
      }
    });
  }

  openPaymentModal(): void {
    if (this.cartItems.length === 0) {
      this.error = 'Your cart is empty';
      return;
    }
    this.showPaymentModal = true;
    this.error = '';
  }

  closePaymentModal(): void {
    this.showPaymentModal = false;
    this.resetVisaForm();
  }

  resetVisaForm(): void {
    this.visaForm = {
      holder: '',
      cardNumber: '',
      monthExpire: 1,
      yearExpire: new Date().getFullYear(),
      cvv2: 0
    };
  }

  processPayment(): void {
    if (!this.selectedPaymentMethod) {
      this.error = 'Please select a payment method';
      return;
    }

    this.processingPayment = true;
    this.error = '';

    let paymentRequest: any = { method: this.selectedPaymentMethod };

    if (this.selectedPaymentMethod === 'Visa') {
      if (!this.validateVisaForm()) {
        this.processingPayment = false;
        return;
      }
      paymentRequest.model = this.visaForm;
    }

    this.cartService.processPayment(paymentRequest).subscribe({
      next: (response) => {
        this.processingPayment = false;
        
        if (this.selectedPaymentMethod === 'Bank' && response instanceof Blob) {
          // Download PDF invoice
          this.downloadInvoice(response);
          this.success = 'Invoice generated successfully!';
        } else {
          this.success = 'Payment processed successfully!';
        }
        
        this.closePaymentModal();
        this.cartItems = [];
        setTimeout(() => {
          this.success = '';
          this.router.navigate(['/orders']);
        }, 2000);
      },
      error: (err) => {
        this.processingPayment = false;
        console.error('Payment error', err);
        this.error = 'Payment failed. Please try again.';
      }
    });
  }

  downloadInvoice(blob: Blob): void {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `invoice-${new Date().getTime()}.pdf`;
    link.click();
    window.URL.revokeObjectURL(url);
  }

  validateVisaForm(): boolean {
    if (!this.visaForm.holder.trim()) {
      this.error = 'Card holder name is required';
      return false;
    }
    if (!this.visaForm.cardNumber || this.visaForm.cardNumber.length < 13) {
      this.error = 'Invalid card number';
      return false;
    }
    if (this.visaForm.monthExpire < 1 || this.visaForm.monthExpire > 12) {
      this.error = 'Invalid expiry month';
      return false;
    }
    if (this.visaForm.yearExpire < new Date().getFullYear()) {
      this.error = 'Card has expired';
      return false;
    }
    if (!this.visaForm.cvv2 || this.visaForm.cvv2.toString().length < 3) {
      this.error = 'Invalid CVV';
      return false;
    }
    return true;
  }

  goToOrders(): void {
    this.router.navigate(['/orders']);
  }
}
