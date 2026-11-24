import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Title } from '@angular/platform-browser';
import { Router, RouterLink } from '@angular/router';
import { catchError, map, of, startWith, switchMap } from 'rxjs';
import { AuthStore } from '../../../core/auth/auth.store';
import { CategoryStore, categoryIcon } from '../../../core/catalog/category.store';
import { ProductsApi } from '../../../core/catalog/products.api';
import { ProductDetails, ProductListItem } from '../../../core/models/catalog.models';
import { CartStore } from '../../../core/shop/cart.store';
import { mediaUrl } from '../../../core/util/media';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { PriceTag } from '../../../shared/ui/price-tag/price-tag';
import { ProductCard } from '../../../shared/ui/product-card/product-card';
import { ProductImage } from '../../../shared/ui/product-image/product-image';
import { QuantityStepper } from '../../../shared/ui/quantity-stepper/quantity-stepper';
import { WishlistButton } from '../../../shared/ui/wishlist-button/wishlist-button';
import { ProductReviewsSection } from './reviews/product-reviews';
import { RatingStars } from '../../../shared/ui/rating-stars/rating-stars';

interface DetailState {
  product: ProductDetails | null;
  loading: boolean;
  notFound: boolean;
}

@Component({
  selector: 'app-product-detail-page',
  imports: [
    DatePipe, RouterLink, MatButtonModule, MatIconModule,
    EmptyState, PriceTag, ProductCard, ProductImage, ProductReviewsSection, QuantityStepper, RatingStars, WishlistButton,
  ],
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
  protected readonly auth = inject(AuthStore);
  private readonly cart = inject(CartStore);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

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
  protected readonly quantity = signal(1);
  protected readonly adding = signal(false);

  /** How many of this product are already in the cart. */
  protected readonly inCart = computed(
    () => this.cart.items().find((item) => item.productId === Number(this.id()))?.quantity ?? 0,
  );

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
      this.quantity.set(1);
    });
  }

  protected addToCart(product: ProductDetails): void {
    // Guests log in first and come back to this product.
    if (!this.auth.isLoggedIn()) {
      void this.router.navigate(['/login'], { queryParams: { returnUrl: this.router.url } });
      return;
    }

    this.adding.set(true);
    this.cart.add(product.id, this.quantity()).subscribe({
      next: () => {
        this.adding.set(false);
        this.snackBar
          .open(`${product.name} is in your cart.`, 'View cart', { duration: 4000 })
          .onAction()
          .subscribe(() => void this.router.navigateByUrl('/cart'));
      },
      error: (error: HttpErrorResponse) => {
        this.adding.set(false);
        // 409 = not enough stock; the API says how many are left.
        if (error.status === 409 || error.status === 400) {
          this.snackBar.open(error.error?.detail ?? "We couldn't add this product.", 'OK', { duration: 5000 });
        }
      },
    });
  }
}
