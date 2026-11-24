import { CurrencyPipe } from '@angular/common';
import { Component, computed, input } from '@angular/core';
import { I18N } from '../../../core/i18n/translate.pipes';

/**
 * Shows a price in lari, with the tetri small and raised: ₾879²⁰ (a common shop style, easier to scan).
 * When there is a discount, the old price is crossed out next to it.
 */
@Component({
  selector: 'app-price-tag',
  imports: [I18N, CurrencyPipe],
  template: `
    <span class="price" [class.large]="size() === 'large'" [class.sale]="hasDiscount()">
      <span class="visually-hidden">{{ finalPrice() | currency: 'GEL' : 'symbol-narrow' : '1.2-2' }}</span>
      <span aria-hidden="true">₾{{ parts().whole }}<sup>{{ parts().tetri }}</sup></span>
    </span>
    @if (hasDiscount()) {
      <span class="old">
        <span class="visually-hidden">{{ 'price.was' | t }}</span>
        {{ price() | currency: 'GEL' : 'symbol-narrow' : '1.2-2' }}
      </span>
      @if (showBadge()) {
        <span class="badge">−{{ percentOff() }}%</span>
      }
    }
  `,
  styles: `
    :host { display: inline-flex; align-items: baseline; flex-wrap: wrap; gap: 4px 8px; }
    .price { font-weight: 700; font-size: 1.125rem; font-variant-numeric: tabular-nums; color: var(--os-ink); }
    .price.large { font-size: 2rem; font-weight: 800; letter-spacing: -0.02em; }
    sup { font-size: 0.58em; vertical-align: 0.62em; margin-left: 1px; line-height: 0; }
    .price.sale { color: var(--os-sale); }
    .old { color: var(--os-muted); text-decoration: line-through; font-size: 0.875rem; }
    .badge { background: var(--os-sale); color: #fff; font-size: 0.75rem; font-weight: 700; padding: 1px 6px; border-radius: var(--os-radius-sm); }
  `,
})
export class PriceTag {
  readonly price = input.required<number>();
  readonly finalPrice = input.required<number>();
  readonly size = input<'normal' | 'large'>('normal');
  /** The product card shows its own round badge, so it turns this one off. */
  readonly showBadge = input(true);

  /** 1234.5 -> { whole: '1,234', tetri: '50' }. toFixed(2) rounds to tetri first. */
  protected readonly parts = computed(() => {
    const [whole, tetri] = this.finalPrice().toFixed(2).split('.');
    return { whole: Number(whole).toLocaleString('en-US'), tetri };
  });

  protected readonly hasDiscount = computed(() => this.finalPrice() < this.price());
  protected readonly percentOff = computed(() =>
    Math.round((1 - this.finalPrice() / this.price()) * 100),
  );
}
