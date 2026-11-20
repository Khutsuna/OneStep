import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink } from '@angular/router';
import { EmptyState } from '../../shared/ui/empty-state/empty-state';

@Component({
  selector: 'app-not-found-page',
  imports: [RouterLink, MatButtonModule, EmptyState],
  template: `
    <div class="page">
      <app-empty-state icon="explore_off" title="This page doesn't exist"
                       message="The link may be old or mistyped.">
        <a matButton="filled" routerLink="/">Go to the home page</a>
      </app-empty-state>
    </div>
  `,
})
export class NotFoundPage {}
