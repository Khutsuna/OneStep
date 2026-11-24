import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ProductReviews, Review, ReviewRequest } from '../models/account.models';

@Injectable({ providedIn: 'root' })
export class ReviewsApi {
  private readonly http = inject(HttpClient);
  private readonly api = `${environment.apiUrl}/api`;

  getForProduct(productId: number, page = 1, pageSize = 5): Observable<ProductReviews> {
    return this.http.get<ProductReviews>(`${this.api}/products/${productId}/reviews`, { params: { page, pageSize } });
  }

  create(productId: number, request: ReviewRequest): Observable<Review> {
    return this.http.post<Review>(`${this.api}/products/${productId}/reviews`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.api}/reviews/${id}`);
  }
}
