import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthStore } from '../../auth/auth.store';
import { CategoryStore } from '../../catalog/category.store';
import { I18N } from '../../i18n/translate.pipes';

@Component({
  selector: 'app-footer',
  imports: [I18N, RouterLink],
  template: `
    <div class="inner">
      <div class="brand">
        <a class="logo" routerLink="/">
          <span class="mark" aria-hidden="true"><span></span><span></span></span>
          OneStep
        </a>
        <p>{{ 'footer.tagline' | t }}</p>
      </div>

      <nav [attr.aria-label]="'footer.shop' | t">
        <h2>{{ 'footer.shop' | t }}</h2>
        <a routerLink="/products">{{ 'header.allProducts' | t }}</a>
        @for (category of categories.tree(); track category.id) {
          <a routerLink="/products" [queryParams]="{ categoryId: category.id }">{{ category.name | term }}</a>
        }
      </nav>

      @if (!auth.isStaff()) {
        <nav [attr.aria-label]="'footer.account' | t">
          <h2>{{ 'footer.account' | t }}</h2>
          <a routerLink="/account/orders">{{ 'header.myOrders' | t }}</a>
          <a routerLink="/account/wishlist">{{ 'header.wishlist' | t }}</a>
          <a routerLink="/account">{{ 'header.myAccount' | t }}</a>
        </nav>
      }

      <div>
        <h2>{{ 'footer.delivery' | t }}</h2>
        <p>{{ 'footer.deliveryStandard' | t }}</p>
        <p>{{ 'footer.deliveryExpress' | t }}</p>
        <p>{{ 'footer.deliveryPickup' | t }}</p>
        <p>{{ 'home.perkDelivery' | t }}</p>
      </div>
    </div>
    <p class="note">{{ 'footer.note' | t }}</p>
  `,
  styles: `
    :host { display: block; background: var(--os-ink); color: #aeb8cf; margin-top: 48px; }
    .inner { max-width: var(--os-page-width); margin: 0 auto; padding: 44px var(--os-gutter) 28px;
             display: grid; gap: 28px; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); }
    .brand { min-width: 220px; }
    .logo { display: inline-flex; align-items: flex-end; gap: 8px; color: #fff; text-decoration: none;
            font-family: var(--os-display); font-stretch: 115%; font-weight: 800; font-size: 1.75rem; letter-spacing: -0.03em; line-height: 1; }
    .mark { display: inline-grid; grid-template-columns: 10px 10px; align-items: end; gap: 3px; height: 26px; }
    .mark span { background: var(--os-step); border-radius: 2px; }
    .mark span:first-child { height: 13px; }
    .mark span:last-child { height: 26px; }
    h2 { color: #fff; font-size: 0.9375rem; font-weight: 700; margin-bottom: 10px; }
    nav { display: flex; flex-direction: column; gap: 6px; }
    a { color: #c3cbdd; text-decoration: none; }
    nav a:hover { color: var(--os-step); }
    p { margin: 10px 0 0; }
    div > p:first-of-type { margin-top: 0; }
    .brand p { margin-top: 12px; max-width: 30ch; }
    .note { max-width: var(--os-page-width); margin: 0 auto; padding: 16px var(--os-gutter) 24px; font-size: 0.8125rem;
            border-top: 1px solid #22335a; }
  `,
})
export class Footer {
  protected readonly categories = inject(CategoryStore);
  protected readonly auth = inject(AuthStore);
}
