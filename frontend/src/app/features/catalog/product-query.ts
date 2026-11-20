import { ParamMap, Params } from '@angular/router';
import { ProductQuery, ProductSort } from '../../core/models/catalog.models';

/**
 * The product list keeps all its filters in the URL (?categoryId=7&sort=PriceAsc...).
 * That way a filtered page survives a refresh, the Back button works and links can be shared.
 * These two functions convert between the URL and a ProductQuery object.
 */

export const DEFAULT_PAGE_SIZE = 12;
const SORTS: ProductSort[] = ['Newest', 'PriceAsc', 'PriceDesc', 'Rating', 'Name'];

export const EMPTY_QUERY: ProductQuery = {
  search: null,
  categoryId: null,
  brandIds: [],
  minPrice: null,
  maxPrice: null,
  attributes: [],
  inStock: false,
  sort: 'Newest',
  page: 1,
  pageSize: DEFAULT_PAGE_SIZE,
};

export function parseProductQuery(params: ParamMap): ProductQuery {
  const sort = params.get('sort') as ProductSort | null;
  return {
    search: params.get('search') || null,
    categoryId: toNumber(params.get('categoryId')),
    brandIds: params.getAll('brand').map(Number).filter((id) => id > 0),
    minPrice: toNumber(params.get('minPrice')),
    maxPrice: toNumber(params.get('maxPrice')),
    attributes: params.getAll('attr'),
    inStock: params.get('inStock') === 'true',
    sort: sort && SORTS.includes(sort) ? sort : 'Newest',
    page: Math.max(1, toNumber(params.get('page')) ?? 1),
    pageSize: toNumber(params.get('pageSize')) ?? DEFAULT_PAGE_SIZE,
  };
}

/** Only non-default values go into the URL, so links stay short. */
export function toQueryParams(query: ProductQuery): Params {
  return {
    search: query.search || null,
    categoryId: query.categoryId,
    brand: query.brandIds.length ? query.brandIds : null,
    minPrice: query.minPrice,
    maxPrice: query.maxPrice,
    attr: query.attributes.length ? query.attributes : null,
    inStock: query.inStock || null,
    sort: query.sort === 'Newest' ? null : query.sort,
    page: query.page > 1 ? query.page : null,
    pageSize: query.pageSize === DEFAULT_PAGE_SIZE ? null : query.pageSize,
  };
}

function toNumber(value: string | null): number | null {
  if (value === null || value.trim() === '') {
    return null;
  }
  const number = Number(value);
  return Number.isFinite(number) ? number : null;
}
