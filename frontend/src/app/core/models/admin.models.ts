// TypeScript versions of the API's admin DTOs (analytics, notifications, campaigns, users, product editing).
import { ProductStatus } from './catalog.models';
import { OrderStatus } from './shop.models';
import { UserRole } from './auth.models';

export interface DashboardSummary {
  revenue: number;
  paidOrderCount: number;
  orderCount: number;
  averageOrderValue: number;
  newCustomers: number;
  pendingOrders: number;
  ordersToShip: number;
  lowStockProducts: number;
}

export interface DailySales {
  day: string; // "2026-11-23"
  orderCount: number;
  revenue: number;
}

export interface TopProduct {
  productId: number;
  productName: string;
  unitsSold: number;
  revenue: number;
}

export interface Dashboard {
  from: string;
  to: string;
  summary: DashboardSummary;
  salesByDay: DailySales[];
  topProducts: TopProduct[];
}

export type NotificationType = 'LowStock' | 'OutOfStock' | 'NewOrder';

export interface StaffNotification {
  id: number;
  type: NotificationType;
  message: string;
  productId: number | null;
  orderId: number | null;
  isRead: boolean;
  createdAt: string;
}

export interface NotificationList {
  items: StaffNotification[];
  unreadCount: number;
}

export type CampaignState = 'Scheduled' | 'Active' | 'Ended' | 'Disabled';

export interface Campaign {
  id: number;
  name: string;
  description: string | null;
  discountType: 'Percentage' | 'FixedAmount';
  discountValue: number;
  appliesToAllProducts: boolean;
  startsAt: string;
  endsAt: string;
  isEnabled: boolean;
  state: CampaignState;
  productCount: number;
  categoryNames: string[];
}

export interface UserListItem {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  isActive: boolean;
  createdAt: string;
  lastLoginAt: string | null;
  orderCount: number;
}

export interface LowStockProduct {
  id: number;
  name: string;
  sku: string;
  stockQuantity: number;
  lowStockThreshold: number;
}

export interface ProductRequest {
  name: string;
  sku: string;
  description: string;
  price: number;
  categoryId: number;
  brandId: number | null;
  stockQuantity: number;
  lowStockThreshold: number;
  status: ProductStatus;
  attributes: { attributeId: number; value: string }[];
}

export interface NamedOption {
  id: number;
  name: string;
}

export type { OrderStatus };
