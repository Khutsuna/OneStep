import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Cart } from '../models/shop.models';

/** Talks to /api/cart. Every call returns the whole updated cart, so the UI never has to guess. */
@Injectable({ providedIn: 'root' })
export class CartApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/cart`;

  get(): Observable<Cart> {
    return this.http.get<Cart>(this.baseUrl);
  }

  add(productId: number, quantity: number): Observable<Cart> {
    return this.http.post<Cart>(`${this.baseUrl}/items`, { productId, quantity });
  }

  updateQuantity(itemId: number, quantity: number): Observable<Cart> {
    return this.http.put<Cart>(`${this.baseUrl}/items/${itemId}/quantity`, { quantity });
  }

  remove(itemId: number): Observable<Cart> {
    return this.http.delete<Cart>(`${this.baseUrl}/items/${itemId}`);
  }
}
