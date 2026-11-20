/** One page of a longer list. Matches PagedResult<T> in the API. */
export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

/** The API's error format (ProblemDetails, RFC 9457). `errors` is filled for validation errors. */
export interface ProblemDetails {
  title?: string;
  status?: number;
  detail?: string;
  errors?: Record<string, string[]>;
}
