import { CurrencyPipe } from '@angular/common';
import { Component, computed, input } from '@angular/core';

/** Shows a price in lari. When there is a discount, the old price is crossed out next to it. */
@Component({
  selector: 'app-price-tag',
  imports: [CurrencyPipe],
  template: `
    <span class="price" [class.large]="size() === 'large'" [class.sale]="hasDiscount()">
      {{ finalPrice() | currency: 'GEL' : 'symbol-narrow' : '1.2-2' }}
    </span>
    @if (hasDiscount()) {
      <span class="old">
        <span class="visually-hidden">Was</span>
        {{ price() | currency: 'GEL' : 'symbol-narrow' : '1.2-2' }}
      </span>
      <span class="badge">−{{ percentOff() }}%</span>
    }
  `,
  styles: `
    :host { display: inline-flex; align-items: baseline; flex-wrap: wrap; gap: 4px 8px; }
    .price { font-weight: 700; font-size: 1.125rem; font-variant-numeric: tabular-nums; color: var(--os-ink); }
    .price.large { font-size: 2rem; font-weight: 800; letter-spacing: -0.02em; }
    .price.sale { color: var(--os-sale); }
    .old { color: var(--os-muted); text-decoration: line-through; font-size: 0.875rem; }
    .badge { background: var(--os-sale); color: #fff; font-size: 0.75rem; font-weight: 700; padding: 1px 6px; border-radius: var(--os-radius-sm); }
  `,
})
export class PriceTag {
  readonly price = input.required<number>();
  readonly finalPrice = input.required<number>();
  readonly size = input<'normal' | 'large'>('normal');

  protected readonly hasDiscount = computed(() => this.finalPrice() < this.price());
  protected readonly percentOff = computed(() =>
    Math.round((1 - this.finalPrice() / this.price()) * 100),
  );
}
