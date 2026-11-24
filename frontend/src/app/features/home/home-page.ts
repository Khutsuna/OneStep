import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink } from '@angular/router';
import { catchError, map, of } from 'rxjs';
import { CategoryStore, categoryIcon } from '../../core/catalog/category.store';
import { ProductsApi } from '../../core/catalog/products.api';
import { I18N } from '../../core/i18n/translate.pipes';
import { ProductListItem } from '../../core/models/catalog.models';
import { ProductCard } from '../../shared/ui/product-card/product-card';
import { EMPTY_QUERY } from '../catalog/product-query';

@Component({
  selector: 'app-home-page',
  imports: [I18N, RouterLink, MatIconModule, ProductCard],
  templateUrl: './home-page.html',
  styleUrl: './home-page.scss',
})
export class HomePage {
  protected readonly categories = inject(CategoryStore);
  private readonly api = inject(ProductsApi);
  protected readonly categoryIcon = categoryIcon;

  // null = still loading; [] = loaded but empty (or failed).
  protected readonly newest = toSignal(this.load('Newest', 10), { initialValue: null });
  protected readonly topRated = toSignal(this.load('Rating', 5), { initialValue: null });

  /** Products with a running campaign discount. The API has no "on sale" filter, so we pick them from a larger page. */
  protected readonly deals = toSignal(
    this.load('Newest', 40).pipe(map((items) => (items ?? []).filter((p) => p.discount > 0).slice(0, 5))),
    { initialValue: null },
  );

  private load(sort: 'Newest' | 'Rating', pageSize: number) {
    return this.api.search({ ...EMPTY_QUERY, sort, pageSize }).pipe(
      map((result): ProductListItem[] | null => result.items),
      catchError(() => of([] as ProductListItem[])),
    );
  }

  protected scrollToDeals(target: HTMLElement): void {
    target.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }
}
