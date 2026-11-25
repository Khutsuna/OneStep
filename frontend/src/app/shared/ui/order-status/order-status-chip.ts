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
    :host(.pending) { background: var(--os-tint-step); color: var(--os-text-warn); }
    :host(.paid) { background: var(--os-tint-ink); color: var(--os-ink); }
    :host(.shipped) { background: var(--os-tint-info); color: var(--os-text-info); }
    :host(.delivered) { background: var(--os-tint-stock); color: var(--os-text-stock); }
    :host(.cancelled) { background: var(--os-tint-neutral); color: var(--os-muted); }
  `,
})
export class OrderStatusChip {
  readonly status = input.required<OrderStatus>();
  protected readonly label = computed(() => statusKey(this.status()));
}
