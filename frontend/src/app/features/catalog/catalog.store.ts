import { computed, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { EMPTY, catchError, distinctUntilChanged, pipe, switchMap, tap } from 'rxjs';
import { ProductsApi } from '../../core/catalog/products.api';
import { ProductFilters, ProductListItem, ProductQuery } from '../../core/models/catalog.models';
import { EMPTY_QUERY } from './product-query';

/** Everything the product list page needs to remember. */
interface CatalogState {
  query: ProductQuery;               // the current filters, sort and page
  products: ProductListItem[];       // the products on the current page
  totalCount: number;                // how many products match in total
  filters: ProductFilters | null;    // options for the filter panel
  loading: boolean;
  error: string | null;
}

const initialState: CatalogState = {
  query: EMPTY_QUERY,
  products: [],
  totalCount: 0,
  filters: null,
  loading: false,
  error: null,
};

/**
 * State for the product list page, built with NgRx Signal Store.
 * State is read through signals (store.products(), store.loading()...),
 * and changed only by the methods below, never directly by components.
 */
export const CatalogStore = signalStore(
  withState(initialState),

  // Values calculated from the state. They update automatically when the state changes.
  withComputed(({ query, totalCount }) => ({
    totalPages: computed(() => Math.max(1, Math.ceil(totalCount() / query().pageSize))),
    activeFilterCount: computed(() => {
      const q = query();
      return (
        q.brandIds.length +
        q.attributes.length +
        (q.minPrice !== null ? 1 : 0) +
        (q.maxPrice !== null ? 1 : 0) +
        (q.inStock ? 1 : 0)
      );
    }),
  })),

  withMethods((store, api = inject(ProductsApi)) => ({
    /**
     * Loads products whenever a new query arrives.
     * switchMap cancels the previous request if the user changes a filter before it finished,
     * so an old, slow response can never overwrite a newer one.
     */
    load: rxMethod<ProductQuery>(
      pipe(
        tap((query) => patchState(store, { query, loading: true, error: null })),
        switchMap((query) =>
          api.search(query).pipe(
            tap((result) =>
              patchState(store, {
                products: result.items,
                totalCount: result.totalCount,
                loading: false,
              }),
            ),
            catchError(() => {
              patchState(store, { loading: false, error: "We couldn't load the products." });
              return EMPTY;
            }),
          ),
        ),
      ),
    ),

    /** Loads the filter options for a category, only when the category actually changes. */
    loadFilters: rxMethod<number | null>(
      pipe(
        distinctUntilChanged(),
        switchMap((categoryId) =>
          api.getFilters(categoryId).pipe(
            tap((filters) => patchState(store, { filters })),
            catchError(() => EMPTY),
          ),
        ),
      ),
    ),
  })),
);
