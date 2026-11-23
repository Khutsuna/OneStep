import { DatePipe } from '@angular/common';
import { Component, computed, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { OrderDetails, OrderStatus } from '../../../core/models/shop.models';

const STEPS: { status: OrderStatus; label: string }[] = [
  { status: 'Pending', label: 'Placed' },
  { status: 'Paid', label: 'Paid' },
  { status: 'Shipped', label: 'Shipped' },
  { status: 'Delivered', label: 'Delivered' },
];

/** The four steps of an order (Placed → Paid → Shipped → Delivered), with the date each one happened. */
@Component({
  selector: 'app-order-tracker',
  imports: [DatePipe, MatIconModule],
  template: `
    <ol>
      @for (step of steps(); track step.status) {
        <li [class.done]="step.done" [class.current]="step.current" [attr.aria-current]="step.current ? 'step' : null">
          <span class="dot" aria-hidden="true">
            @if (step.done) {
              <mat-icon>check</mat-icon>
            }
          </span>
          <span class="label">{{ step.label }}</span>
          <span class="when">{{ step.date ? (step.date | date: 'd MMM, HH:mm') : '' }}</span>
        </li>
      }
    </ol>
  `,
  styles: `
    ol { list-style: none; margin: 0; padding: 0; display: grid; grid-template-columns: repeat(4, 1fr); }
    li { position: relative; display: flex; flex-direction: column; align-items: center; text-align: center; gap: 4px; }
    /* The line between two steps, drawn from the previous dot to this one. */
    li + li::before { content: ''; position: absolute; top: 13px; right: 50%; width: 100%; height: 3px; background: var(--os-line); z-index: 0; }
    li.done + li.done::before { background: var(--os-ink); }
    .dot { position: relative; z-index: 1; width: 28px; height: 28px; border-radius: 50%; display: grid; place-items: center; background: var(--os-surface); border: 3px solid var(--os-line); }
    li.done .dot { background: var(--os-ink); border-color: var(--os-ink); color: #fff; }
    li.current .dot { box-shadow: 0 0 0 4px var(--os-step); }
    .dot mat-icon { font-size: 16px; width: 16px; height: 16px; }
    .label { font-weight: 600; font-size: 0.875rem; }
    li:not(.done) .label { color: var(--os-muted); }
    .when { color: var(--os-muted); font-size: 0.75rem; min-height: 1.2em; }
  `,
})
export class OrderTracker {
  readonly order = input.required<OrderDetails>();

  protected readonly steps = computed(() => {
    const order = this.order();
    const reached = STEPS.findIndex((s) => s.status === order.status);
    return STEPS.map((step, index) => ({
      ...step,
      done: index <= reached,
      current: index === reached,
      date: order.history.find((h) => h.toStatus === step.status)?.changedAt ?? null,
    }));
  });
}
