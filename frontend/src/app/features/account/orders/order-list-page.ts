import { CurrencyPipe } from '@angular/common';
import { Component, computed, inject, input, linkedSignal, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink } from '@angular/router';
import { catchError, debounceTime, distinctUntilChanged, map, of, startWith, switchMap } from 'rxjs';
import { AuthStore } from '../../../core/auth/auth.store';
import { PagedResult } from '../../../core/models/common.models';
import { OrderStatus, OrderSummary } from '../../../core/models/shop.models';
import { OrdersApi } from '../../../core/shop/orders.api';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { OrderStatusChip, statusKey } from '../../../shared/ui/order-status/order-status-chip';
import { I18N } from '../../../core/i18n/translate.pipes';

interface ListState {
  result: PagedResult<OrderSummary> | null;
  loading: boolean;
  failed: boolean;
}

/**
 * "My orders" for customers, and the order list of the admin panel (route data { admin: true }).
 * Same component, because the API already returns the right orders for the token:
 * customers get their own, staff get everyone's.
 */
@Component({
  selector: 'app-order-list-page',
  imports: [I18N, CurrencyPipe, FormsModule, RouterLink, MatButtonModule, MatIconModule, EmptyState, OrderStatusChip],
  templateUrl: './order-list-page.html',
  styleUrl: './order-list-page.scss',
})
export class OrderListPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(OrdersApi);

  protected readonly statuses: (OrderStatus | null)[] = [null, 'Pending', 'Paid', 'Shipped', 'Delivered', 'Cancelled'];
  protected readonly statusKey = statusKey;

  /** true inside the admin panel (from the route's data). */
  readonly admin = input(false);
  /** ?status=Paid in the URL, e.g. from the dashboard's "orders to ship" link. */
  readonly status = input<string>();

  /** Starts from the URL's status, and can then be changed by the filter buttons. */
  protected readonly selectedStatus = linkedSignal<OrderStatus | null>(
    () => (this.statuses.includes(this.status() as OrderStatus) ? (this.status() as OrderStatus) : null),
  );
  protected readonly page = signal(1);
  protected readonly search = signal('');
  private readonly searchTerm = toSignal(
    toObservable(this.search).pipe(debounceTime(300), map((s) => s.trim()), distinctUntilChanged()),
    { initialValue: '' },
  );

  protected readonly baseUrl = computed(() => (this.admin() ? '/admin/orders' : '/account/orders'));

  protected readonly query = computed(() => ({
    status: this.selectedStatus(),
    search: this.searchTerm() || null,
    page: this.page(),
    pageSize: 10,
  }));

  /** Reloads whenever the filter or page changes. */
  protected readonly state = toSignal(
    toObservable(this.query).pipe(
      switchMap((query) =>
        this.api.list(query).pipe(
          map((result): ListState => ({ result, loading: false, failed: false })),
          startWith<ListState>({ result: null, loading: true, failed: false }),
          catchError(() => of<ListState>({ result: null, loading: false, failed: true })),
        ),
      ),
    ),
    { initialValue: { result: null, loading: true, failed: false } as ListState },
  );

  protected setStatus(status: OrderStatus | null): void {
    this.selectedStatus.set(status);
    this.page.set(1);
  }

  protected onSearch(value: string): void {
    this.search.set(value);
    this.page.set(1);
  }

  protected goToPage(page: number): void {
    this.page.set(page);
  }
}
