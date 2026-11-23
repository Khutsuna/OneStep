import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Address, AddressRequest } from '../models/shop.models';

/** The logged-in customer's address book (/api/account/addresses). */
@Injectable({ providedIn: 'root' })
export class AddressesApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/account/addresses`;

  getAll(): Observable<Address[]> {
    return this.http.get<Address[]>(this.baseUrl);
  }

  create(request: AddressRequest): Observable<Address> {
    return this.http.post<Address>(this.baseUrl, request);
  }

  update(id: number, request: AddressRequest): Observable<Address> {
    return this.http.put<Address>(`${this.baseUrl}/${id}`, request);
  }

  setDefault(id: number): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/default`, {});
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
