import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthStore } from './auth.store';
import { LanguageStore } from '../i18n/language.store';

/**
 * Handles errors that every page would otherwise repeat:
 * 401 -> session ended, go to login; 403 -> no access; 0 -> server down; 5xx -> server error.
 * The error is still passed on, so the page can react too (e.g. stop a spinner).
 */
export const errorInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthStore);
  const router = inject(Router);
  const snackBar = inject(MatSnackBar);
  const i18n = inject(LanguageStore);

  return next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      const isAuthCall = request.url.includes('/api/auth/');

      if (error.status === 401 && !isAuthCall) {
        auth.logout();
        snackBar.open(i18n.t('errors.session'), i18n.t('common.ok'), { duration: 5000 });
        void router.navigate(['/login'], { queryParams: { returnUrl: router.url } });
      } else if (error.status === 403) {
        snackBar.open(i18n.t('errors.forbidden'), i18n.t('common.ok'), { duration: 5000 });
      } else if (error.status === 0) {
        snackBar.open(i18n.t('errors.offline'), i18n.t('common.ok'), {
          duration: 6000,
        });
      } else if (error.status >= 500) {
        snackBar.open(i18n.t('errors.server'), i18n.t('common.ok'), {
          duration: 5000,
        });
      }

      return throwError(() => error);
    }),
  );
};
