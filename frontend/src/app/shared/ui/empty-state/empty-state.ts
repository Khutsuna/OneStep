import { Component, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

/** A friendly message for empty lists and missing pages, with room for an action button. */
@Component({
  selector: 'app-empty-state',
  imports: [MatIconModule],
  template: `
    <mat-icon aria-hidden="true">{{ icon() }}</mat-icon>
    <h2>{{ title() }}</h2>
    @if (message()) {
      <p>{{ message() }}</p>
    }
    <ng-content />
  `,
  styles: `
    :host { display: flex; flex-direction: column; align-items: center; text-align: center; gap: 8px; padding: 48px 16px; }
    mat-icon { font-size: 48px; width: 48px; height: 48px; color: var(--os-muted); }
    h2 { font-size: 1.25rem; }
    p { color: var(--os-muted); margin: 0 0 8px; max-width: 42ch; }
  `,
})
export class EmptyState {
  readonly icon = input('search_off');
  readonly title = input.required<string>();
  readonly message = input<string | null>(null);
}
