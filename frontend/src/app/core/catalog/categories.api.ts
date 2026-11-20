import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CategoryNode } from '../models/catalog.models';

@Injectable({ providedIn: 'root' })
export class CategoriesApi {
  private readonly http = inject(HttpClient);

  getTree(): Observable<CategoryNode[]> {
    return this.http.get<CategoryNode[]>(`${environment.apiUrl}/api/categories`);
  }
}
