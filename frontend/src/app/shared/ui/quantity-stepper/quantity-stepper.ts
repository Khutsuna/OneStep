import { Component, input, output } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { I18N } from '../../../core/i18n/translate.pipes';

/**
 * "−  2  +" control. It doesn't change anything itself: it only tells the parent
 * which quantity the user wants (output), and the parent decides what to do.
 */
@Component({
  selector: 'app-quantity-stepper',
  imports: [I18N, MatIconModule],
  template: `
    <button type="button" [disabled]="disabled() || value() <= min()" (click)="changed.emit(value() - 1)"
            [attr.aria-label]="'stepper.decrease' | t: { name: label() }">
      <mat-icon aria-hidden="true">remove</mat-icon>
    </button>
    <span class="value" aria-live="polite">{{ value() }}</span>
    <button type="button" [disabled]="disabled() || value() >= max()" (click)="changed.emit(value() + 1)"
            [attr.aria-label]="'stepper.increase' | t: { name: label() }">
      <mat-icon aria-hidden="true">add</mat-icon>
    </button>
  `,
  styles: `
    :host { display: inline-flex; align-items: center; border: 1px solid var(--os-line); border-radius: var(--os-radius-sm); background: var(--os-surface); }
    button { width: 36px; height: 36px; display: grid; place-items: center; border: 0; background: none; color: var(--os-ink); cursor: pointer; border-radius: var(--os-radius-sm); }
    button:hover:not(:disabled) { background: var(--os-paper); }
    button:disabled { color: #b4bccb; cursor: default; }
    mat-icon { font-size: 18px; width: 18px; height: 18px; }
    .value { min-width: 28px; text-align: center; font-weight: 600; font-variant-numeric: tabular-nums; }
  `,
})
export class QuantityStepper {
  readonly value = input.required<number>();
  readonly min = input(1);
  readonly max = input(99);
  readonly disabled = input(false);
  /** What the item is called, for screen readers ("Increase quantity of AirPods 4"). */
  readonly label = input('item');
  readonly changed = output<number>();
}
