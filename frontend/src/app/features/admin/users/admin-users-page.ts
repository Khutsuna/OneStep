import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, debounceTime, distinctUntilChanged, map, of, startWith, switchMap } from 'rxjs';
import { AdminApi } from '../../../core/admin/admin.api';
import { AuthStore } from '../../../core/auth/auth.store';
import { UserListItem } from '../../../core/models/admin.models';
import { UserRole } from '../../../core/models/auth.models';
import { PagedResult } from '../../../core/models/common.models';
import { I18N } from '../../../core/i18n/translate.pipes';
import { LanguageStore } from '../../../core/i18n/language.store';
import { TranslationKey } from '../../../core/i18n/en';

/** Admin only: every account, with its role and whether it can log in. */
@Component({
  selector: 'app-admin-users-page',
  imports: [I18N, FormsModule, MatButtonModule, MatIconModule, MatSlideToggleModule],
  templateUrl: './admin-users-page.html',
  styleUrl: './admin-users-page.scss',
})
export class AdminUsersPage {
  private readonly api = inject(AdminApi);
  private readonly snackBar = inject(MatSnackBar);
  private readonly i18n = inject(LanguageStore);
  protected readonly auth = inject(AuthStore);

  protected readonly roles: UserRole[] = ['Customer', 'StoreManager', 'Admin'];
  protected readonly roleKey = (role: UserRole): TranslationKey => `users.role${role}`;

  protected readonly search = signal('');
  protected readonly role = signal<UserRole | null>(null);
  protected readonly page = signal(1);
  private readonly reload = signal(0);

  private readonly searchTerm = toSignal(
    toObservable(this.search).pipe(debounceTime(300), map((s) => s.trim()), distinctUntilChanged()),
    { initialValue: '' },
  );

  private readonly query = computed(() => ({
    search: this.searchTerm(),
    role: this.role(),
    page: this.page(),
    reload: this.reload(),
  }));

  protected readonly result = toSignal(
    toObservable(this.query).pipe(
      switchMap((q) =>
        this.api.getUsers(q.search || null, q.role, q.page).pipe(
          map((r): PagedResult<UserListItem> | null => r),
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

  protected onRoleFilter(value: string): void {
    this.role.set((value || null) as UserRole | null);
    this.page.set(1);
  }

  protected isMe(user: UserListItem): boolean {
    return user.id === this.auth.user()?.id;
  }

  protected update(user: UserListItem, role: UserRole, isActive: boolean): void {
    this.api.updateUser(user.id, role, isActive).subscribe({
      next: (updated) => {
        this.snackBar.open(this.i18n.t('users.saved', { name: `${updated.firstName} ${updated.lastName}` }), this.i18n.t('common.ok'), { duration: 3000 });
        this.reload.update((n) => n + 1);
      },
      error: (error: HttpErrorResponse) => {
        this.snackBar.open(error.error?.detail ?? this.i18n.t('users.notSaved'), this.i18n.t('common.ok'), { duration: 5000 });
        this.reload.update((n) => n + 1); // show the real values again
      },
    });
  }
}
