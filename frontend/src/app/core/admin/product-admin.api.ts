import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { NamedOption, ProductRequest } from '../models/admin.models';
import { ProductDetails, ProductImage, ProductListItem } from '../models/catalog.models';
import { PagedResult } from '../models/common.models';

/** Product management for staff: list (including inactive), create, edit, stock, images. */
@Injectable({ providedIn: 'root' })
export class ProductAdminApi {
  private readonly http = inject(HttpClient);
  private readonly api = `${environment.apiUrl}/api`;
  private readonly products = `${this.api}/products`;

  list(search: string | null, page: number, pageSize = 20): Observable<PagedResult<ProductListItem>> {
    let params = new HttpParams()
      .set('includeInactive', true)
      .set('sort', 'Name')
      .set('page', page)
      .set('pageSize', pageSize);
    if (search) params = params.set('search', search);
    return this.http.get<PagedResult<ProductListItem>>(this.products, { params });
  }

  getById(id: number): Observable<ProductDetails> {
    return this.http.get<ProductDetails>(`${this.products}/${id}`);
  }

  create(request: ProductRequest): Observable<ProductDetails> {
    return this.http.post<ProductDetails>(this.products, request);
  }

  update(id: number, request: ProductRequest): Observable<ProductDetails> {
    return this.http.put<ProductDetails>(`${this.products}/${id}`, request);
  }

  updateStock(id: number, stockQuantity: number): Observable<void> {
    return this.http.patch<void>(`${this.products}/${id}/stock`, { stockQuantity });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.products}/${id}`);
  }

  /** Sends the file as multipart/form-data, the way a browser form would. */
  uploadImage(productId: number, file: File): Observable<ProductImage> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<ProductImage>(`${this.products}/${productId}/images`, form);
  }

  setMainImage(productId: number, imageId: number): Observable<void> {
    return this.http.put<void>(`${this.products}/${productId}/images/${imageId}/main`, {});
  }

  deleteImage(productId: number, imageId: number): Observable<void> {
    return this.http.delete<void>(`${this.products}/${productId}/images/${imageId}`);
  }

  getBrands(): Observable<NamedOption[]> {
    return this.http.get<NamedOption[]>(`${this.api}/brands`);
  }

  getAttributes(): Observable<NamedOption[]> {
    return this.http.get<NamedOption[]>(`${this.api}/attributes`);
  }
}
