import { DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { MatBadgeModule } from '@angular/material/badge';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthStore } from '../../../core/auth/auth.store';
import { StaffNotification } from '../../../core/models/admin.models';
import { NotificationsStore } from './notifications.store';

/** The frame of the admin panel: top bar with alerts, side navigation, and the current admin page. */
@Component({
  selector: 'app-admin-layout',
  imports: [DatePipe, RouterLink, RouterLinkActive, RouterOutlet, MatBadgeModule, MatButtonModule, MatIconModule, MatMenuModule],
  providers: [NotificationsStore],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.scss',
})
export class AdminLayout {
  protected readonly auth = inject(AuthStore);
  protected readonly notifications = inject(NotificationsStore);
  private readonly router = inject(Router);

  protected readonly icons: Record<StaffNotification['type'], string> = {
    LowStock: 'inventory',
    OutOfStock: 'remove_shopping_cart',
    NewOrder: 'shopping_bag',
  };

  /** Opens what the alert is about, and marks it as read. */
  protected open(notification: StaffNotification): void {
    this.notifications.markRead(notification.id);
    if (notification.orderId) {
      void this.router.navigate(['/admin/orders', notification.orderId]);
    } else if (notification.productId) {
      void this.router.navigate(['/admin/products', notification.productId]);
    }
  }

  protected logout(): void {
    this.auth.logout();
    void this.router.navigateByUrl('/');
  }
}
