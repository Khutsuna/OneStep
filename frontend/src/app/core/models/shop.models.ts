// TypeScript versions of the API's cart, address, delivery and order DTOs.

export interface CartItem {
  id: number;
  productId: number;
  name: string;
  imageUrl: string | null;
  unitPrice: number;
  unitFinalPrice: number;
  quantity: number;
  stockQuantity: number;
  lineTotal: number;
}

/** The cart with current prices. Shipping and VAT are added at checkout. */
export interface Cart {
  items: CartItem[];
  itemCount: number;
  subtotal: number;
  discountTotal: number;
  total: number;
}

export interface Address {
  id: number;
  label: string;
  recipientName: string;
  phone: string;
  line1: string;
  line2: string | null;
  city: string;
  region: string | null;
  postalCode: string | null;
  countryCode: string;
  isDefault: boolean;
}

export type AddressRequest = Omit<Address, 'id'>;

export interface DeliveryMethod {
  id: number;
  name: string;
  description: string | null;
  price: number;
  freeOverAmount: number | null;
  minDays: number;
  maxDays: number;
}

export type OrderStatus = 'Pending' | 'Paid' | 'Shipped' | 'Delivered' | 'Cancelled';
export type PaymentMethod = 'Card' | 'CashOnDelivery';

/** Matches OrderTotals in the API (CheckoutCalculator). */
export interface OrderTotals {
  subtotal: number;
  discountTotal: number;
  shippingCost: number;
  taxAmount: number;
  totalAmount: number;
}

export interface PlaceOrderRequest {
  addressId: number;
  deliveryMethodId: number;
  paymentMethod: PaymentMethod;
  customerNote: string | null;
}

export interface OrderSummary {
  id: number;
  status: OrderStatus;
  totalAmount: number;
  itemCount: number;
  createdAt: string;
  customerName: string;
  customerEmail: string;
}

export interface OrderItem {
  productId: number;
  productName: string;
  sku: string;
  imageUrl: string | null;
  unitPrice: number;
  unitDiscount: number;
  quantity: number;
  lineTotal: number;
}

export interface ShippingAddress {
  recipientName: string;
  phone: string;
  line1: string;
  line2: string | null;
  city: string;
  region: string | null;
  postalCode: string | null;
  countryCode: string;
}

/** One step of the order-tracking timeline. */
export interface OrderStatusStep {
  fromStatus: OrderStatus | null;
  toStatus: OrderStatus;
  changedAt: string;
  note: string | null;
  changedBy: string | null;
}

export interface OrderDetails {
  id: number;
  status: OrderStatus;
  createdAt: string;
  paidAt: string | null;
  paymentMethod: PaymentMethod;
  deliveryMethodName: string;
  customerNote: string | null;
  customerId: number;
  customerName: string;
  customerEmail: string;
  shippingAddress: ShippingAddress;
  items: OrderItem[];
  totals: OrderTotals;
  history: OrderStatusStep[];
  nextStatuses: OrderStatus[]; // for staff: which status buttons to show
  canCancel: boolean;
  canPay: boolean;
}

export interface OrderListQuery {
  status: OrderStatus | null;
  search: string | null;
  page: number;
  pageSize: number;
}
