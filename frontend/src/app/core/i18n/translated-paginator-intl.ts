import { Injectable, effect, inject } from '@angular/core';
import { MatPaginatorIntl } from '@angular/material/paginator';
import { LanguageStore } from './language.store';

/** The texts inside Angular Material's paginator ("Items per page", "1–12 of 40"), in the current language. */
@Injectable()
export class TranslatedPaginatorIntl extends MatPaginatorIntl {
  private readonly language = inject(LanguageStore);

  constructor() {
    super();
    effect(() => {
      const t = this.language.t;
      this.language.lang(); // re-run when the language changes
      this.itemsPerPageLabel = t('paginator.perPage');
      this.nextPageLabel = t('paginator.next');
      this.previousPageLabel = t('paginator.previous');
      this.firstPageLabel = t('paginator.first');
      this.lastPageLabel = t('paginator.last');
      this.getRangeLabel = (page, pageSize, length) => {
        const from = length === 0 ? 0 : page * pageSize + 1;
        const to = Math.min((page + 1) * pageSize, length);
        return t('paginator.range', { from, to, total: length });
      };
      this.changes.next(); // tell the paginator to redraw
    });
  }
}
