import { CurrencyPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { RouterLink } from '@angular/router';
import { catchError, debounceTime, distinctUntilChanged, map, of, startWith, switchMap } from 'rxjs';
import { ProductAdminApi } from '../../../core/admin/product-admin.api';
import { ProductListItem } from '../../../core/models/catalog.models';
import { PagedResult } from '../../../core/models/common.models';
import { ProductImage } from '../../../shared/ui/product-image/product-image';

/** Product list for staff: search, inactive products included, quick stock edit. */
@Component({
  selector: 'app-admin-products-page',
  imports: [CurrencyPipe, FormsModule, RouterLink, MatButtonModule, MatIconModule, ProductImage],
  templateUrl: './admin-products-page.html',
  styleUrl: './admin-products-page.scss',
})
export class AdminProductsPage {
  private readonly api = inject(ProductAdminApi);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly search = signal('');
  protected readonly page = signal(1);
  private readonly reload = signal(0);

  /** Typing waits 300 ms (debounce), so we don't call the API on every key press. */
  private readonly searchTerm = toSignal(
    toObservable(this.search).pipe(debounceTime(300), map((s) => s.trim()), distinctUntilChanged()),
    { initialValue: '' },
  );

  /** Everything the list depends on. When any part changes, the list reloads once. */
  private readonly query = computed(() => ({ search: this.searchTerm(), page: this.page(), reload: this.reload() }));

  protected readonly result = toSignal(
    toObservable(this.query).pipe(
      switchMap((q) =>
        this.api.list(q.search || null, q.page).pipe(
          map((r): PagedResult<ProductListItem> | null => r),
          startWith(null),
          catchError(() => of(null)),
        ),
      ),
    ),
    { initialValue: null },
  );

  protected onSearch(value: string): void {
    this.search.set(value);
    this.page.set(1);
  }

  /** Saves a new stock number when the field loses focus or Enter is pressed. */
  protected saveStock(product: ProductListItem, input: HTMLInputElement): void {
    const value = Number(input.value);
    if (!Number.isInteger(value) || value < 0) {
      input.value = String(product.stockQuantity);
      return;
    }
    if (value === product.stockQuantity) return;

    this.api.updateStock(product.id, value).subscribe({
      next: () => {
        this.snackBar.open(`Stock of ${product.name} is now ${value}.`, 'OK', { duration: 3000 });
        this.reload.update((n) => n + 1);
      },
      error: (error: HttpErrorResponse) => {
        input.value = String(product.stockQuantity);
        this.snackBar.open(error.error?.detail ?? "Stock wasn't saved.", 'OK', { duration: 5000 });
      },
    });
  }
}
