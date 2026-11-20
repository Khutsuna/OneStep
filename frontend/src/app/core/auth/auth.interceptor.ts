import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { AuthStore } from './auth.store';

/**
 * Runs for every HTTP request the app makes. If the user is logged in and the request
 * goes to OUR API, it adds the header "Authorization: Bearer <token>".
 * Requests to other sites never get the token, so it can't leak.
 */
export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const token = inject(AuthStore).token();

  if (!token || !request.url.startsWith(environment.apiUrl)) {
    return next(request);
  }

  // Requests are immutable, so we clone it with the extra header.
  const withToken = request.clone({
    setHeaders: { Authorization: `Bearer ${token}` },
  });
  return next(withToken);
};
