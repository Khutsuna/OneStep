import { HttpErrorResponse } from '@angular/common/http';
import { FormGroup } from '@angular/forms';
import { ProblemDetails } from '../models/common.models';
import { TranslationKey } from '../i18n/en';

/**
 * Shows the API's validation errors on the matching form fields
 * (e.g. errors.Email -> the "email" control) and returns a general message for a banner.
 */
/** Anything that can translate a key (the LanguageStore). */
interface Translator {
  t(key: TranslationKey): string;
}

export function applyServerErrors(form: FormGroup, error: HttpErrorResponse, i18n: Translator): string {
  const problem = (error.error ?? {}) as ProblemDetails;

  if (problem.errors) {
    for (const [key, messages] of Object.entries(problem.errors)) {
      const controlName = key.charAt(0).toLowerCase() + key.slice(1);
      form.get(controlName)?.setErrors({ server: messages[0] });
    }
    return i18n.t('errors.fixFields');
  }

  if (error.status === 0) {
    return i18n.t('errors.offline');
  }

  return problem.detail ?? problem.title ?? i18n.t('errors.generic');
}
