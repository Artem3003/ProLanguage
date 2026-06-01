import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Order, OrderDetail } from '../models/cart.model';
import { Course } from '../models/course.model';
import { CartService } from '../services/cart.service';
import { CourseService } from '../services/course.service';

@Component({
  selector: 'app-orders',
  imports: [CommonModule],
  templateUrl: './orders.html',
  styleUrl: './orders.scss'
})
export class Orders implements OnInit {
  orders: Order[] = [];
  selectedOrder: Order | null = null;
  orderDetails: OrderDetail[] = [];
  courses: Map<string, Course> = new Map();
  loading: boolean = false;
  loadingDetails: boolean = false;
  error: string = '';

  constructor(
    private cartService: CartService,
    private courseService: CourseService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.loading = true;
    this.error = '';
    this.cartService.getOrders().subscribe({
      next: (orders) => {
        this.orders = orders;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading orders', err);
        this.error = 'Failed to load orders';
        this.loading = false;
      }
    });
  }

  viewOrderDetails(order: Order): void {
    this.selectedOrder = order;
    this.loadingDetails = true;

    this.cartService.getOrderDetails(order.id).subscribe({
      next: (details) => {
        this.orderDetails = details;
        this.loadCourseDetails();
        this.loadingDetails = false;
      },
      error: (err) => {
        console.error('Error loading order details', err);
        this.error = 'Failed to load order details';
        this.loadingDetails = false;
      }
    });
  }

  loadCourseDetails(): void {
    this.orderDetails.forEach(item => {
      if (!this.courses.has(item.courseId)) {
        this.courseService.getCourseById(item.courseId).subscribe({
          next: (course) => this.courses.set(item.courseId, course),
          error: (err) => console.error(`Error loading course ${item.courseId}`, err)
        });
      }
    });
  }

  getCourseName(courseId: string): string {
    return this.courses.get(courseId)?.title || 'Loading...';
  }

  getItemTotal(item: OrderDetail): number {
    const discountedPrice = item.price * (1 - item.discount / 100);
    return discountedPrice * item.quantity;
  }

  getOrderTotal(): number {
    return this.orderDetails.reduce((total, item) => total + this.getItemTotal(item), 0);
  }

  closeDetails(): void {
    this.selectedOrder = null;
    this.orderDetails = [];
  }

  formatDate(dateString: string): string {
    if (!dateString) {
      return '';
    }

    // Order dates are stored in UTC. If the value has no time-zone designator,
    // treat it as UTC so the browser converts it to the client's local time zone.
    const hasTimeZone = /[zZ]|[+-]\d{2}:?\d{2}$/.test(dateString);
    const utcDate = hasTimeZone ? dateString : `${dateString}Z`;

    // `undefined` locale + no timeZone option => the client's own locale and zone.
    return new Date(utcDate).toLocaleString(undefined, {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  goToCart(): void {
    this.router.navigate(['/cart']);
  }
}
