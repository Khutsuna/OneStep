import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormArray, FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router, RouterLink } from '@angular/router';
import { catchError, concatMap, from, of, toArray } from 'rxjs';
import { ProductAdminApi } from '../../../core/admin/product-admin.api';
import { CategoryStore } from '../../../core/catalog/category.store';
import { NamedOption, ProductRequest } from '../../../core/models/admin.models';
import { ProductDetails, ProductStatus } from '../../../core/models/catalog.models';
import { applyServerErrors } from '../../../core/util/form-errors';
import { mediaUrl } from '../../../core/util/media';
import { I18N } from '../../../core/i18n/translate.pipes';
import { LanguageStore } from '../../../core/i18n/language.store';

/** One "Color = Black" row of the specifications list. */
type AttributeRow = FormGroup<{ attributeId: FormControl<number>; value: FormControl<string> }>;

/** Create a product (/admin/products/new) or edit one (/admin/products/12), including its photos. */
@Component({
  selector: 'app-product-edit-page',
  imports: [I18N, ReactiveFormsModule, RouterLink, MatButtonModule, MatFormFieldModule, MatIconModule, MatInputModule, MatSelectModule,
  ],
  templateUrl: './product-edit-page.html',
  styleUrl: './product-edit-page.scss',
})
export class ProductEditPage {
  /** From the URL; missing on /admin/products/new. */
  readonly id = input<string>();

  private readonly api = inject(ProductAdminApi);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);
  private readonly i18n = inject(LanguageStore);
  protected readonly categories = inject(CategoryStore);
  protected readonly mediaUrl = mediaUrl;

  protected readonly isNew = computed(() => !this.id());
  protected readonly product = signal<ProductDetails | null>(null);
  protected readonly saving = signal(false);
  protected readonly uploading = signal(false);
  protected readonly confirmingDelete = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly brands = toSignal(this.api.getBrands().pipe(catchError(() => of([] as NamedOption[]))), {
    initialValue: [] as NamedOption[],
  });
  protected readonly attributeOptions = toSignal(this.api.getAttributes().pipe(catchError(() => of([] as NamedOption[]))), {
    initialValue: [] as NamedOption[],
  });

  private readonly fb = inject(FormBuilder).nonNullable;

  protected readonly form = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    sku: ['', [Validators.required, Validators.maxLength(50)]],
    description: ['', Validators.required],
    price: [0, [Validators.required, Validators.min(0)]],
    categoryId: [0, [Validators.required, Validators.min(1)]],
    brandId: [null as number | null],
    stockQuantity: [0, [Validators.required, Validators.min(0)]],
    lowStockThreshold: [5, [Validators.required, Validators.min(0)]],
    status: ['Active' as ProductStatus],
    // A FormArray is a list of small forms: one row per attribute (e.g. Color = Black).
    attributes: this.fb.array<AttributeRow>([]),
  });

  protected get attributes(): FormArray<AttributeRow> {
    return this.form.controls.attributes;
  }

  constructor() {
    // Load the product when editing (and again if the id in the URL changes).
    effect(() => {
      const id = this.id();
      if (!id) return;
      this.api.getById(Number(id)).subscribe({
        next: (product) => this.fill(product),
        error: () => this.errorMessage.set(this.i18n.t('pe.loadFailed')),
      });
    });
  }

  private attributeRow(attributeId = 0, value = ''): AttributeRow {
    return this.fb.group({
      attributeId: [attributeId, [Validators.required, Validators.min(1)]],
      value: [value, [Validators.required, Validators.maxLength(100)]],
    });
  }

  protected addAttribute(): void {
    this.form.controls.attributes.push(this.attributeRow());
  }

  protected removeAttribute(index: number): void {
    this.form.controls.attributes.removeAt(index);
  }

  private fill(product: ProductDetails): void {
    this.product.set(product);
    this.form.patchValue({
      name: product.name,
      sku: product.sku,
      description: product.description,
      price: product.price,
      categoryId: product.categoryId,
      brandId: product.brandId,
      stockQuantity: product.stockQuantity,
      lowStockThreshold: product.lowStockThreshold,
      status: product.status,
    });
    this.form.controls.attributes.clear();
    product.attributes.forEach((a) => this.form.controls.attributes.push(this.attributeRow(a.attributeId, a.value)));
  }

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const request: ProductRequest = this.form.getRawValue();
    const existing = this.product();
    this.saving.set(true);
    this.errorMessage.set(null);

    const save$ = existing ? this.api.update(existing.id, request) : this.api.create(request);
    save$.subscribe({
      next: (product) => {
        this.saving.set(false);
        if (existing) {
          this.fill(product);
          this.snackBar.open(this.i18n.t('pe.saved'), this.i18n.t('common.ok'), { duration: 3000 });
        } else {
          // Go to the edit page of the new product, where photos can be added.
          this.snackBar.open(this.i18n.t('pe.created'), this.i18n.t('common.ok'), { duration: 4000 });
          void this.router.navigate(['/admin/products', product.id]);
        }
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.errorMessage.set(applyServerErrors(this.form, error, this.i18n));
      },
    });
  }

  /** Uploads the chosen files one after another (concatMap keeps the order), then reloads the product. */
  protected upload(input: HTMLInputElement): void {
    const product = this.product();
    const files = Array.from(input.files ?? []);
    input.value = '';
    if (!product || files.length === 0) return;

    this.uploading.set(true);
    from(files)
      .pipe(
        concatMap((file) =>
          this.api.uploadImage(product.id, file).pipe(
            catchError((error: HttpErrorResponse) => {
              this.snackBar.open(error.error?.detail ? `${file.name}: ${error.error.detail}` : this.i18n.t('pe.uploadFailed', { file: file.name }), this.i18n.t('common.ok'), { duration: 5000 });
              return of(null);
            }),
          ),
        ),
        toArray(),
      )
      .subscribe(() => {
        this.uploading.set(false);
        this.reload();
      });
  }

  protected setMain(imageId: number): void {
    const product = this.product()!;
    this.api.setMainImage(product.id, imageId).subscribe(() => this.reload());
  }

  protected deleteImage(imageId: number): void {
    const product = this.product()!;
    this.api.deleteImage(product.id, imageId).subscribe(() => this.reload());
  }

  protected deleteProduct(): void {
    const product = this.product()!;
    this.api.delete(product.id).subscribe(() => {
      this.snackBar.open(this.i18n.t('pe.deleted', { name: product.name }), this.i18n.t('common.ok'), { duration: 4000 });
      void this.router.navigateByUrl('/admin/products');
    });
  }

  private reload(): void {
    const product = this.product();
    if (product) {
      this.api.getById(product.id).subscribe((fresh) => this.product.set(fresh));
    }
  }
}
