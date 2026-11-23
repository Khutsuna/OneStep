import { Component, computed, input } from '@angular/core';
import { OrderStatus } from '../../../core/models/shop.models';

/** Customer-friendly words for each order status. Used by the chip and the timeline. */
export const ORDER_STATUS_LABELS: Record<OrderStatus, string> = {
  Pending: 'Awaiting payment',
  Paid: 'Paid',
  Shipped: 'On the way',
  Delivered: 'Delivered',
  Cancelled: 'Cancelled',
};

/** Words for the history list: what happened at each step. */
export const ORDER_STEP_LABELS: Record<OrderStatus, string> = {
  Pending: 'Placed',
  Paid: 'Paid',
  Shipped: 'Shipped',
  Delivered: 'Delivered',
  Cancelled: 'Cancelled',
};

/** A small coloured label with an order's status. */
@Component({
  selector: 'app-order-status-chip',
  template: `{{ label() }}`,
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
  protected readonly label = computed(() => ORDER_STATUS_LABELS[this.status()]);
}
