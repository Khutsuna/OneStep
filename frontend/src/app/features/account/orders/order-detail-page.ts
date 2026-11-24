import { CurrencyPipe, DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { toObservable } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar } from '@angular/material/snack-bar';
import { RouterLink } from '@angular/router';
import { Observable, switchMap } from 'rxjs';
import { AuthStore } from '../../../core/auth/auth.store';
import { OrderDetails, OrderStatus } from '../../../core/models/shop.models';
import { OrdersApi } from '../../../core/shop/orders.api';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { ORDER_STATUS_LABELS, ORDER_STEP_LABELS, OrderStatusChip } from '../../../shared/ui/order-status/order-status-chip';
import { ProductImage } from '../../../shared/ui/product-image/product-image';
import { TotalsTable } from '../../../shared/ui/totals-table/totals-table';
import { OrderTracker } from './order-tracker';

/**
 * One order: tracking steps, items, address and totals.
 * Customers can pay (test mode) or cancel; staff can move the order to its next status.
 * Which buttons appear comes from the API (canPay, canCancel, nextStatuses), so the rules live in one place.
 */
@Component({
  selector: 'app-order-detail-page',
  imports: [
    CurrencyPipe, DatePipe, FormsModule, RouterLink, MatButtonModule, MatFormFieldModule, MatIconModule, MatInputModule,
    EmptyState, OrderStatusChip, OrderTracker, ProductImage, TotalsTable,
  ],
  templateUrl: './order-detail-page.html',
  styleUrl: './order-detail-page.scss',
})
export class OrderDetailPage {
  /** From the URL /account/orders/:id */
  readonly id = input.required<string>();
  /** ?placed=true right after checkout, to say thank you. */
  readonly placed = input<string | undefined>();
  /** true inside the admin panel (from the route's data). */
  readonly admin = input(false);

  protected readonly auth = inject(AuthStore);
  private readonly api = inject(OrdersApi);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly order = signal<OrderDetails | null>(null);
  protected readonly notFound = signal(false);
  protected readonly busy = signal(false);
  protected readonly confirmingCancel = signal(false);
  protected readonly statusNote = signal('');
  protected readonly statusLabels = ORDER_STATUS_LABELS;
  protected readonly stepLabels = ORDER_STEP_LABELS;

  /** Staff buttons: every allowed next status except Cancelled (that has its own button). */
  protected readonly moveTo = computed(() => this.order()?.nextStatuses.filter((s) => s !== 'Cancelled') ?? []);

  constructor() {
    toObservable(this.id)
      .pipe(switchMap((id) => this.api.getById(Number(id))))
      .subscribe({
        next: (order) => this.order.set(order),
        error: () => this.notFound.set(true),
      });
  }

  protected pay(): void {
    this.act(this.api.pay(this.order()!.id), 'Payment received. Thank you!');
  }

  protected cancel(): void {
    this.act(this.api.cancel(this.order()!.id), 'Your order was cancelled.');
  }

  protected changeStatus(status: OrderStatus): void {
    const note = this.statusNote().trim() || null;
    this.act(this.api.changeStatus(this.order()!.id, status, note), `Order marked as ${ORDER_STEP_LABELS[status].toLowerCase()}.`);
  }

  /** Runs an action; the API answers with the updated order, which replaces ours. */
  private act(request: Observable<OrderDetails>, successMessage: string): void {
    this.busy.set(true);
    request.subscribe({
      next: (order) => {
        this.order.set(order);
        this.busy.set(false);
        this.confirmingCancel.set(false);
        this.statusNote.set('');
        this.snackBar.open(successMessage, 'OK', { duration: 4000 });
      },
      error: (error: HttpErrorResponse) => {
        this.busy.set(false);
        if (error.status === 409 || error.status === 400) {
          this.snackBar.open(error.error?.detail ?? "That isn't possible right now.", 'OK', { duration: 5000 });
        }
      },
    });
  }
}
