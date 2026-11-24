import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink } from '@angular/router';
import { EmptyState } from '../../shared/ui/empty-state/empty-state';
import { I18N } from '../../core/i18n/translate.pipes';

@Component({
  selector: 'app-not-found-page',
  imports: [I18N, RouterLink, MatButtonModule, EmptyState],
  template: `
    <div class="page">
      <app-empty-state icon="explore_off" [title]="'notFound.title' | t"
                       [message]="'notFound.message' | t">
        <a matButton="filled" routerLink="/">{{ 'notFound.home' | t }}</a>
      </app-empty-state>
    </div>
  `,
})
export class NotFoundPage {}
