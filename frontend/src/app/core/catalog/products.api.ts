import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/common.models';
import { PricePoint, ProductDetails, ProductDocument, ProductFilters, ProductListItem, ProductQuery } from '../models/catalog.models';

@Injectable({ providedIn: 'root' })
export class ProductsApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/products`;

  search(query: ProductQuery): Observable<PagedResult<ProductListItem>> {
    return this.http.get<PagedResult<ProductListItem>>(this.baseUrl, { params: toHttpParams(query) });
  }

  getById(id: number): Observable<ProductDetails> {
    return this.http.get<ProductDetails>(`${this.baseUrl}/${id}`);
  }

  getFilters(categoryId: number | null): Observable<ProductFilters> {
    const params = categoryId ? new HttpParams().set('categoryId', categoryId) : undefined;
    return this.http.get<ProductFilters>(`${this.baseUrl}/filters`, { params });
  }

  /** The base price over time (written by the SQL trigger trg_Products_PriceHistory). */
  getPriceHistory(id: number): Observable<PricePoint[]> {
    return this.http.get<PricePoint[]>(`${this.baseUrl}/${id}/price-history`);
  }

  /** PDF manuals and other documents staff uploaded for this product. */
  getDocuments(id: number): Observable<ProductDocument[]> {
    return this.http.get<ProductDocument[]>(`${this.baseUrl}/${id}/documents`);
  }

  getSimilar(id: number, count = 4): Observable<ProductListItem[]> {
    return this.http.get<ProductListItem[]>(`${this.baseUrl}/${id}/similar`, {
      params: new HttpParams().set('count', count),
    });
  }
}

/** Turns a ProductQuery into URL parameters the API understands (lists become repeated keys). */
function toHttpParams(query: ProductQuery): HttpParams {
  let params = new HttpParams()
    .set('sort', query.sort)
    .set('page', query.page)
    .set('pageSize', query.pageSize);

  if (query.search) params = params.set('search', query.search);
  if (query.categoryId) params = params.set('categoryId', query.categoryId);
  if (query.minPrice !== null) params = params.set('minPrice', query.minPrice);
  if (query.maxPrice !== null) params = params.set('maxPrice', query.maxPrice);
  if (query.inStock) params = params.set('inStock', true);
  query.brandIds.forEach((id) => (params = params.append('brandIds', id)));
  query.attributes.forEach((attribute) => (params = params.append('attributes', attribute)));

  return params;
}
