import { computed, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withHooks, withMethods, withState } from '@ngrx/signals';
import { Observable, tap } from 'rxjs';
import { AuthResponse, LoginRequest, RegisterRequest, User } from '../models/auth.models';
import { AuthApi } from './auth.api';

const STORAGE_KEY = 'onestep.auth';

interface AuthState {
  user: User | null;
  token: string | null;
  expiresAt: string | null; // ISO date of when the token stops working
}

const initialState: AuthState = { user: null, token: null, expiresAt: null };

/**
 * Global store for "who is logged in". One instance for the whole app (providedIn: 'root').
 * The token is also saved in localStorage, so a page refresh keeps you logged in.
 */
export const AuthStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),

  withComputed(({ user, token, expiresAt }) => ({
    isLoggedIn: computed(() => !!token() && !!expiresAt() && new Date(expiresAt()!) > new Date()),
    isStaff: computed(() => user()?.role === 'Admin' || user()?.role === 'StoreManager'),
    isAdmin: computed(() => user()?.role === 'Admin'),
    isCustomer: computed(() => user()?.role === 'Customer'),
    firstName: computed(() => user()?.firstName ?? ''),
  })),

  withMethods((store, api = inject(AuthApi)) => {
    function save(response: AuthResponse): void {
      const state: AuthState = {
        user: response.user,
        token: response.accessToken,
        expiresAt: response.expiresAt,
      };
      patchState(store, state);
      try {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
      } catch {
        // Storage can be unavailable (private mode); the app still works until refresh.
      }
    }

    return {
      login(request: LoginRequest): Observable<AuthResponse> {
        return api.login(request).pipe(tap(save));
      },

      register(request: RegisterRequest): Observable<AuthResponse> {
        return api.register(request).pipe(tap(save));
      },

      logout(): void {
        patchState(store, initialState);
        try {
          localStorage.removeItem(STORAGE_KEY);
        } catch {
          // ignore
        }
      },
    };
  }),

  withHooks({
    // Runs once when the store is created: restore a saved login if it hasn't expired.
    onInit(store) {
      try {
        const saved = localStorage.getItem(STORAGE_KEY);
        if (!saved) {
          return;
        }
        const state = JSON.parse(saved) as AuthState;
        if (state.expiresAt && new Date(state.expiresAt) > new Date()) {
          patchState(store, state);
        } else {
          localStorage.removeItem(STORAGE_KEY);
        }
      } catch {
        // Broken or unavailable storage: start logged out.
      }
    },
  }),
);
