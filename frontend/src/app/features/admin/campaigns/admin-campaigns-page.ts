import { CurrencyPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AdminApi } from '../../../core/admin/admin.api';
import { Campaign, CampaignState } from '../../../core/models/admin.models';
import { I18N } from '../../../core/i18n/translate.pipes';
import { LanguageStore } from '../../../core/i18n/language.store';
import { TranslationKey } from '../../../core/i18n/en';

/** Marketing campaigns: when they run, what they discount, and a switch to turn each one off or on. */
@Component({
  selector: 'app-admin-campaigns-page',
  imports: [I18N, CurrencyPipe, MatSlideToggleModule],
  templateUrl: './admin-campaigns-page.html',
  styleUrl: './admin-campaigns-page.scss',
})
export class AdminCampaignsPage {
  private readonly api = inject(AdminApi);
  private readonly snackBar = inject(MatSnackBar);
  private readonly i18n = inject(LanguageStore);

  protected readonly campaigns = signal<Campaign[] | null>(null);
  protected readonly stateKey = (state: CampaignState): TranslationKey => `camp.${state}`;

  constructor() {
    this.api.getCampaigns().subscribe((list) => this.campaigns.set(list));
  }

  protected toggle(campaign: Campaign, isEnabled: boolean): void {
    this.api.setCampaignEnabled(campaign.id, isEnabled).subscribe({
      next: (updated) => {
        this.campaigns.update((list) => list!.map((c) => (c.id === updated.id ? updated : c)));
        this.snackBar.open(
          this.i18n.t(isEnabled ? 'camp.nowOn' : 'camp.nowOff', { name: campaign.name }),
          this.i18n.t('common.ok'),
          { duration: 4000 },
        );
      },
      // Put the switch back if saving failed.
      error: () => this.campaigns.update((list) => [...list!]),
    });
  }

  protected target(c: Campaign): string {
    if (c.appliesToAllProducts) return this.i18n.t('camp.allProducts');
    const parts = c.categoryNames.map((name) => this.i18n.term(name));
    if (c.productCount) parts.push(this.i18n.t('camp.products', { count: c.productCount }));
    return parts.join(', ') || '—';
  }
}
