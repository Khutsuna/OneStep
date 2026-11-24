import { CurrencyPipe } from '@angular/common';
import { Component, input } from '@angular/core';
import { OrderTotals } from '../../../core/models/shop.models';
import { I18N } from '../../../core/i18n/translate.pipes';

/** The money summary of an order: subtotal, discount, delivery, VAT and total. */
@Component({
  selector: 'app-totals-table',
  imports: [I18N, CurrencyPipe],
  template: `
    <dl>
      <div><dt>{{ 'totals.items' | t }}</dt><dd>{{ totals().subtotal | currency: 'GEL' : 'symbol-narrow' }}</dd></div>
      @if (totals().discountTotal > 0) {
        <div class="discount"><dt>{{ 'totals.discounts' | t }}</dt><dd>−{{ totals().discountTotal | currency: 'GEL' : 'symbol-narrow' }}</dd></div>
      }
      <div>
        <dt>{{ 'totals.delivery' | t }}</dt>
        <dd>{{ totals().shippingCost === 0 ? ('totals.free' | t) : (totals().shippingCost | currency: 'GEL' : 'symbol-narrow') }}</dd>
      </div>
      <div><dt>{{ 'totals.vat' | t }}</dt><dd>{{ totals().taxAmount | currency: 'GEL' : 'symbol-narrow' }}</dd></div>
      <div class="total"><dt>{{ 'totals.total' | t }}</dt><dd>{{ totals().totalAmount | currency: 'GEL' : 'symbol-narrow' }}</dd></div>
    </dl>
  `,
  styles: `
    dl { margin: 0; }
    div { display: flex; justify-content: space-between; gap: 12px; padding: 5px 0; }
    dt { color: var(--os-muted); }
    dd { margin: 0; font-variant-numeric: tabular-nums; font-weight: 500; }
    .discount dd { color: var(--os-sale); }
    .total { border-top: 1px solid var(--os-line); margin-top: 6px; padding-top: 12px; }
    .total dt, .total dd { color: var(--os-ink); font-weight: 800; font-size: 1.25rem; }
  `,
})
export class TotalsTable {
  readonly totals = input.required<OrderTotals>();
}
