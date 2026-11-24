import { registerLocaleData } from '@angular/common';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import localeKa from '@angular/common/locales/ka';
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { MAT_FORM_FIELD_DEFAULT_OPTIONS } from '@angular/material/form-field';
import { MAT_ICON_DEFAULT_OPTIONS } from '@angular/material/icon';
import { MatPaginatorIntl } from '@angular/material/paginator';
import { TitleStrategy, provideRouter, withComponentInputBinding, withInMemoryScrolling } from '@angular/router';
import { routes } from './app.routes';
import { authInterceptor } from './core/auth/auth.interceptor';
import { errorInterceptor } from './core/auth/error.interceptor';
import { TranslatedPaginatorIntl } from './core/i18n/translated-paginator-intl';
import { TranslatedTitleStrategy } from './core/i18n/translated-title.strategy';

// Georgian month and day names for dates (the English ones are built in).
registerLocaleData(localeKa);

export const appConfig: ApplicationConfig = {
  providers: [
    { provide: TitleStrategy, useClass: TranslatedTitleStrategy },
    { provide: MatPaginatorIntl, useClass: TranslatedPaginatorIntl },
    provideBrowserGlobalErrorListeners(),
    provideRouter(
      routes,
      withComponentInputBinding(), // route params and query params arrive as component inputs
      withInMemoryScrolling({ scrollPositionRestoration: 'top' }),
    ),
    // Every HTTP request passes through these interceptors, in this order.
    provideHttpClient(withFetch(), withInterceptors([authInterceptor, errorInterceptor])),
    { provide: MAT_ICON_DEFAULT_OPTIONS, useValue: { fontSet: 'material-symbols-outlined' } },
    { provide: MAT_FORM_FIELD_DEFAULT_OPTIONS, useValue: { appearance: 'outline' } },
  ],
};
