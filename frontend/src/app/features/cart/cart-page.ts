import { CurrencyPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { Cart, CartItem } from '../../core/models/shop.models';
import { CartStore } from '../../core/shop/cart.store';
import { EmptyState } from '../../shared/ui/empty-state/empty-state';
import { ProductImage } from '../../shared/ui/product-image/product-image';
import { QuantityStepper } from '../../shared/ui/quantity-stepper/quantity-stepper';

@Component({
  selector: 'app-cart-page',
  imports: [CurrencyPipe, RouterLink, MatButtonModule, MatIconModule, EmptyState, ProductImage, QuantityStepper],
  templateUrl: './cart-page.html',
  styleUrl: './cart-page.scss',
})
export class CartPage {
  protected readonly cart = inject(CartStore);
  private readonly snackBar = inject(MatSnackBar);

  /** The item that is being saved right now, so its buttons can be disabled. */
  protected readonly busyItemId = signal<number | null>(null);

  protected changeQuantity(item: CartItem, quantity: number): void {
    this.run(item, this.cart.updateQuantity(item.id, quantity));
  }

  protected remove(item: CartItem): void {
    this.run(item, this.cart.remove(item.id), `${item.name} was removed from your cart.`);
  }

  private run(item: CartItem, request: Observable<Cart>, successMessage?: string): void {
    this.busyItemId.set(item.id);
    request.subscribe({
      next: () => {
        this.busyItemId.set(null);
        if (successMessage) this.snackBar.open(successMessage, 'OK', { duration: 3000 });
      },
      error: (error: HttpErrorResponse) => {
        this.busyItemId.set(null);
        // 409 = not enough stock. The API's message says how many are left.
        if (error.status === 409 || error.status === 400) {
          this.snackBar.open(error.error?.detail ?? "That quantity isn't available.", 'OK', { duration: 5000 });
        }
      },
    });
  }
}
