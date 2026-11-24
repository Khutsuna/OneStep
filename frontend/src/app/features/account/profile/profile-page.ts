import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar } from '@angular/material/snack-bar';
import { RouterLink } from '@angular/router';
import { AuthStore } from '../../../core/auth/auth.store';
import { Profile } from '../../../core/models/account.models';
import { Address } from '../../../core/models/shop.models';
import { AccountApi } from '../../../core/shop/account.api';
import { AddressesApi } from '../../../core/shop/addresses.api';
import { applyServerErrors } from '../../../core/util/form-errors';
import { AddressForm } from '../address-form/address-form';
import { I18N } from '../../../core/i18n/translate.pipes';
import { LanguageStore } from '../../../core/i18n/language.store';

/** "My account": personal details, password, and (for customers) the address book. */
@Component({
  selector: 'app-profile-page',
  imports: [I18N, ReactiveFormsModule, RouterLink, MatButtonModule, MatFormFieldModule, MatIconModule, MatInputModule,
    AddressForm,
  ],
  templateUrl: './profile-page.html',
  styleUrl: './profile-page.scss',
})
export class ProfilePage {
  protected readonly auth = inject(AuthStore);
  private readonly accountApi = inject(AccountApi);
  private readonly addressesApi = inject(AddressesApi);
  private readonly snackBar = inject(MatSnackBar);
  private readonly i18n = inject(LanguageStore);
  private readonly fb = inject(FormBuilder).nonNullable;

  protected readonly profile = signal<Profile | null>(null);
  protected readonly savingProfile = signal(false);
  protected readonly savingPassword = signal(false);
  protected readonly profileError = signal<string | null>(null);
  protected readonly passwordError = signal<string | null>(null);

  // ---- Address book ----
  protected readonly addresses = signal<Address[] | null>(null);
  /** null = no form open; 'new' = adding; an Address = editing that one. */
  protected readonly editing = signal<Address | 'new' | null>(null);
  protected readonly confirmDeleteId = signal<number | null>(null);

  protected readonly profileForm = this.fb.group({
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    phoneNumber: ['', [Validators.maxLength(30), Validators.pattern(/^[+0-9 ()-]{6,}$/)]],
  });

  protected readonly passwordForm = this.fb.group({
    currentPassword: ['', Validators.required],
    newPassword: ['', [Validators.required, Validators.minLength(8), Validators.pattern(/^(?=.*[A-Za-z])(?=.*\d).+$/)]],
  });

  constructor() {
    this.accountApi.getProfile().subscribe((profile) => {
      this.profile.set(profile);
      this.profileForm.setValue({
        firstName: profile.firstName,
        lastName: profile.lastName,
        phoneNumber: profile.phoneNumber ?? '',
      });
    });
    if (this.auth.isCustomer()) {
      this.loadAddresses();
    }
  }

  protected saveProfile(): void {
    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }
    const value = this.profileForm.getRawValue();
    this.savingProfile.set(true);
    this.profileError.set(null);
    this.accountApi.updateProfile({ ...value, phoneNumber: value.phoneNumber.trim() || null }).subscribe({
      next: (profile) => {
        this.savingProfile.set(false);
        this.profile.set(profile);
        this.auth.updateUser({ firstName: profile.firstName, lastName: profile.lastName }); // header shows the new name
        this.snackBar.open(this.i18n.t('profile.saved'), this.i18n.t('common.ok'), { duration: 3000 });
      },
      error: (error: HttpErrorResponse) => {
        this.savingProfile.set(false);
        this.profileError.set(applyServerErrors(this.profileForm, error, this.i18n));
      },
    });
  }

  protected changePassword(): void {
    if (this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();
      return;
    }
    this.savingPassword.set(true);
    this.passwordError.set(null);
    this.accountApi.changePassword(this.passwordForm.getRawValue()).subscribe({
      next: () => {
        this.savingPassword.set(false);
        this.passwordForm.reset();
        this.snackBar.open(this.i18n.t('profile.passwordChanged'), this.i18n.t('common.ok'), { duration: 3000 });
      },
      error: (error: HttpErrorResponse) => {
        this.savingPassword.set(false);
        this.passwordError.set(applyServerErrors(this.passwordForm, error, this.i18n));
      },
    });
  }

  // ---- Address book ----

  private loadAddresses(): void {
    this.addressesApi.getAll().subscribe((list) => this.addresses.set(list));
  }

  protected onAddressSaved(): void {
    this.editing.set(null);
    this.loadAddresses();
    this.snackBar.open(this.i18n.t('profile.addressSaved'), this.i18n.t('common.ok'), { duration: 3000 });
  }

  protected makeDefault(address: Address): void {
    this.addressesApi.setDefault(address.id).subscribe(() => this.loadAddresses());
  }

  protected deleteAddress(address: Address): void {
    this.addressesApi.delete(address.id).subscribe(() => {
      this.confirmDeleteId.set(null);
      this.loadAddresses();
      this.snackBar.open(this.i18n.t('profile.addressDeleted'), this.i18n.t('common.ok'), { duration: 3000 });
    });
  }

  protected isEditing(address: Address): boolean {
    const editing = this.editing();
    return editing !== null && editing !== 'new' && editing.id === address.id;
  }
}
