import { Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { categoryIcon } from '../../../core/catalog/category.store';
import { ProductListItem } from '../../../core/models/catalog.models';
import { PriceTag } from '../price-tag/price-tag';
import { ProductImage } from '../product-image/product-image';
import { RatingStars } from '../rating-stars/rating-stars';
import { WishlistButton } from '../wishlist-button/wishlist-button';
import { I18N } from '../../../core/i18n/translate.pipes';

/**
 * Presentational component: it only receives a product through an input and displays it.
 * It doesn't load data or know about stores, so it can be reused anywhere.
 */
@Component({
  selector: 'app-product-card',
  imports: [I18N, RouterLink, PriceTag, ProductImage, RatingStars, WishlistButton],
  templateUrl: './product-card.html',
  styleUrl: './product-card.scss',
})
export class ProductCard {
  readonly product = input.required<ProductListItem>();

  /** Placeholder icon based on the category, e.g. "Chargers & Cables" -> "chargers-cables" -> cable. */
  protected readonly icon = computed(() =>
    categoryIcon(this.product().categoryName.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '')),
  );
}
