import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ProductListItem } from '../models/catalog.models';

@Injectable({ providedIn: 'root' })
export class WishlistApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/account/wishlist`;

  getProducts(): Observable<ProductListItem[]> {
    return this.http.get<ProductListItem[]>(this.baseUrl);
  }

  getIds(): Observable<number[]> {
    return this.http.get<number[]>(`${this.baseUrl}/ids`);
  }

  add(productId: number): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${productId}`, {});
  }

  remove(productId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${productId}`);
  }
}
