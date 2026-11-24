import { computed, effect, inject, untracked } from '@angular/core';
import { patchState, signalStore, withComputed, withHooks, withMethods, withState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { EMPTY, catchError, pipe, switchMap, tap } from 'rxjs';
import { AuthStore } from '../auth/auth.store';
import { WishlistApi } from './wishlist.api';

interface WishlistState {
  ids: number[]; // the saved product ids: enough to colour every heart on the page
}

/**
 * Global store for the wishlist, built like CartStore.
 * One difference: toggle() is OPTIMISTIC. The heart changes colour at once, and if the API
 * call fails, the store puts it back. Clicking a heart feels instant that way.
 */
export const WishlistStore = signalStore(
  { providedIn: 'root' },
  withState<WishlistState>({ ids: [] }),

  withComputed(({ ids }) => ({
    count: computed(() => ids().length),
    /** A Set makes "is this product saved?" quick, even with many hearts on a page. */
    idSet: computed(() => new Set(ids())),
  })),

  withMethods((store, api = inject(WishlistApi)) => ({
    load: rxMethod<void>(
      pipe(
        switchMap(() =>
          api.getIds().pipe(
            tap((ids) => patchState(store, { ids })),
            catchError(() => EMPTY),
          ),
        ),
      ),
    ),

    has(productId: number): boolean {
      return store.idSet().has(productId);
    },

    toggle(productId: number): void {
      const wasSaved = store.idSet().has(productId);
      const before = store.ids();

      // 1. Update the screen straight away.
      patchState(store, { ids: wasSaved ? before.filter((id) => id !== productId) : [productId, ...before] });

      // 2. Tell the API. If it fails, undo step 1.
      const request = wasSaved ? api.remove(productId) : api.add(productId);
      request.subscribe({ error: () => patchState(store, { ids: before }) });
    },

    reset(): void {
      patchState(store, { ids: [] });
    },
  })),

  withHooks({
    onInit(store) {
      const auth = inject(AuthStore);
      effect(() => {
        const isCustomer = auth.isLoggedIn() && auth.isCustomer();
        untracked(() => (isCustomer ? store.load() : store.reset()));
      });
    },
  }),
);
