import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DeliveryMethod } from '../models/shop.models';

@Injectable({ providedIn: 'root' })
export class DeliveryMethodsApi {
  private readonly http = inject(HttpClient);

  getAll(): Observable<DeliveryMethod[]> {
    return this.http.get<DeliveryMethod[]>(`${environment.apiUrl}/api/delivery-methods`);
  }
}
