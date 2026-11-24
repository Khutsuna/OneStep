import { Routes } from '@angular/router';
import { adminGuard, authGuard, customerGuard, staffGuard } from './core/auth/auth.guards';
import { Shell } from './core/layout/shell/shell';

/**
 * Every page is lazy-loaded (loadComponent): its code is downloaded only when you open it,
 * so the first page loads faster.
 */
// Titles are translation keys; TranslatedTitleStrategy shows them in the current language.
export const routes: Routes = [
  // The admin panel has its own layout (no shop header), so it sits outside the Shell.
  // It must come first: the Shell's '**' (page not found) would otherwise catch /admin.
  {
    path: 'admin',
    canActivate: [staffGuard],
    loadComponent: () => import('./features/admin/layout/admin-layout').then((m) => m.AdminLayout),
    children: [
      {
        path: '',
        title: 'title.dashboard',
        loadComponent: () => import('./features/admin/dashboard/dashboard-page').then((m) => m.DashboardPage),
      },
      {
        path: 'orders',
        title: 'title.adminOrders',
        data: { admin: true },
        loadComponent: () =>
          import('./features/account/orders/order-list-page').then((m) => m.OrderListPage),
      },
      {
        path: 'orders/:id',
        title: 'title.order',
        data: { admin: true },
        loadComponent: () =>
          import('./features/account/orders/order-detail-page').then((m) => m.OrderDetailPage),
      },
      {
        path: 'products',
        title: 'title.adminProducts',
        loadComponent: () =>
          import('./features/admin/products/admin-products-page').then((m) => m.AdminProductsPage),
      },
      {
        path: 'products/new',
        title: 'title.newProduct',
        loadComponent: () => import('./features/admin/products/product-edit-page').then((m) => m.ProductEditPage),
      },
      {
        path: 'products/:id',
        title: 'title.editProduct',
        loadComponent: () => import('./features/admin/products/product-edit-page').then((m) => m.ProductEditPage),
      },
      {
        path: 'campaigns',
        title: 'title.campaigns',
        loadComponent: () =>
          import('./features/admin/campaigns/admin-campaigns-page').then((m) => m.AdminCampaignsPage),
      },
      {
        path: 'users',
        title: 'title.users',
        canActivate: [adminGuard],
        loadComponent: () => import('./features/admin/users/admin-users-page').then((m) => m.AdminUsersPage),
      },
    ],
  },
  {
    path: '',
    component: Shell,
    children: [
      {
        path: '',
        title: 'title.default',
        loadComponent: () => import('./features/home/home-page').then((m) => m.HomePage),
      },
      {
        path: 'products',
        title: 'title.products',
        loadComponent: () =>
          import('./features/catalog/product-list/product-list-page').then((m) => m.ProductListPage),
      },
      {
        path: 'products/:id',
        loadComponent: () =>
          import('./features/catalog/product-detail/product-detail-page').then((m) => m.ProductDetailPage),
      },
      {
        path: 'cart',
        title: 'title.cart',
        canActivate: [customerGuard],
        loadComponent: () => import('./features/cart/cart-page').then((m) => m.CartPage),
      },
      {
        path: 'checkout',
        title: 'title.checkout',
        canActivate: [customerGuard],
        loadComponent: () => import('./features/checkout/checkout-page').then((m) => m.CheckoutPage),
      },
      {
        path: 'account',
        title: 'title.account',
        canActivate: [authGuard],
        loadComponent: () => import('./features/account/profile/profile-page').then((m) => m.ProfilePage),
      },
      {
        path: 'account/wishlist',
        title: 'title.wishlist',
        canActivate: [customerGuard],
        loadComponent: () => import('./features/account/wishlist/wishlist-page').then((m) => m.WishlistPage),
      },
      {
        path: 'account/orders',
        title: 'title.myOrders',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/account/orders/order-list-page').then((m) => m.OrderListPage),
      },
      {
        path: 'account/orders/:id',
        title: 'title.order',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/account/orders/order-detail-page').then((m) => m.OrderDetailPage),
      },
      {
        path: 'login',
        title: 'title.login',
        loadComponent: () => import('./features/auth/login/login-page').then((m) => m.LoginPage),
      },
      {
        path: 'register',
        title: 'title.register',
        loadComponent: () => import('./features/auth/register/register-page').then((m) => m.RegisterPage),
      },
      {
        path: 'forgot-password',
        title: 'title.forgotPassword',
        loadComponent: () =>
          import('./features/auth/forgot-password/forgot-password-page').then((m) => m.ForgotPasswordPage),
      },
      {
        // Opened from the link in the email: /reset-password?token=...
        path: 'reset-password',
        title: 'title.resetPassword',
        loadComponent: () =>
          import('./features/auth/reset-password/reset-password-page').then((m) => m.ResetPasswordPage),
      },
      {
        path: '**',
        title: 'title.notFound',
        loadComponent: () => import('./features/not-found/not-found-page').then((m) => m.NotFoundPage),
      },
    ],
  },
];
