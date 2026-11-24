import { CurrencyPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink } from '@angular/router';
import { catchError, map, of, startWith, switchMap } from 'rxjs';
import { AdminApi } from '../../../core/admin/admin.api';
import { Dashboard, LowStockProduct } from '../../../core/models/admin.models';
import { NotificationsStore } from '../layout/notifications.store';
import { SalesChart } from './sales-chart';
import { I18N } from '../../../core/i18n/translate.pipes';

/** The admin home page: key numbers, sales chart, best sellers, low stock and alerts. */
@Component({
  selector: 'app-dashboard-page',
  imports: [I18N, CurrencyPipe, RouterLink, MatIconModule, SalesChart],
  templateUrl: './dashboard-page.html',
  styleUrl: './dashboard-page.scss',
})
export class DashboardPage {
  private readonly api = inject(AdminApi);
  /** The same instance as the bell in the top bar (provided by AdminLayout). */
  protected readonly notifications = inject(NotificationsStore);

  protected readonly periods = [7, 30, 90];
  protected readonly days = signal(30);

  protected readonly dashboard = toSignal(
    toObservable(this.days).pipe(
      switchMap((days) =>
        this.api.getDashboard(days).pipe(
          map((data) => ({ data, failed: false })),
          startWith(null),
          catchError(() => of({ data: null as Dashboard | null, failed: true })),
        ),
      ),
    ),
    { initialValue: null },
  );

  protected readonly lowStock = toSignal(this.api.getLowStock().pipe(catchError(() => of([] as LowStockProduct[]))), {
    initialValue: [] as LowStockProduct[],
  });
}
