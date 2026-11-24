import { Component, computed, input } from '@angular/core';
import { OrderStatus } from '../../../core/models/shop.models';
import { I18N } from '../../../core/i18n/translate.pipes';
import { TranslationKey } from '../../../core/i18n/en';

/** Translation key of each status's customer-friendly name ("On the way"), e.g. statusKey('Shipped'). */
export const statusKey = (status: OrderStatus): TranslationKey => `status.${status}`;

/** Translation key of what happened at each step ("Shipped"), for the history list and staff buttons. */
export const stepKey = (status: OrderStatus): TranslationKey => `step.${status}`;

/** A small coloured label with an order's status. */
@Component({
  selector: 'app-order-status-chip',
  imports: [I18N],
  template: `{{ label() | t }}`,
  host: { '[class]': 'status().toLowerCase()' },
  styles: `
    :host { display: inline-block; padding: 2px 10px; border-radius: 999px; font-size: 0.8125rem; font-weight: 600; white-space: nowrap; }
    :host(.pending) { background: #fff4d6; color: #7a5300; }
    :host(.paid) { background: #e3e8f3; color: var(--os-ink); }
    :host(.shipped) { background: #dceefe; color: #0b4f8a; }
    :host(.delivered) { background: #dcf3e9; color: #0b5e43; }
    :host(.cancelled) { background: #eceff3; color: var(--os-muted); }
  `,
})
export class OrderStatusChip {
  readonly status = input.required<OrderStatus>();
  protected readonly label = computed(() => statusKey(this.status()));
}
