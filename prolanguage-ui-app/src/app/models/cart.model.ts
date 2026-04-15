export interface CartItem {
  courseId: string;
  price: number;
  quantity: number;
  discount: number;
}

export interface Order {
  id: string;
  customerId: string;
  date: string;
  status?: OrderStatus;
}

export interface OrderDetail {
  courseId: string;
  price: number;
  quantity: number;
  discount: number;
}

export enum OrderStatus {
  Open = 'Open',
  Checkout = 'Checkout',
  Paid = 'Paid',
  Cancelled = 'Cancelled'
}

export interface PaymentMethod {
  imageUrl: string;
  title: string;
  description: string;
}

export interface PaymentMethodsResponse {
  paymentMethods: PaymentMethod[];
}

export interface PaymentRequest {
  method: string;
  model?: VisaPaymentModel;
}

export interface VisaPaymentModel {
  holder: string;
  cardNumber: string;
  monthExpire: number;
  yearExpire: number;
  cvv2: number;
}

export interface PaymentResponse {
  userId: string;
  orderId: string;
  paymentDate: string;
  sum: number;
}
