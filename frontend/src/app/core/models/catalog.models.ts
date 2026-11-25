// TypeScript versions of the API's catalog DTOs.

export type ProductStatus = 'Active' | 'Inactive';
export type ProductSort = 'Newest' | 'PriceAsc' | 'PriceDesc' | 'Rating' | 'Name';

export interface CategoryNode {
  id: number;
  parentId: number | null;
  name: string;
  slug: string;
  description: string | null;
  imageUrl: string | null;
  sortOrder: number;
  productCount: number;
  children: CategoryNode[];
}

export interface Brand {
  id: number;
  name: string;
  slug: string;
  logoUrl: string | null;
  productCount: number;
}

export interface ProductListItem {
  id: number;
  name: string;
  slug: string;
  price: number;
  finalPrice: number;
  discount: number;
  campaignName: string | null;
  /** When the campaign's discount ends (UTC), for the deals countdown. */
  campaignEndsAt: string | null;
  brandName: string | null;
  categoryName: string;
  mainImageUrl: string | null;
  averageRating: number;
  reviewCount: number;
  stockQuantity: number;
  inStock: boolean;
  status: ProductStatus;
}

/** One point of the price history: from this date on, the base price was this. */
export interface PricePoint {
  date: string;
  price: number;
}

export interface ProductImage {
  id: number;
  url: string;
  altText: string | null;
  sortOrder: number;
  isMain: boolean;
}

/** A downloadable PDF for a product: a user manual, a warranty card... */
export interface ProductDocument {
  id: number;
  title: string;
  fileName: string;
  sizeBytes: number;
  url: string;
  createdAt: string;
}

export interface ProductAttributeValue {
  attributeId: number;
  name: string;
  value: string;
}

export interface ProductDetails {
  id: number;
  name: string;
  slug: string;
  sku: string;
  description: string;
  price: number;
  finalPrice: number;
  discount: number;
  campaignName: string | null;
  stockQuantity: number;
  lowStockThreshold: number;
  inStock: boolean;
  status: ProductStatus;
  categoryId: number;
  categoryName: string;
  brandId: number | null;
  brandName: string | null;
  averageRating: number;
  reviewCount: number;
  images: ProductImage[];
  attributes: ProductAttributeValue[];
  createdAt: string;
}

export interface ProductFilters {
  brands: { id: number; name: string; productCount: number }[];
  minPrice: number;
  maxPrice: number;
  attributes: { name: string; values: string[] }[];
}

/** Everything the product list can be filtered and sorted by. Mirrors ProductSearchRequest in the API. */
export interface ProductQuery {
  search: string | null;
  categoryId: number | null;
  brandIds: number[];
  minPrice: number | null;
  maxPrice: number | null;
  attributes: string[]; // "Name:Value", e.g. "Color:Black"
  inStock: boolean;
  sort: ProductSort;
  page: number;
  pageSize: number;
}
