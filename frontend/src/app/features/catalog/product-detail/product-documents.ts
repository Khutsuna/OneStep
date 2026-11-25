import { Component, inject, input } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { MatIconModule } from '@angular/material/icon';
import { catchError, of, switchMap } from 'rxjs';
import { ProductsApi } from '../../../core/catalog/products.api';
import { I18N } from '../../../core/i18n/translate.pipes';
import { ProductDocument } from '../../../core/models/catalog.models';
import { formatFileSize } from '../../../core/util/file-size';
import { mediaUrl } from '../../../core/util/media';

/**
 * The product's downloadable documents (user manual, warranty card), uploaded by staff in the admin panel.
 * Hidden when the product has none. Same loading pattern as the price history chart.
 */
@Component({
  selector: 'app-product-documents',
  imports: [I18N, MatIconModule],
  template: `
    @if (documents().length) {
      <section class="box" aria-labelledby="documents-title">
        <h2 id="documents-title">{{ 'docs.title' | t }}</h2>
        <ul>
          @for (doc of documents(); track doc.id) {
            <li>
              <a [href]="mediaUrl(doc.url)" target="_blank" rel="noopener" [attr.download]="doc.fileName"
                 [attr.aria-label]="'docs.download' | t: { title: doc.title }">
                <mat-icon class="pdf" aria-hidden="true">picture_as_pdf</mat-icon>
                <span class="text">
                  <span class="name">{{ doc.title }}</span>
                  <span class="meta">PDF · {{ size(doc.sizeBytes) }}</span>
                </span>
                <mat-icon class="get" aria-hidden="true">download</mat-icon>
              </a>
            </li>
          }
        </ul>
      </section>
    }
  `,
  styles: `
    .box { margin-top: 24px; padding: 18px; border: 1px solid var(--os-line); border-radius: var(--os-radius-lg); background: var(--os-surface); }
    h2 { font-size: 1.125rem; font-weight: 700; margin: 0 0 10px; }
    ul { list-style: none; margin: 0; padding: 0; display: grid; gap: 8px; }
    a { display: flex; align-items: center; gap: 12px; padding: 10px 12px; border: 1px solid var(--os-line);
        border-radius: var(--os-radius-sm); text-decoration: none; transition: border-color 160ms ease, background-color 160ms ease; }
    a:hover { border-color: var(--os-ink); background: var(--os-paper); }
    .pdf { color: var(--os-sale); flex: none; }
    .text { display: flex; flex-direction: column; min-width: 0; flex: 1; }
    .name { font-weight: 600; overflow-wrap: anywhere; }
    .meta { color: var(--os-muted); font-size: 0.8125rem; }
    .get { color: var(--os-muted); flex: none; }
  `,
})
export class ProductDocuments {
  readonly productId = input.required<number>();

  private readonly api = inject(ProductsApi);
  protected readonly mediaUrl = mediaUrl;
  protected readonly size = formatFileSize;

  /** Reloads when the product changes (for example after clicking a similar product). */
  protected readonly documents = toSignal(
    toObservable(this.productId).pipe(
      switchMap((id) => this.api.getDocuments(id).pipe(catchError(() => of([] as ProductDocument[])))),
    ),
    { initialValue: [] as ProductDocument[] },
  );
}
