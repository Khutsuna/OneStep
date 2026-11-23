import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink } from '@angular/router';
import { catchError, map, of, startWith, switchMap } from 'rxjs';
import { AuthStore } from '../../../core/auth/auth.store';
import { PagedResult } from '../../../core/models/common.models';
import { OrderListQuery, OrderStatus, OrderSummary } from '../../../core/models/shop.models';
import { OrdersApi } from '../../../core/shop/orders.api';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { ORDER_STATUS_LABELS, OrderStatusChip } from '../../../shared/ui/order-status/order-status-chip';

interface ListState {
  result: PagedResult<OrderSummary> | null;
  loading: boolean;
  failed: boolean;
}

/** "My orders": newest first, filterable by status. Staff see everyone's orders here. */
@Component({
  selector: 'app-order-list-page',
  imports: [CurrencyPipe, DatePipe, RouterLink, MatButtonModule, MatIconModule, EmptyState, OrderStatusChip],
  templateUrl: './order-list-page.html',
  styleUrl: './order-list-page.scss',
})
export class OrderListPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(OrdersApi);

  protected readonly statuses: (OrderStatus | null)[] = [null, 'Pending', 'Paid', 'Shipped', 'Delivered', 'Cancelled'];
  protected readonly statusLabels = ORDER_STATUS_LABELS;

  protected readonly query = signal<OrderListQuery>({ status: null, search: null, page: 1, pageSize: 10 });

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
    this.query.update((q) => ({ ...q, status, page: 1 }));
  }

  protected goToPage(page: number): void {
    this.query.update((q) => ({ ...q, page }));
  }
}
