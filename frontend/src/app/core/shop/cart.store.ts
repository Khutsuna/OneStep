import { computed, effect, inject, untracked } from '@angular/core';
import { patchState, signalStore, withComputed, withHooks, withMethods, withState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { EMPTY, Observable, catchError, pipe, switchMap, tap } from 'rxjs';
import { AuthStore } from '../auth/auth.store';
import { Cart } from '../models/shop.models';
import { CartApi } from './cart.api';

interface CartState {
  cart: Cart | null; // null = not loaded yet (or nobody is logged in)
  loading: boolean;
}

const initialState: CartState = { cart: null, loading: false };

/**
 * Global store for the shopping cart. One instance for the whole app (providedIn: 'root'),
 * because the header badge, the product page and the cart page all show the same cart.
 * The server owns the cart: every change goes to the API, and the store keeps the cart it returns.
 */
export const CartStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),

  withComputed(({ cart }) => ({
    itemCount: computed(() => cart()?.itemCount ?? 0),
    items: computed(() => cart()?.items ?? []),
    isEmpty: computed(() => (cart()?.items.length ?? 0) === 0),
  })),

  withMethods((store, api = inject(CartApi)) => {
    /** Saves the cart the API returned. Used after every successful change. */
    const save = (cart: Cart) => patchState(store, { cart, loading: false });

    return {
      /** Loads the cart from the API. switchMap ignores an older request if load() is called again. */
      load: rxMethod<void>(
        pipe(
          tap(() => patchState(store, { loading: true })),
          switchMap(() =>
            api.get().pipe(
              tap(save),
              catchError(() => {
                patchState(store, { loading: false });
                return EMPTY;
              }),
            ),
          ),
        ),
      ),

      // These return an Observable, so the page can react (show a message, stop a spinner).
      add(productId: number, quantity: number): Observable<Cart> {
        return api.add(productId, quantity).pipe(tap(save));
      },

      updateQuantity(itemId: number, quantity: number): Observable<Cart> {
        return api.updateQuantity(itemId, quantity).pipe(tap(save));
      },

      remove(itemId: number): Observable<Cart> {
        return api.remove(itemId).pipe(tap(save));
      },

      /** Forget the cart locally, e.g. after logout or after the order was placed (the API emptied it). */
      reset(): void {
        patchState(store, initialState);
      },
    };
  }),

  withHooks({
    onInit(store) {
      const auth = inject(AuthStore);
      // Runs now and again whenever the login changes:
      // a customer logs in -> load their cart; someone logs out -> forget it.
      effect(() => {
        const isCustomer = auth.isLoggedIn() && auth.isCustomer();
        untracked(() => (isCustomer ? store.load() : store.reset()));
      });
    },
  }),
);
