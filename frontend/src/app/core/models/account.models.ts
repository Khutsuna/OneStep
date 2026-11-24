// TypeScript versions of the API's review, profile and password DTOs.
import { PagedResult } from './common.models';

export interface Review {
  id: number;
  productId: number;
  rating: number;
  title: string | null;
  comment: string | null;
  authorName: string; // "Nino B."
  createdAt: string;
  isMine: boolean;
}

export interface ProductReviews {
  averageRating: number;
  reviewCount: number;
  ratingCounts: number[]; // [5-star, 4-star, 3-star, 2-star, 1-star]
  reviews: PagedResult<Review>;
  canReview: boolean;
  cannotReviewReason: string | null;
}

export interface ReviewRequest {
  rating: number;
  title: string | null;
  comment: string | null;
}

export interface Profile {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber: string | null;
  createdAt: string;
}

export interface UpdateProfileRequest {
  firstName: string;
  lastName: string;
  phoneNumber: string | null;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}
