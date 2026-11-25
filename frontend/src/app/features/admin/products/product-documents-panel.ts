import { HttpErrorResponse } from '@angular/common/http';
import { Component, effect, inject, input, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ProductAdminApi } from '../../../core/admin/product-admin.api';
import { I18N } from '../../../core/i18n/translate.pipes';
import { LanguageStore } from '../../../core/i18n/language.store';
import { ProductDocument } from '../../../core/models/catalog.models';
import { formatFileSize } from '../../../core/util/file-size';
import { mediaUrl } from '../../../core/util/media';

/** Admin: upload and delete a product's PDF documents (shown to customers on the product page). */
@Component({
  selector: 'app-product-documents-panel',
  imports: [I18N, MatButtonModule, MatIconModule],
  template: `
    <section class="card" aria-labelledby="documents-admin-title">
      <h2 id="documents-admin-title">{{ 'docs.adminTitle' | t }}</h2>

      <ul>
        @for (doc of documents(); track doc.id) {
          <li>
            <mat-icon class="pdf" aria-hidden="true">picture_as_pdf</mat-icon>
            <a [href]="mediaUrl(doc.url)" target="_blank" rel="noopener">
              <span class="name">{{ doc.title }}</span>
              <span class="meta">{{ doc.fileName }} · {{ size(doc.sizeBytes) }}</span>
            </a>
            <button matIconButton (click)="remove(doc)" [attr.aria-label]="'docs.delete' | t: { title: doc.title }">
              <mat-icon aria-hidden="true">delete</mat-icon>
            </button>
          </li>
        } @empty {
          <li class="muted">{{ 'docs.none' | t }}</li>
        }
      </ul>

      <label class="title-field">
        <span>{{ 'docs.titleLabel' | t }}</span>
        <input #titleInput type="text" maxlength="150" [placeholder]="'docs.titlePlaceholder' | t" />
      </label>

      <label class="upload" [class.busy]="uploading()">
        <mat-icon aria-hidden="true">upload_file</mat-icon>
        {{ (uploading() ? 'docs.uploading' : 'docs.upload') | t }}
        <input #fileInput type="file" accept=".pdf,application/pdf" [disabled]="uploading()"
               (change)="upload(fileInput, titleInput)" />
      </label>
      <p class="muted small">{{ 'docs.rules' | t }}</p>
    </section>
  `,
  styles: `
    .card { background: var(--os-surface); border: 1px solid var(--os-line); border-radius: var(--os-radius-lg); padding: 20px; }
    h2 { font-size: 1.0625rem; font-weight: 700; margin-bottom: 14px; }
    ul { list-style: none; margin: 0 0 14px; padding: 0; display: grid; gap: 6px; }
    li { display: flex; align-items: center; gap: 10px; }
    .pdf { color: var(--os-sale); flex: none; }
    li a { display: flex; flex-direction: column; min-width: 0; flex: 1; text-decoration: none; }
    li a:hover .name { text-decoration: underline; }
    .name { font-weight: 600; overflow-wrap: anywhere; }
    .meta { color: var(--os-muted); font-size: 0.8125rem; overflow-wrap: anywhere; }
    .title-field { display: grid; gap: 4px; font-size: 0.875rem; font-weight: 600; }
    .title-field input { font: inherit; font-weight: 400; padding: 9px 10px; border: 1px solid var(--os-line);
                         border-radius: var(--os-radius-sm); background: var(--os-surface); color: var(--os-ink); }
    .upload { display: flex; align-items: center; justify-content: center; gap: 8px; margin-top: 12px; padding: 12px;
              border: 1px dashed var(--os-ink-soft); border-radius: var(--os-radius-sm); font-weight: 600; cursor: pointer; position: relative; }
    .upload:hover { background: var(--os-paper); }
    .upload:focus-within { outline: 3px solid var(--os-step); outline-offset: 2px; }
    .upload.busy { opacity: 0.6; cursor: progress; }
    .upload input { position: absolute; inset: 0; opacity: 0; cursor: pointer; }
    .muted { color: var(--os-muted); margin: 0; }
    .small { font-size: 0.8125rem; margin-top: 8px; }
  `,
})
export class ProductDocumentsPanel {
  readonly productId = input.required<number>();

  private readonly api = inject(ProductAdminApi);
  private readonly snackBar = inject(MatSnackBar);
  private readonly i18n = inject(LanguageStore);
  protected readonly mediaUrl = mediaUrl;
  protected readonly size = formatFileSize;

  protected readonly documents = signal<ProductDocument[]>([]);
  protected readonly uploading = signal(false);

  constructor() {
    effect(() => this.load(this.productId()));
  }

  protected upload(fileInput: HTMLInputElement, titleInput: HTMLInputElement): void {
    const file = fileInput.files?.[0];
    fileInput.value = '';
    if (!file) return;

    this.uploading.set(true);
    this.api.uploadDocument(this.productId(), file, titleInput.value).subscribe({
      next: (doc) => {
        this.uploading.set(false);
        titleInput.value = '';
        this.snackBar.open(this.i18n.t('docs.uploaded', { title: doc.title }), this.i18n.t('common.ok'), { duration: 3000 });
        this.load(this.productId());
      },
      error: (error: HttpErrorResponse) => {
        this.uploading.set(false);
        // The API explains what was wrong (not a PDF, too big...) in ProblemDetails.detail.
        const message = error.error?.detail ?? this.i18n.t('docs.uploadFailed', { file: file.name });
        this.snackBar.open(message, this.i18n.t('common.ok'), { duration: 5000 });
      },
    });
  }

  protected remove(doc: ProductDocument): void {
    this.api.deleteDocument(this.productId(), doc.id).subscribe(() => this.load(this.productId()));
  }

  private load(productId: number): void {
    this.api.getDocuments(productId).subscribe((documents) => this.documents.set(documents));
  }
}
