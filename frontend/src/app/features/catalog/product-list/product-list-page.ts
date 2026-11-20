import { Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { map } from 'rxjs';
import { CategoryStore } from '../../../core/catalog/category.store';
import { ProductQuery, ProductSort } from '../../../core/models/catalog.models';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { ProductCard } from '../../../shared/ui/product-card/product-card';
import { CatalogStore } from '../catalog.store';
import { FilterPanel } from '../filter-panel/filter-panel';
import { parseProductQuery, toQueryParams } from '../product-query';

/**
 * Page component ("smart"): connects the URL, the CatalogStore and the presentational components.
 * The URL is the single source of truth: changing a filter changes the URL,
 * and the store reloads whenever the URL changes.
 */
@Component({
  selector: 'app-product-list-page',
  imports: [
    RouterLink,
    MatButtonModule,
    MatIconModule,
    MatPaginatorModule,
    MatProgressBarModule,
    MatSelectModule,
    EmptyState,
    FilterPanel,
    ProductCard,
  ],
  providers: [CatalogStore], // a fresh store for this page, destroyed when you leave it
  templateUrl: './product-list-page.html',
  styleUrl: './product-list-page.scss',
})
export class ProductListPage {
  protected readonly store = inject(CatalogStore);
  protected readonly categories = inject(CategoryStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly filtersOpen = signal(false);
  protected readonly sortOptions: { value: ProductSort; label: string }[] = [
    { value: 'Newest', label: 'Newest first' },
    { value: 'PriceAsc', label: 'Price: low to high' },
    { value: 'PriceDesc', label: 'Price: high to low' },
    { value: 'Rating', label: 'Best rated' },
    { value: 'Name', label: 'Name A–Z' },
  ];

  /** The current query, read from the URL. */
  private readonly query = toSignal(this.route.queryParamMap.pipe(map(parseProductQuery)), {
    requireSync: true,
  });

  protected readonly breadcrumb = computed(() => {
    const categoryId = this.query().categoryId;
    return categoryId ? this.categories.pathTo(categoryId) : [];
  });

  protected readonly title = computed(() => {
    const query = this.query();
    if (query.search) {
      return `Results for “${query.search}”`;
    }
    const path = this.breadcrumb();
    return path.length ? path[path.length - 1].name : 'All products';
  });

  constructor() {
    // Give the store a signal: it reloads automatically every time the query signal changes.
    this.store.load(this.query);
    this.store.loadFilters(computed(() => this.query().categoryId));
  }

  /** Changes part of the query by navigating to the new URL. Filter changes go back to page 1. */
  protected update(changes: Partial<ProductQuery>, resetPage = true): void {
    const next: ProductQuery = { ...this.query(), ...changes };
    if (resetPage) {
      next.page = 1;
    }
    void this.router.navigate([], { relativeTo: this.route, queryParams: toQueryParams(next) });
  }

  protected onPage(event: PageEvent): void {
    const pageSizeChanged = event.pageSize !== this.query().pageSize;
    this.update({ page: pageSizeChanged ? 1 : event.pageIndex + 1, pageSize: event.pageSize }, false);
  }

  protected clearFilters(): void {
    this.update({ brandIds: [], attributes: [], minPrice: null, maxPrice: null, inStock: false, search: null });
  }

  protected retry(): void {
    this.store.load(this.query());
  }
}
