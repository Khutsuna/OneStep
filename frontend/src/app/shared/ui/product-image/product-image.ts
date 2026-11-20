import { Component, computed, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { mediaUrl } from '../../../core/util/media';

/** A product photo on a light background, or a calm placeholder when there is no photo yet. */
@Component({
  selector: 'app-product-image',
  imports: [MatIconModule],
  template: `
    @if (src()) {
      <img [src]="src()" [alt]="alt()" loading="lazy" />
    } @else {
      <div class="placeholder" role="img" [attr.aria-label]="'No photo of ' + alt() + ' yet'">
        <mat-icon aria-hidden="true">{{ icon() }}</mat-icon>
      </div>
    }
  `,
  styles: `
    :host { display: block; aspect-ratio: 1 / 1; background: var(--os-photo); border-radius: var(--os-radius-sm); overflow: hidden; }
    img { width: 100%; height: 100%; object-fit: contain; padding: 8%; }
    .placeholder { width: 100%; height: 100%; display: grid; place-items: center; color: #b4bccb; }
    .placeholder mat-icon { font-size: 56px; width: 56px; height: 56px; }
  `,
})
export class ProductImage {
  readonly url = input<string | null>(null);
  readonly alt = input('');
  readonly icon = input('devices');

  protected readonly src = computed(() => mediaUrl(this.url()));
}
