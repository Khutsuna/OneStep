import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, input, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Router, RouterLink } from '@angular/router';
import { AuthStore } from '../../../core/auth/auth.store';
import { applyServerErrors } from '../../../core/util/form-errors';
import { I18N } from '../../../core/i18n/translate.pipes';
import { LanguageStore } from '../../../core/i18n/language.store';

@Component({
  selector: 'app-register-page',
  imports: [I18N, ReactiveFormsModule, RouterLink, MatButtonModule, MatFormFieldModule, MatInputModule],
  templateUrl: './register-page.html',
  styleUrl: '../auth-page.scss',
})
export class RegisterPage {
  readonly returnUrl = input<string>('/');

  private readonly auth = inject(AuthStore);
  private readonly router = inject(Router);
  private readonly i18n = inject(LanguageStore);

  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  // The same rules as RegisterRequest in the API, so most mistakes are caught before sending.
  protected readonly form = inject(FormBuilder).nonNullable.group({
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    password: [
      '',
      [Validators.required, Validators.minLength(8), Validators.pattern(/^(?=.*[A-Za-z])(?=.*\d).+$/)],
    ],
  });

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);

    this.auth.register(this.form.getRawValue()).subscribe({
      next: () => void this.router.navigateByUrl(this.returnUrl() || '/'),
      error: (error: HttpErrorResponse) => {
        this.submitting.set(false);
        this.errorMessage.set(applyServerErrors(this.form, error, this.i18n));
      },
    });
  }
}
