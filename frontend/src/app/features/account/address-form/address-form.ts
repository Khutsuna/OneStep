import { HttpErrorResponse } from '@angular/common/http';
import { Component, effect, inject, input, output, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Address } from '../../../core/models/shop.models';
import { AddressesApi } from '../../../core/shop/addresses.api';
import { applyServerErrors } from '../../../core/util/form-errors';

/**
 * Add or edit an address. Used at checkout now, and in the address book later.
 * It saves through the API itself and tells the parent the result through (saved).
 */
@Component({
  selector: 'app-address-form',
  imports: [ReactiveFormsModule, MatButtonModule, MatCheckboxModule, MatFormFieldModule, MatInputModule],
  templateUrl: './address-form.html',
  styles: `
    form { display: grid; gap: 4px 12px; grid-template-columns: 1fr 1fr; }
    .wide { grid-column: 1 / -1; }
    .error { grid-column: 1 / -1; background: #fdecee; color: var(--os-sale); border-radius: var(--os-radius-sm); padding: 10px 12px; margin: 0 0 8px; }
    .actions { grid-column: 1 / -1; display: flex; gap: 8px; flex-wrap: wrap; margin-top: 4px; }
    @media (max-width: 599px) { form { grid-template-columns: 1fr; } }
  `,
})
export class AddressForm {
  /** Pass an address to edit it; leave empty to add a new one. */
  readonly address = input<Address | null>(null);
  readonly saved = output<Address>();
  readonly cancelled = output<void>();

  private readonly api = inject(AddressesApi);
  protected readonly saving = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly form = inject(FormBuilder).nonNullable.group({
    label: ['Home', [Validators.required, Validators.maxLength(50)]],
    recipientName: ['', [Validators.required, Validators.maxLength(150)]],
    phone: ['', [Validators.required, Validators.maxLength(30), Validators.pattern(/^[+0-9 ()-]{6,}$/)]],
    line1: ['', [Validators.required, Validators.maxLength(200)]],
    line2: ['', Validators.maxLength(200)],
    city: ['Tbilisi', [Validators.required, Validators.maxLength(100)]],
    region: ['', Validators.maxLength(100)],
    postalCode: ['', Validators.maxLength(20)],
    isDefault: [false],
  });

  constructor() {
    // When editing, fill the form with the existing address.
    effect(() => {
      const address = this.address();
      if (address) {
        this.form.patchValue({
          ...address,
          line2: address.line2 ?? '',
          region: address.region ?? '',
          postalCode: address.postalCode ?? '',
        });
      }
    });
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const request = {
      ...value,
      line2: value.line2 || null,
      region: value.region || null,
      postalCode: value.postalCode || null,
      countryCode: 'GE',
    };
    const existing = this.address();

    this.saving.set(true);
    this.errorMessage.set(null);
    const save$ = existing ? this.api.update(existing.id, request) : this.api.create(request);

    save$.subscribe({
      next: (address) => {
        this.saving.set(false);
        this.saved.emit(address);
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.errorMessage.set(applyServerErrors(this.form, error));
      },
    });
  }
}
