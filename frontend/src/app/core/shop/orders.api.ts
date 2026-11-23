import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/common.models';
import {
  OrderDetails,
  OrderListQuery,
  OrderStatus,
  OrderSummary,
  OrderTotals,
  PlaceOrderRequest,
} from '../models/shop.models';

/**
 * Talks to /api/orders. Customers get their own orders; staff get everyone's.
 * The API decides that from the token, so the same calls work for both.
 */
@Injectable({ providedIn: 'root' })
export class OrdersApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/orders`;

  list(query: OrderListQuery): Observable<PagedResult<OrderSummary>> {
    let params = new HttpParams().set('page', query.page).set('pageSize', query.pageSize);
    if (query.status) params = params.set('status', query.status);
    if (query.search) params = params.set('search', query.search);
    return this.http.get<PagedResult<OrderSummary>>(this.baseUrl, { params });
  }

  getById(id: number): Observable<OrderDetails> {
    return this.http.get<OrderDetails>(`${this.baseUrl}/${id}`);
  }

  /** Totals for the current cart with a delivery method: the live preview at checkout. */
  quote(deliveryMethodId: number): Observable<OrderTotals> {
    return this.http.post<OrderTotals>(`${this.baseUrl}/quote`, { deliveryMethodId });
  }

  place(request: PlaceOrderRequest): Observable<OrderDetails> {
    return this.http.post<OrderDetails>(this.baseUrl, request);
  }

  /** Mock card payment: no real money moves. */
  pay(id: number): Observable<OrderDetails> {
    return this.http.post<OrderDetails>(`${this.baseUrl}/${id}/pay`, {});
  }

  cancel(id: number): Observable<OrderDetails> {
    return this.http.post<OrderDetails>(`${this.baseUrl}/${id}/cancel`, {});
  }

  /** Staff only. */
  changeStatus(id: number, status: OrderStatus, note: string | null): Observable<OrderDetails> {
    return this.http.put<OrderDetails>(`${this.baseUrl}/${id}/status`, { status, note });
  }
}
