import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AdminApi } from '../../../core/admin/admin.api';
import { Campaign, CampaignState } from '../../../core/models/admin.models';

/** Marketing campaigns: when they run, what they discount, and a switch to turn each one off or on. */
@Component({
  selector: 'app-admin-campaigns-page',
  imports: [CurrencyPipe, DatePipe, MatSlideToggleModule],
  templateUrl: './admin-campaigns-page.html',
  styleUrl: './admin-campaigns-page.scss',
})
export class AdminCampaignsPage {
  private readonly api = inject(AdminApi);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly campaigns = signal<Campaign[] | null>(null);
  protected readonly stateLabels: Record<CampaignState, string> = {
    Scheduled: 'Scheduled',
    Active: 'Running now',
    Ended: 'Ended',
    Disabled: 'Switched off',
  };

  constructor() {
    this.api.getCampaigns().subscribe((list) => this.campaigns.set(list));
  }

  protected toggle(campaign: Campaign, isEnabled: boolean): void {
    this.api.setCampaignEnabled(campaign.id, isEnabled).subscribe({
      next: (updated) => {
        this.campaigns.update((list) => list!.map((c) => (c.id === updated.id ? updated : c)));
        this.snackBar.open(
          isEnabled ? `${campaign.name} is switched on.` : `${campaign.name} is switched off. Prices are back to normal.`,
          'OK',
          { duration: 4000 },
        );
      },
      // Put the switch back if saving failed.
      error: () => this.campaigns.update((list) => [...list!]),
    });
  }

  protected target(c: Campaign): string {
    if (c.appliesToAllProducts) return 'All products';
    const parts = [...c.categoryNames];
    if (c.productCount) parts.push(`${c.productCount} product${c.productCount === 1 ? '' : 's'}`);
    return parts.join(', ') || '—';
  }
}
