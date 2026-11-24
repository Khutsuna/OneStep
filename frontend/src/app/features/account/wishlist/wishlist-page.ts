import { Component, computed, effect, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink } from '@angular/router';
import { ProductListItem } from '../../../core/models/catalog.models';
import { WishlistApi } from '../../../core/shop/wishlist.api';
import { WishlistStore } from '../../../core/shop/wishlist.store';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { ProductCard } from '../../../shared/ui/product-card/product-card';

/** The saved products. Un-hearting a card removes it from this page straight away. */
@Component({
  selector: 'app-wishlist-page',
  imports: [RouterLink, MatButtonModule, EmptyState, ProductCard],
  template: `
    <div class="page">
      <h1 class="title">Wishlist</h1>
      @if (products(); as list) {
        @if (visible().length) {
          <p class="count">{{ visible().length }} saved {{ visible().length === 1 ? 'product' : 'products' }}</p>
          <div class="product-grid">
            @for (product of visible(); track product.id) {
              <app-product-card [product]="product" />
            }
          </div>
        } @else {
          <app-empty-state icon="favorite" title="Nothing saved yet"
                           message="Tap the heart on any product to keep it here for later.">
            <a matButton="filled" routerLink="/products">Browse products</a>
          </app-empty-state>
        }
      } @else {
        <div class="loading" aria-busy="true" aria-label="Loading wishlist"></div>
      }
    </div>
  `,
  styles: `
    .title { font-size: 1.75rem; font-weight: 800; letter-spacing: -0.03em; }
    .count { color: var(--os-muted); margin: 4px 0 16px; }
    .loading { height: 320px; border-radius: var(--os-radius-lg); background: var(--os-photo); margin-top: 16px; }
  `,
})
export class WishlistPage {
  private readonly api = inject(WishlistApi);
  private readonly store = inject(WishlistStore);

  protected readonly products = signal<ProductListItem[] | null>(null);

  /** Only products that are still in the store, so a removed heart disappears without reloading. */
  protected readonly visible = computed(() => (this.products() ?? []).filter((p) => this.store.idSet().has(p.id)));

  constructor() {
    // Reload when the number of saved products changes (e.g. the store finished loading after login).
    effect(() => {
      this.store.count();
      this.api.getProducts().subscribe((list) => this.products.set(list));
    });
  }
}
