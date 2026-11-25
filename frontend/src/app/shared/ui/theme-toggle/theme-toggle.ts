import { Component, inject } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { ThemeStore } from '../../../core/theme/theme.store';
import { I18N } from '../../../core/i18n/translate.pipes';

/** Moon / sun button for the navy top bars (shop header and admin panel). */
@Component({
  selector: 'app-theme-toggle',
  imports: [I18N, MatIconModule],
  template: `
    <button type="button" (click)="theme.toggle()" [attr.aria-pressed]="theme.theme() === 'dark'"
            [attr.aria-label]="'theme.night' | t" [title]="(theme.theme() === 'dark' ? 'theme.toDay' : 'theme.toNight') | t">
      <mat-icon aria-hidden="true">{{ theme.theme() === 'dark' ? 'light_mode' : 'dark_mode' }}</mat-icon>
    </button>
  `,
  styles: `
    :host { display: inline-flex; margin-right: 4px; }
    button {
      display: grid; place-items: center; width: 30px; height: 30px; padding: 0; cursor: pointer;
      border: 1px solid rgb(255 255 255 / 0.3); border-radius: var(--os-radius-sm);
      background: none; color: #c3cbdd;
    }
    button:hover { color: #fff; border-color: rgb(255 255 255 / 0.6); }
    button[aria-pressed='true'] { color: var(--os-step); }
    mat-icon { font-size: 18px; width: 18px; height: 18px; }
  `,
})
export class ThemeToggle {
  protected readonly theme = inject(ThemeStore);
}
