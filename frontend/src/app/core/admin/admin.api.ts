import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/common.models';
import { UserRole } from '../models/auth.models';
import { Campaign, Dashboard, LowStockProduct, NotificationList, UserListItem } from '../models/admin.models';

/** Admin-panel endpoints: analytics, alerts, campaigns, users and low stock. All need a staff token. */
@Injectable({ providedIn: 'root' })
export class AdminApi {
  private readonly http = inject(HttpClient);
  private readonly api = `${environment.apiUrl}/api`;

  getDashboard(days: number): Observable<Dashboard> {
    return this.http.get<Dashboard>(`${this.api}/analytics/dashboard`, { params: { days } });
  }

  getLowStock(): Observable<LowStockProduct[]> {
    return this.http.get<LowStockProduct[]>(`${this.api}/products/low-stock`);
  }

  getNotifications(take = 20): Observable<NotificationList> {
    return this.http.get<NotificationList>(`${this.api}/notifications`, { params: { take } });
  }

  markNotificationRead(id: number): Observable<void> {
    return this.http.put<void>(`${this.api}/notifications/${id}/read`, {});
  }

  markAllNotificationsRead(): Observable<void> {
    return this.http.put<void>(`${this.api}/notifications/read-all`, {});
  }

  getCampaigns(): Observable<Campaign[]> {
    return this.http.get<Campaign[]>(`${this.api}/campaigns`);
  }

  setCampaignEnabled(id: number, isEnabled: boolean): Observable<Campaign> {
    return this.http.put<Campaign>(`${this.api}/campaigns/${id}/enabled`, { isEnabled });
  }

  /** Admin only. */
  getUsers(search: string | null, role: UserRole | null, page: number, pageSize = 20): Observable<PagedResult<UserListItem>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (search) params = params.set('search', search);
    if (role) params = params.set('role', role);
    return this.http.get<PagedResult<UserListItem>>(`${this.api}/users`, { params });
  }

  /** Admin only. */
  updateUser(id: number, role: UserRole, isActive: boolean): Observable<UserListItem> {
    return this.http.put<UserListItem>(`${this.api}/users/${id}`, { role, isActive });
  }
}
