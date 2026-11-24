import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Footer } from '../footer/footer';
import { Header } from '../header/header';
import { I18N } from '../../i18n/translate.pipes';

/** The frame around every shop page: header on top, the current page in the middle, footer below. */
@Component({
  selector: 'app-shell',
  imports: [I18N, RouterOutlet, Header, Footer],
  template: `
    <a class="skip-link" href="#main">{{ 'shell.skip' | t }}</a>
    <app-header />
    <main id="main" tabindex="-1">
      <router-outlet />
    </main>
    <app-footer />
  `,
  styles: `
    :host { display: flex; flex-direction: column; min-height: 100vh; }
    main { flex: 1; outline: none; }
    .skip-link { position: absolute; left: -9999px; top: 8px; z-index: 100; background: var(--os-step); color: var(--os-ink); padding: 8px 12px; border-radius: var(--os-radius-sm); font-weight: 600; }
    .skip-link:focus { left: 8px; }
  `,
})
export class Shell {}
