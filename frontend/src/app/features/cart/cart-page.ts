import { CurrencyPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { RouterLink } from '@angular/router';
import { Observable, catchError, map, of } from 'rxjs';
import { Cart, CartItem } from '../../core/models/shop.models';
import { CartStore } from '../../core/shop/cart.store';
import { DeliveryMethodsApi } from '../../core/shop/delivery-methods.api';
import { EmptyState } from '../../shared/ui/empty-state/empty-state';
import { ProductImage } from '../../shared/ui/product-image/product-image';
import { QuantityStepper } from '../../shared/ui/quantity-stepper/quantity-stepper';
import { I18N } from '../../core/i18n/translate.pipes';
import { LanguageStore } from '../../core/i18n/language.store';

@Component({
  selector: 'app-cart-page',
  imports: [I18N, CurrencyPipe, RouterLink, MatButtonModule, MatIconModule, EmptyState, ProductImage, QuantityStepper],
  templateUrl: './cart-page.html',
  styleUrl: './cart-page.scss',
})
export class CartPage {
  protected readonly cart = inject(CartStore);
  private readonly snackBar = inject(MatSnackBar);
  private readonly i18n = inject(LanguageStore);

  /** The lowest "free delivery from" amount of the delivery methods (₾200 for the standard courier). */
  private readonly freeFrom = toSignal(
    inject(DeliveryMethodsApi)
      .getAll()
      .pipe(
        map((methods) => methods.map((m) => m.freeOverAmount).filter((amount): amount is number => amount !== null)),
        map((amounts) => (amounts.length ? Math.min(...amounts) : null)),
        catchError(() => of(null)),
      ),
    { initialValue: null },
  );

  /**
   * How far the cart is from free delivery. Uses the total after discounts,
   * the same amount the API's CheckoutCalculator compares with the threshold.
   */
  protected readonly freeDelivery = computed(() => {
    const from = this.freeFrom();
    const cart = this.cart.cart();
    if (!from || !cart) {
      return null;
    }
    return {
      left: Math.max(from - cart.total, 0),
      percent: Math.min(100, Math.round((cart.total / from) * 100)),
    };
  });

  /** The item that is being saved right now, so its buttons can be disabled. */
  protected readonly busyItemId = signal<number | null>(null);

  protected changeQuantity(item: CartItem, quantity: number): void {
    this.run(item, this.cart.updateQuantity(item.id, quantity));
  }

  protected remove(item: CartItem): void {
    this.run(item, this.cart.remove(item.id), this.i18n.t('cart.removed', { name: item.name }));
  }

  private run(item: CartItem, request: Observable<Cart>, successMessage?: string): void {
    this.busyItemId.set(item.id);
    request.subscribe({
      next: () => {
        this.busyItemId.set(null);
        if (successMessage) this.snackBar.open(successMessage, this.i18n.t('common.ok'), { duration: 3000 });
      },
      error: (error: HttpErrorResponse) => {
        this.busyItemId.set(null);
        // 409 = not enough stock. The API's message says how many are left.
        if (error.status === 409 || error.status === 400) {
          this.snackBar.open(error.error?.detail ?? this.i18n.t('cart.unavailable'), this.i18n.t('common.ok'), { duration: 5000 });
        }
      },
    });
  }
}
