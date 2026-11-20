import { DatePipe } from '@angular/common';
import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { MatIconModule } from '@angular/material/icon';
import { Title } from '@angular/platform-browser';
import { RouterLink } from '@angular/router';
import { catchError, map, of, startWith, switchMap } from 'rxjs';
import { CategoryStore, categoryIcon } from '../../../core/catalog/category.store';
import { ProductsApi } from '../../../core/catalog/products.api';
import { ProductDetails, ProductListItem } from '../../../core/models/catalog.models';
import { mediaUrl } from '../../../core/util/media';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { PriceTag } from '../../../shared/ui/price-tag/price-tag';
import { ProductCard } from '../../../shared/ui/product-card/product-card';
import { ProductImage } from '../../../shared/ui/product-image/product-image';
import { RatingStars } from '../../../shared/ui/rating-stars/rating-stars';

interface DetailState {
  product: ProductDetails | null;
  loading: boolean;
  notFound: boolean;
}

@Component({
  selector: 'app-product-detail-page',
  imports: [DatePipe, RouterLink, MatIconModule, EmptyState, PriceTag, ProductCard, ProductImage, RatingStars],
  templateUrl: './product-detail-page.html',
  styleUrl: './product-detail-page.scss',
})
export class ProductDetailPage {
  /** The ":id" from the URL /products/:id, passed in by the router (withComponentInputBinding). */
  readonly id = input.required<string>();

  private readonly api = inject(ProductsApi);
  private readonly categories = inject(CategoryStore);
  private readonly titleService = inject(Title);
  protected readonly mediaUrl = mediaUrl;

  /** Reloads whenever the id changes, e.g. when you click a similar product. */
  private readonly state = toSignal(
    toObservable(this.id).pipe(
      switchMap((id) =>
        this.api.getById(Number(id)).pipe(
          map((product): DetailState => ({ product, loading: false, notFound: false })),
          startWith<DetailState>({ product: null, loading: true, notFound: false }),
          catchError(() => of<DetailState>({ product: null, loading: false, notFound: true })),
        ),
      ),
    ),
    { initialValue: { product: null, loading: true, notFound: false } },
  );

  protected readonly product = computed(() => this.state().product);
  protected readonly loading = computed(() => this.state().loading);
  protected readonly notFound = computed(() => this.state().notFound);

  protected readonly similar = toSignal(
    toObservable(this.id).pipe(
      switchMap((id) => this.api.getSimilar(Number(id)).pipe(catchError(() => of([] as ProductListItem[])))),
    ),
    { initialValue: [] as ProductListItem[] },
  );

  protected readonly selectedImage = signal(0);

  protected readonly breadcrumb = computed(() => {
    const product = this.product();
    return product ? this.categories.pathTo(product.categoryId) : [];
  });

  protected readonly icon = computed(() => {
    const path = this.breadcrumb();
    return path.length ? categoryIcon(path[path.length - 1].slug) : 'devices';
  });

  protected readonly stockText = computed(() => {
    const product = this.product();
    if (!product) return '';
    if (!product.inStock) return 'Out of stock';
    if (product.stockQuantity <= product.lowStockThreshold) return `Only ${product.stockQuantity} left`;
    return 'In stock';
  });

  constructor() {
    // Keep the browser tab title in sync, and show the first photo of each new product.
    effect(() => {
      const product = this.product();
      this.titleService.setTitle(product ? `${product.name} — OneStep` : 'OneStep');
    });
    effect(() => {
      this.id();
      this.selectedImage.set(0);
    });
  }
}
