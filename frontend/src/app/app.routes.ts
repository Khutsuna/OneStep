import { Routes } from '@angular/router';
import { authGuard, customerGuard } from './core/auth/auth.guards';
import { Shell } from './core/layout/shell/shell';

/**
 * Every page is lazy-loaded (loadComponent): its code is downloaded only when you open it,
 * so the first page loads faster.
 */
export const routes: Routes = [
  {
    path: '',
    component: Shell,
    children: [
      {
        path: '',
        title: 'OneStep — electronics in Georgia',
        loadComponent: () => import('./features/home/home-page').then((m) => m.HomePage),
      },
      {
        path: 'products',
        title: 'Products — OneStep',
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
        title: 'Your cart — OneStep',
        canActivate: [customerGuard],
        loadComponent: () => import('./features/cart/cart-page').then((m) => m.CartPage),
      },
      {
        path: 'checkout',
        title: 'Checkout — OneStep',
        canActivate: [customerGuard],
        loadComponent: () => import('./features/checkout/checkout-page').then((m) => m.CheckoutPage),
      },
      {
        path: 'account/orders',
        title: 'My orders — OneStep',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/account/orders/order-list-page').then((m) => m.OrderListPage),
      },
      {
        path: 'account/orders/:id',
        title: 'Order — OneStep',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/account/orders/order-detail-page').then((m) => m.OrderDetailPage),
      },
      {
        path: 'login',
        title: 'Log in — OneStep',
        loadComponent: () => import('./features/auth/login/login-page').then((m) => m.LoginPage),
      },
      {
        path: 'register',
        title: 'Create an account — OneStep',
        loadComponent: () => import('./features/auth/register/register-page').then((m) => m.RegisterPage),
      },
      {
        path: '**',
        title: 'Page not found — OneStep',
        loadComponent: () => import('./features/not-found/not-found-page').then((m) => m.NotFoundPage),
      },
    ],
  },
];
