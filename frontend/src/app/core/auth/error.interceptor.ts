import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthStore } from './auth.store';

/**
 * Handles errors that every page would otherwise repeat:
 * 401 -> session ended, go to login; 403 -> no access; 0 -> server down; 5xx -> server error.
 * The error is still passed on, so the page can react too (e.g. stop a spinner).
 */
export const errorInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthStore);
  const router = inject(Router);
  const snackBar = inject(MatSnackBar);

  return next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      const isAuthCall = request.url.includes('/api/auth/');

      if (error.status === 401 && !isAuthCall) {
        auth.logout();
        snackBar.open('Your session has ended. Please log in again.', 'OK', { duration: 5000 });
        void router.navigate(['/login'], { queryParams: { returnUrl: router.url } });
      } else if (error.status === 403) {
        snackBar.open("You don't have access to this.", 'OK', { duration: 5000 });
      } else if (error.status === 0) {
        snackBar.open("Can't reach the OneStep server. Check that the API is running.", 'OK', {
          duration: 6000,
        });
      } else if (error.status >= 500) {
        snackBar.open('Something went wrong on the server. Please try again.', 'OK', {
          duration: 5000,
        });
      }

      return throwError(() => error);
    }),
  );
};
