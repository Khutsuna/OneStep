import { HttpErrorResponse } from '@angular/common/http';
import { FormGroup } from '@angular/forms';
import { ProblemDetails } from '../models/common.models';

/**
 * Shows the API's validation errors on the matching form fields
 * (e.g. errors.Email -> the "email" control) and returns a general message for a banner.
 */
export function applyServerErrors(form: FormGroup, error: HttpErrorResponse): string {
  const problem = (error.error ?? {}) as ProblemDetails;

  if (problem.errors) {
    for (const [key, messages] of Object.entries(problem.errors)) {
      const controlName = key.charAt(0).toLowerCase() + key.slice(1);
      form.get(controlName)?.setErrors({ server: messages[0] });
    }
    return 'Please fix the highlighted fields.';
  }

  if (error.status === 0) {
    return "Can't reach the OneStep server. Check that the API is running.";
  }

  return problem.detail ?? problem.title ?? 'Something went wrong. Please try again.';
}
