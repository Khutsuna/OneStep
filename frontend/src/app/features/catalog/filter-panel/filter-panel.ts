import { Component, computed, input, linkedSignal, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { CategoryNode, ProductFilters, ProductQuery } from '../../../core/models/catalog.models';

/**
 * Presentational component: gets the current query and the available options as inputs,
 * and reports what the user changed through the `changed` output.
 * The page decides what to do with it (it updates the URL).
 */
@Component({
  selector: 'app-filter-panel',
  imports: [FormsModule, MatButtonModule, MatCheckboxModule, MatIconModule, MatSlideToggleModule],
  templateUrl: './filter-panel.html',
  styleUrl: './filter-panel.scss',
})
export class FilterPanel {
  readonly query = input.required<ProductQuery>();
  readonly filters = input<ProductFilters | null>(null);
  readonly categories = input<CategoryNode[]>([]);

  readonly changed = output<Partial<ProductQuery>>();
  readonly closed = output<void>();

  // Local copies of the price inputs; they reset whenever the query changes.
  protected readonly minPrice = linkedSignal(() => this.query().minPrice);
  protected readonly maxPrice = linkedSignal(() => this.query().maxPrice);

  protected readonly hasActiveFilters = computed(() => {
    const q = this.query();
    return q.brandIds.length > 0 || q.attributes.length > 0 || q.minPrice !== null || q.maxPrice !== null || q.inStock;
  });

  protected selectCategory(categoryId: number | null): void {
    // Brands and attributes differ per category, so a new category starts with a clean filter set.
    this.changed.emit({ categoryId, brandIds: [], attributes: [] });
  }

  protected toggleBrand(brandId: number, checked: boolean): void {
    const current = this.query().brandIds;
    const brandIds = checked ? [...current, brandId] : current.filter((id) => id !== brandId);
    this.changed.emit({ brandIds });
  }

  protected isAttributeSelected(name: string, value: string): boolean {
    return this.query().attributes.includes(`${name}:${value}`);
  }

  protected toggleAttribute(name: string, value: string): void {
    const key = `${name}:${value}`;
    const current = this.query().attributes;
    const attributes = current.includes(key) ? current.filter((a) => a !== key) : [...current, key];
    this.changed.emit({ attributes });
  }

  protected applyPrice(): void {
    this.changed.emit({ minPrice: this.toPrice(this.minPrice()), maxPrice: this.toPrice(this.maxPrice()) });
  }

  protected clearAll(): void {
    this.changed.emit({ brandIds: [], attributes: [], minPrice: null, maxPrice: null, inStock: false });
  }

  private toPrice(value: number | null | string): number | null {
    if (value === null || value === '') {
      return null;
    }
    const number = Number(value);
    return Number.isFinite(number) && number >= 0 ? number : null;
  }
}
