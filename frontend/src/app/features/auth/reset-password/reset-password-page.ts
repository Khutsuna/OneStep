import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, input, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { RouterLink } from '@angular/router';
import { AuthApi } from '../../../core/auth/auth.api';
import { LanguageStore } from '../../../core/i18n/language.store';
import { I18N } from '../../../core/i18n/translate.pipes';
import { applyServerErrors } from '../../../core/util/form-errors';

/** Validator for "repeat the password": must equal the newPassword field next to it. */
function matchesNewPassword(control: AbstractControl): ValidationErrors | null {
  const newPassword = control.parent?.get('newPassword')?.value as string | undefined;
  return control.value && newPassword !== undefined && control.value !== newPassword ? { mismatch: true } : null;
}

/** Step 2 of password reset, opened from the emailed link: /reset-password?token=... */
@Component({
  selector: 'app-reset-password-page',
  imports: [I18N, ReactiveFormsModule, RouterLink, MatButtonModule, MatFormFieldModule, MatInputModule],
  templateUrl: './reset-password-page.html',
  styleUrl: '../auth-page.scss',
})
export class ResetPasswordPage {
  /** The ?token=... query parameter (withComponentInputBinding passes it in as an input). */
  readonly token = input<string>('');

  private readonly authApi = inject(AuthApi);
  private readonly i18n = inject(LanguageStore);

  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly done = signal(false);

  // The same password rules as registration and the API (ResetPasswordRequest).
  protected readonly form = inject(FormBuilder).nonNullable.group({
    newPassword: [
      '',
      [Validators.required, Validators.minLength(8), Validators.pattern(/^(?=.*[A-Za-z])(?=.*\d).+$/)],
    ],
    confirmPassword: ['', [Validators.required, matchesNewPassword]],
  });

  protected submit(): void {
    // The new password may have changed after "repeat" was typed, so check the match again.
    this.form.controls.confirmPassword.updateValueAndValidity();
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);

    this.authApi.resetPassword({ token: this.token(), newPassword: this.form.getRawValue().newPassword }).subscribe({
      next: () => {
        this.submitting.set(false);
        this.done.set(true);
      },
      error: (error: HttpErrorResponse) => {
        this.submitting.set(false);
        // A 400 without field errors means the token itself was rejected (wrong, expired or used).
        const tokenRejected = error.status === 400 && !error.error?.errors;
        this.errorMessage.set(
          tokenRejected ? this.i18n.t('auth.resetInvalid') : applyServerErrors(this.form, error, this.i18n),
        );
      },
    });
  }
}
