import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, input, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Router, RouterLink } from '@angular/router';
import { AuthStore } from '../../../core/auth/auth.store';
import { applyServerErrors } from '../../../core/util/form-errors';

@Component({
  selector: 'app-login-page',
  imports: [ReactiveFormsModule, RouterLink, MatButtonModule, MatFormFieldModule, MatInputModule],
  templateUrl: './login-page.html',
  styleUrl: '../auth-page.scss',
})
export class LoginPage {
  /** Where to go after logging in, from ?returnUrl=... (set by the auth guard). */
  readonly returnUrl = input<string>('/');

  private readonly auth = inject(AuthStore);
  private readonly router = inject(Router);

  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly form = inject(FormBuilder).nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);

    this.auth.login(this.form.getRawValue()).subscribe({
      next: () => {
        // A page asked for the login (returnUrl)? Go back there. Otherwise staff start in the admin panel.
        const returnUrl = this.returnUrl();
        const target = returnUrl && returnUrl !== '/' ? returnUrl : this.auth.isStaff() ? '/admin' : '/';
        void this.router.navigateByUrl(target);
      },
      error: (error: HttpErrorResponse) => {
        this.submitting.set(false);
        this.errorMessage.set(
          error.status === 401 ? 'Wrong email or password.' : applyServerErrors(this.form, error),
        );
      },
    });
  }
}
