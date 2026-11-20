import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-footer',
  imports: [RouterLink],
  template: `
    <div class="inner">
      <div>
        <a class="name" routerLink="/">OneStep</a>
        <p>Electronics with delivery across Georgia.</p>
      </div>
      <p class="note">IT Step Academy diploma project, 2026</p>
    </div>
  `,
  styles: `
    :host { display: block; background: var(--os-ink); color: #c9d1e3; margin-top: 32px; }
    .inner { max-width: var(--os-page-width); margin: 0 auto; padding: 28px var(--os-gutter); display: flex; flex-wrap: wrap; justify-content: space-between; gap: 12px; align-items: flex-end; }
    .name { color: #fff; font-weight: 800; font-size: 1.125rem; text-decoration: none; letter-spacing: -0.02em; }
    p { margin: 4px 0 0; }
    .note { font-size: 0.8125rem; }
  `,
})
export class Footer {}
