import { formatDate } from '@angular/common';
import { Pipe, PipeTransform, inject } from '@angular/core';
import { TranslationKey } from './en';
import { LanguageStore } from './language.store';

/**
 * {{ 'cart.title' | t }} or {{ 'orders.count' | t: { count: 3 } }}
 * Not "pure", so it runs again when the language changes. It only does a dictionary lookup, so that's cheap.
 */
@Pipe({ name: 't', pure: false })
export class TranslatePipe implements PipeTransform {
  private readonly language = inject(LanguageStore);

  transform(key: TranslationKey, params?: Record<string, string | number | null | undefined>): string {
    return this.language.t(key, params);
  }
}

/** {{ category.name | term }}: translates names stored in the database (categories, attributes...). */
@Pipe({ name: 'term', pure: false })
export class TermPipe implements PipeTransform {
  private readonly language = inject(LanguageStore);

  transform(name: string | null | undefined): string {
    return this.language.term(name);
  }
}

/** {{ order.createdAt | localDate: 'd MMM y' }}: like Angular's date pipe, in the current language. */
@Pipe({ name: 'localDate', pure: false })
export class LocalDatePipe implements PipeTransform {
  private readonly language = inject(LanguageStore);

  transform(value: string | Date | null | undefined, format = 'd MMM y'): string {
    return value ? formatDate(value, format, this.language.locale()) : '';
  }
}

/** Import this in a component to get all three pipes. */
export const I18N = [TranslatePipe, TermPipe, LocalDatePipe] as const;
