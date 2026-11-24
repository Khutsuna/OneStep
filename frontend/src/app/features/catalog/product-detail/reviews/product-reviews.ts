import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar } from '@angular/material/snack-bar';
import { RouterLink } from '@angular/router';
import { AuthStore } from '../../../../core/auth/auth.store';
import { ProductReviews, Review } from '../../../../core/models/account.models';
import { ReviewsApi } from '../../../../core/shop/reviews.api';
import { RatingStars } from '../../../../shared/ui/rating-stars/rating-stars';
import { I18N } from '../../../../core/i18n/translate.pipes';
import { TranslationKey } from '../../../../core/i18n/en';
import { LanguageStore } from '../../../../core/i18n/language.store';

/**
 * The reviews section of the product page: rating summary, the reviews, and the form.
 * Whether the form appears comes from the API (canReview), which uses the same rule as when saving.
 */
@Component({
  selector: 'app-product-reviews',
  imports: [I18N, DecimalPipe, ReactiveFormsModule, RouterLink,
    MatButtonModule, MatFormFieldModule, MatIconModule, MatInputModule, RatingStars,
  ],
  templateUrl: './product-reviews.html',
  styleUrl: './product-reviews.scss',
})
export class ProductReviewsSection {
  readonly productId = input.required<number>();

  protected readonly auth = inject(AuthStore);
  private readonly api = inject(ReviewsApi);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly data = signal<ProductReviews | null>(null);
  /** Reviews shown so far ("Show more" adds the next page). */
  protected readonly reviews = signal<Review[]>([]);
  protected readonly page = signal(1);
  protected readonly saving = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly hasMore = computed(() => {
    const d = this.data();
    return !!d && this.reviews().length < d.reviewCount;
  });

  protected readonly form = inject(FormBuilder).nonNullable.group({
    rating: [0, [Validators.min(1), Validators.max(5)]],
    title: ['', Validators.maxLength(150)],
    comment: ['', Validators.maxLength(2000)],
  });

  /** The star the mouse is over, to preview the rating before clicking. */
  protected readonly hoverRating = signal(0);
  private readonly i18n = inject(LanguageStore);

  /** "Poor" ... "Excellent" for the hovered or chosen number of stars. */
  protected ratingWord(): string {
    const stars = this.hoverRating() || this.form.controls.rating.value;
    return stars ? this.i18n.t(`reviews.word${stars}` as TranslationKey) : '';
  }

  constructor() {
    // Load page 1 whenever the product changes (e.g. clicking a "you might also like" product).
    effect(() => {
      const id = this.productId();
      this.load(id, 1);
    });
  }

  private load(productId: number, page: number): void {
    this.api.getForProduct(productId, page).subscribe((data) => {
      this.data.set(data);
      this.page.set(page);
      this.reviews.update((current) => (page === 1 ? data.reviews.items : [...current, ...data.reviews.items]));
    });
  }

  protected showMore(): void {
    this.load(this.productId(), this.page() + 1);
  }

  protected setRating(stars: number): void {
    this.form.controls.rating.setValue(stars);
  }

  protected submit(): void {
    if (this.form.controls.rating.value < 1) {
      this.errorMessage.set(this.i18n.t('reviews.chooseStars'));
      return;
    }
    if (this.form.invalid) return;

    const value = this.form.getRawValue();
    this.saving.set(true);
    this.errorMessage.set(null);

    this.api
      .create(this.productId(), {
        rating: value.rating,
        title: value.title.trim() || null,
        comment: value.comment.trim() || null,
      })
      .subscribe({
        next: () => {
          this.saving.set(false);
          this.form.reset({ rating: 0, title: '', comment: '' });
          this.snackBar.open(this.i18n.t('reviews.thanks'), this.i18n.t('common.ok'), { duration: 4000 });
          this.load(this.productId(), 1); // new average, and the form disappears (already reviewed)
        },
        error: (error: HttpErrorResponse) => {
          this.saving.set(false);
          this.errorMessage.set(error.error?.detail ?? error.error?.title ?? this.i18n.t('reviews.notSaved'));
        },
      });
  }

  protected remove(review: Review): void {
    this.api.delete(review.id).subscribe(() => {
      this.snackBar.open(this.i18n.t('reviews.deleted'), this.i18n.t('common.ok'), { duration: 3000 });
      this.load(this.productId(), 1);
    });
  }

  /** Width of a rating bar, as a share of all reviews. */
  protected share(count: number): number {
    const total = this.data()?.reviewCount ?? 0;
    return total === 0 ? 0 : Math.round((count / total) * 100);
  }
}
