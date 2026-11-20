import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { Router, RouterLink } from '@angular/router';
import { AuthStore } from '../../auth/auth.store';
import { CategoryStore, categoryIcon } from '../../catalog/category.store';

@Component({
  selector: 'app-header',
  imports: [FormsModule, RouterLink, MatButtonModule, MatIconModule, MatMenuModule],
  templateUrl: './header.html',
  styleUrl: './header.scss',
})
export class Header {
  protected readonly auth = inject(AuthStore);
  protected readonly categories = inject(CategoryStore);
  private readonly router = inject(Router);

  protected readonly searchTerm = signal('');
  protected readonly categoryIcon = categoryIcon;

  protected search(): void {
    const term = this.searchTerm().trim();
    void this.router.navigate(['/products'], { queryParams: term ? { search: term } : {} });
  }

  protected logout(): void {
    this.auth.logout();
    void this.router.navigateByUrl('/');
  }
}
