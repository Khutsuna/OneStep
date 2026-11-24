import { Component, computed, effect, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink } from '@angular/router';
import { ProductListItem } from '../../../core/models/catalog.models';
import { WishlistApi } from '../../../core/shop/wishlist.api';
import { WishlistStore } from '../../../core/shop/wishlist.store';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { ProductCard } from '../../../shared/ui/product-card/product-card';
import { I18N } from '../../../core/i18n/translate.pipes';

/** The saved products. Un-hearting a card removes it from this page straight away. */
@Component({
  selector: 'app-wishlist-page',
  imports: [I18N, RouterLink, MatButtonModule, EmptyState, ProductCard],
  template: `
    <div class="page">
      <h1 class="title">{{ 'wishlist.title' | t }}</h1>
      @if (products(); as list) {
        @if (visible().length) {
          <p class="count">{{ 'wishlist.count' | t: { count: visible().length } }}</p>
          <div class="product-grid">
            @for (product of visible(); track product.id) {
              <app-product-card [product]="product" />
            }
          </div>
        } @else {
          <app-empty-state icon="favorite" [title]="'wishlist.empty' | t"
                           [message]="'wishlist.emptyHint' | t">
            <a matButton="filled" routerLink="/products">{{ 'cart.browse' | t }}</a>
          </app-empty-state>
        }
      } @else {
        <div class="loading" aria-busy="true" [attr.aria-label]="'wishlist.loading' | t"></div>
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
