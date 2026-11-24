import { Component, computed, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { I18N } from '../../../core/i18n/translate.pipes';

/** Five stars (full, half or empty) and the number of reviews. */
@Component({
  selector: 'app-rating-stars',
  imports: [I18N, MatIconModule],
  template: `
    <span class="stars" [attr.aria-label]="'rating.label' | t: { rating: rating().toFixed(1) }" role="img">
      @for (star of stars(); track $index) {
        <mat-icon [class.icon-filled]="star !== 'star_outline'" aria-hidden="true">{{
          star === 'star_outline' ? 'star' : star
        }}</mat-icon>
      }
    </span>
    @if (showCount()) {
      <span class="count">{{ count() > 0 ? '(' + count() + ')' : ('rating.none' | t) }}</span>
    }
  `,
  styles: `
    :host { display: inline-flex; align-items: center; gap: 6px; }
    .stars { display: inline-flex; color: var(--os-step); }
    mat-icon { font-size: 18px; width: 18px; height: 18px; }
    .count { color: var(--os-muted); font-size: 0.8125rem; }
  `,
})
export class RatingStars {
  readonly rating = input.required<number>();
  readonly count = input(0);
  readonly showCount = input(true);

  protected readonly stars = computed(() => {
    const rating = this.rating();
    return [1, 2, 3, 4, 5].map((position) =>
      rating >= position ? 'star' : rating >= position - 0.5 ? 'star_half' : 'star_outline',
    );
  });

}
