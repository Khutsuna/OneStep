import { environment } from '../../../environments/environment';

/** The API returns image paths like "/uploads/products/1/a.jpg"; the browser needs the full API address. */
export function mediaUrl(path: string | null | undefined): string | null {
  return path ? `${environment.apiUrl}${path}` : null;
}
