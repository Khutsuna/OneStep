import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStore } from './auth.store';

/**
 * Route guards only decide which SCREENS are shown. The real security is in the API,
 * which checks the token and role on every request.
 */

/** Only for logged-in users; others go to /login and come back afterwards. */
export const authGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthStore);
  return auth.isLoggedIn()
    ? true
    : inject(Router).createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};

/** Only for Admins and Store Managers. */
export const staffGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthStore);
  if (!auth.isLoggedIn()) {
    return inject(Router).createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
  }
  return auth.isStaff() ? true : inject(Router).createUrlTree(['/']);
};

/** Only for customers (the cart and checkout). Staff accounts can't shop, so they go home. */
export const customerGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthStore);
  if (!auth.isLoggedIn()) {
    return inject(Router).createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
  }
  return auth.isCustomer() ? true : inject(Router).createUrlTree(['/']);
};
