import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { RouterLink } from '@angular/router';
import { AuthApi } from '../../../core/auth/auth.api';
import { LanguageStore } from '../../../core/i18n/language.store';
import { I18N } from '../../../core/i18n/translate.pipes';
import { applyServerErrors } from '../../../core/util/form-errors';

/** Step 1 of password reset: the user enters their email and the API sends them a link. */
@Component({
  selector: 'app-forgot-password-page',
  imports: [I18N, ReactiveFormsModule, RouterLink, MatButtonModule, MatFormFieldModule, MatInputModule],
  templateUrl: './forgot-password-page.html',
  styleUrl: '../auth-page.scss',
})
export class ForgotPasswordPage {
  private readonly authApi = inject(AuthApi);
  private readonly i18n = inject(LanguageStore);

  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  /** Set after a successful request; switches the page to the "check your email" message. */
  protected readonly sentTo = signal<string | null>(null);

  protected readonly form = inject(FormBuilder).nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
  });

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);
    const email = this.form.getRawValue().email.trim();

    this.authApi.forgotPassword({ email }).subscribe({
      next: () => {
        this.submitting.set(false);
        this.sentTo.set(email);
      },
      error: (error: HttpErrorResponse) => {
        this.submitting.set(false);
        this.errorMessage.set(applyServerErrors(this.form, error, this.i18n));
      },
    });
  }
}
