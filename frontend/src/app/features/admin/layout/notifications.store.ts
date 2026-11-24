import { computed, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withHooks, withMethods, withState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { EMPTY, catchError, pipe, switchMap, tap, timer } from 'rxjs';
import { AdminApi } from '../../../core/admin/admin.api';
import { StaffNotification } from '../../../core/models/admin.models';

interface NotificationsState {
  items: StaffNotification[];
  unreadCount: number;
}

/**
 * Staff alerts (low stock, new orders) for the admin panel.
 * Unlike CartStore, this store is NOT global: it's provided by AdminLayout,
 * so it exists only while the admin panel is open and stops polling when you leave.
 */
export const NotificationsStore = signalStore(
  withState<NotificationsState>({ items: [], unreadCount: 0 }),

  withComputed(({ unreadCount }) => ({
    hasUnread: computed(() => unreadCount() > 0),
  })),

  withMethods((store, api = inject(AdminApi)) => ({
    /** Checks for new alerts now and then every 60 seconds. switchMap drops a slow, older request. */
    poll: rxMethod<void>(
      pipe(
        switchMap(() => timer(0, 60_000)),
        switchMap(() =>
          api.getNotifications(20).pipe(
            tap((list) => patchState(store, { items: list.items, unreadCount: list.unreadCount })),
            catchError(() => EMPTY),
          ),
        ),
      ),
    ),

    markRead(id: number): void {
      const target = store.items().find((n) => n.id === id);
      if (!target || target.isRead) return;
      api.markNotificationRead(id).subscribe(() =>
        patchState(store, (state) => ({
          items: state.items.map((n) => (n.id === id ? { ...n, isRead: true } : n)),
          unreadCount: Math.max(0, state.unreadCount - 1),
        })),
      );
    },

    markAllRead(): void {
      api.markAllNotificationsRead().subscribe(() =>
        patchState(store, (state) => ({
          items: state.items.map((n) => ({ ...n, isRead: true })),
          unreadCount: 0,
        })),
      );
    },
  })),

  withHooks({
    onInit(store) {
      store.poll();
    },
  }),
);
