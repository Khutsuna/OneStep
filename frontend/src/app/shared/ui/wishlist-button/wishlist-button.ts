import { Component, computed, inject, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { Router } from '@angular/router';
import { AuthStore } from '../../../core/auth/auth.store';
import { WishlistStore } from '../../../core/shop/wishlist.store';

/**
 * The heart button. It reads and changes the global WishlistStore itself, so any page can drop it in
 * with just a product id. Guests are sent to log in; staff don't see it (they can't shop).
 */
@Component({
  selector: 'app-wishlist-button',
  imports: [MatIconModule],
  template: `
    @if (!auth.isStaff()) {
      <button
        type="button"
        [class.saved]="saved()"
        [class.labelled]="showLabel()"
        [attr.aria-pressed]="saved()"
        [attr.aria-label]="showLabel() ? null : (saved() ? 'Remove from wishlist' : 'Save to wishlist')"
        (click)="toggle($event)"
      >
        <mat-icon aria-hidden="true" [class.icon-filled]="saved()">favorite</mat-icon>
        @if (showLabel()) {
          <span>{{ saved() ? 'Saved' : 'Save' }}</span>
        }
      </button>
    }
  `,
  styles: `
    button { display: inline-flex; align-items: center; justify-content: center; gap: 6px; width: 40px; height: 40px;
             border: 1px solid var(--os-line); border-radius: 50%; background: var(--os-surface); color: var(--os-ink);
             cursor: pointer; font: inherit; font-weight: 600; }
    button:hover { border-color: var(--os-ink); }
    button.saved mat-icon { color: var(--os-sale); }
    button.labelled { width: auto; height: 48px; padding: 0 16px; border-radius: var(--os-radius-sm); }
    mat-icon { font-size: 22px; width: 22px; height: 22px; }
  `,
})
export class WishlistButton {
  readonly productId = input.required<number>();
  /** true = "♥ Save" button (product page); false = round icon (product cards). */
  readonly showLabel = input(false);

  protected readonly auth = inject(AuthStore);
  private readonly wishlist = inject(WishlistStore);
  private readonly router = inject(Router);

  protected readonly saved = computed(() => this.wishlist.idSet().has(this.productId()));

  protected toggle(event: MouseEvent): void {
    // The button sits on a product card that is itself a link: don't open the product.
    event.preventDefault();
    event.stopPropagation();

    if (!this.auth.isLoggedIn()) {
      void this.router.navigate(['/login'], { queryParams: { returnUrl: this.router.url } });
      return;
    }
    this.wishlist.toggle(this.productId());
  }
}
