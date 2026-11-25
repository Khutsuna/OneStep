import { Component } from '@angular/core';

/**
 * Technica's logo mark: a bold "T" on a marigold tile. Inline SVG, so it is sharp at any size
 * and needs no image file. The size comes from the CSS of the place that uses it.
 */
@Component({
  selector: 'app-logo-mark',
  template: `
    <svg viewBox="0 0 32 32" aria-hidden="true" focusable="false">
      <rect width="32" height="32" rx="8" fill="#f5b82e" />
      <path d="M8 8h16v4.5h-5.75V25h-4.5V12.5H8z" fill="#101a33" />
    </svg>
  `,
  styles: `
    :host { display: inline-block; flex: none; }
    svg { display: block; width: 100%; height: 100%; }
  `,
})
export class LogoMark {}
