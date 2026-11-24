import { CurrencyPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { Router, RouterLink } from '@angular/router';
import { catchError, filter, of, switchMap, tap } from 'rxjs';
import { Address, DeliveryMethod, OrderTotals, PaymentMethod } from '../../core/models/shop.models';
import { AddressesApi } from '../../core/shop/addresses.api';
import { CartStore } from '../../core/shop/cart.store';
import { DeliveryMethodsApi } from '../../core/shop/delivery-methods.api';
import { OrdersApi } from '../../core/shop/orders.api';
import { EmptyState } from '../../shared/ui/empty-state/empty-state';
import { TotalsTable } from '../../shared/ui/totals-table/totals-table';
import { AddressForm } from '../account/address-form/address-form';
import { I18N } from '../../core/i18n/translate.pipes';
import { LanguageStore } from '../../core/i18n/language.store';

/**
 * Checkout on one page: address, delivery, payment, then "Place order".
 * The totals on the right come from the API (/api/orders/quote), never from the browser,
 * so they always match what the order will really cost.
 */
@Component({
  selector: 'app-checkout-page',
  imports: [I18N, CurrencyPipe, FormsModule, RouterLink, MatButtonModule, MatFormFieldModule, MatIconModule, MatInputModule,
    AddressForm, EmptyState, TotalsTable,
  ],
  templateUrl: './checkout-page.html',
  styleUrl: './checkout-page.scss',
})
export class CheckoutPage {
  protected readonly cart = inject(CartStore);
  private readonly addressesApi = inject(AddressesApi);
  private readonly ordersApi = inject(OrdersApi);
  private readonly router = inject(Router);
  private readonly i18n = inject(LanguageStore);

  // ---- 1. Address ----
  protected readonly addresses = signal<Address[] | null>(null);
  protected readonly addressId = signal<number | null>(null);
  protected readonly addingAddress = signal(false);

  // ---- 2. Delivery ----
  protected readonly deliveryMethods = toSignal(
    inject(DeliveryMethodsApi).getAll().pipe(
      tap((methods) => this.deliveryMethodId.set(methods[0]?.id ?? null)),
      catchError(() => of([] as DeliveryMethod[])),
    ),
    { initialValue: [] as DeliveryMethod[] },
  );
  protected readonly deliveryMethodId = signal<number | null>(null);

  // ---- 3. Payment ----
  protected readonly paymentMethod = signal<PaymentMethod>('Card');
  protected readonly note = signal('');

  /** New totals every time the delivery method changes. switchMap drops an older, slower answer. */
  protected readonly totals = toSignal(
    toObservable(this.deliveryMethodId).pipe(
      filter((id): id is number => id !== null),
      switchMap((id) => this.ordersApi.quote(id).pipe(catchError(() => of(null)))),
    ),
    { initialValue: null as OrderTotals | null },
  );

  protected readonly placing = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly canPlace = computed(
    () => !!this.addressId() && !!this.deliveryMethodId() && !!this.totals() && !this.cart.isEmpty() && !this.placing(),
  );

  constructor() {
    this.loadAddresses();
  }

  /** Loads the address book and selects the default address (or the one just added). */
  protected loadAddresses(selectId?: number): void {
    this.addressesApi.getAll().subscribe((addresses) => {
      this.addresses.set(addresses);
      const preferred = addresses.find((a) => a.id === selectId) ?? addresses.find((a) => a.isDefault) ?? addresses[0];
      this.addressId.set(preferred?.id ?? null);
      // No addresses yet: open the form straight away.
      this.addingAddress.set(addresses.length === 0);
    });
  }

  protected onAddressSaved(address: Address): void {
    this.addingAddress.set(false);
    this.loadAddresses(address.id);
  }

  protected deliveryNote(method: DeliveryMethod): string {
    if (method.maxDays === 0) return this.i18n.t('checkout.pickupToday');
    const days = method.minDays === method.maxDays ? `${method.minDays}` : `${method.minDays}–${method.maxDays}`;
    return this.i18n.t('checkout.days', { days, count: method.maxDays });
  }

  /** What this method costs for the current cart: free above its threshold (the API uses the same rule). */
  protected deliveryPrice(method: DeliveryMethod): number {
    const goods = this.cart.cart()?.total ?? 0;
    return method.freeOverAmount !== null && goods >= method.freeOverAmount ? 0 : method.price;
  }

  protected placeOrder(): void {
    if (!this.canPlace()) return;

    this.placing.set(true);
    this.errorMessage.set(null);

    this.ordersApi
      .place({
        addressId: this.addressId()!,
        deliveryMethodId: this.deliveryMethodId()!,
        paymentMethod: this.paymentMethod(),
        customerNote: this.note().trim() || null,
      })
      .subscribe({
        next: (order) => {
          // The API emptied the cart in the same transaction; forget our copy too.
          this.cart.reset();
          void this.router.navigate(['/account/orders', order.id], { queryParams: { placed: true } });
        },
        error: (error: HttpErrorResponse) => {
          this.placing.set(false);
          this.errorMessage.set(error.error?.detail ?? this.i18n.t('checkout.failed'));
          // 409 = stock changed: reload the cart so the customer sees what's actually available.
          if (error.status === 409) this.cart.load();
        },
      });
  }
}
