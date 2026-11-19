using System.ComponentModel.DataAnnotations;
using OneStep.Domain.Enums;

namespace OneStep.Application.Features.Products;

/// <summary>Sort options for the product list.</summary>
public enum ProductSort
{
    Newest,
    PriceAsc,
    PriceDesc,
    Rating,
    Name
}

/// <summary>
/// Everything the shop's product list can filter by. All filters are optional and combine with AND.
/// Bound from the query string, e.g.
/// /api/products?categoryId=1&amp;brandIds=1&amp;brandIds=2&amp;minPrice=500&amp;attributes=Color:Black&amp;sort=PriceAsc
/// </summary>
public class ProductSearchRequest
{
    /// <summary>Text to find in the product, brand or category name.</summary>
    public string? Search { get; set; }

    /// <summary>A category; products in its subcategories are included too.</summary>
    public int? CategoryId { get; set; }

    public List<int> BrandIds { get; set; } = [];

    [Range(0, 1_000_000)]
    public decimal? MinPrice { get; set; }

    [Range(0, 1_000_000)]
    public decimal? MaxPrice { get; set; }

    /// <summary>Attribute filters written as "Name:Value", e.g. "Color:Black" or "Storage:256 GB".</summary>
    public List<string> Attributes { get; set; } = [];

    /// <summary>Only products that are in stock.</summary>
    public bool InStock { get; set; }

    public ProductSort Sort { get; set; } = ProductSort.Newest;

    /// <summary>Staff only: include inactive products (for the admin product list).</summary>
    public bool IncludeInactive { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 12;
}

/// <summary>A product card in the shop's product list.</summary>
public record ProductListItemDto(
    int Id,
    string Name,
    string Slug,
    decimal Price,
    decimal FinalPrice,
    decimal Discount,
    string? CampaignName,
    string? BrandName,
    string CategoryName,
    string? MainImageUrl,
    decimal AverageRating,
    int ReviewCount,
    int StockQuantity,
    bool InStock,
    ProductStatus Status);

public record ProductImageDto(int Id, string Url, string? AltText, int SortOrder, bool IsMain);

public record ProductAttributeValueDto(int AttributeId, string Name, string Value);

/// <summary>The product page.</summary>
public record ProductDetailsDto(
    int Id,
    string Name,
    string Slug,
    string Sku,
    string Description,
    decimal Price,
    decimal FinalPrice,
    decimal Discount,
    string? CampaignName,
    int StockQuantity,
    int LowStockThreshold,
    bool InStock,
    ProductStatus Status,
    int CategoryId,
    string CategoryName,
    int? BrandId,
    string? BrandName,
    decimal AverageRating,
    int ReviewCount,
    IReadOnlyList<ProductImageDto> Images,
    IReadOnlyList<ProductAttributeValueDto> Attributes,
    DateTime CreatedAt);

/// <summary>Options for the filter panel: what brands, prices and attribute values exist.</summary>
public record ProductFiltersDto(
    IReadOnlyList<BrandFilterOption> Brands,
    decimal MinPrice,
    decimal MaxPrice,
    IReadOnlyList<AttributeFilterOption> Attributes);

public record BrandFilterOption(int Id, string Name, int ProductCount);

public record AttributeFilterOption(string Name, IReadOnlyList<string> Values);

public record LowStockProductDto(int Id, string Name, string Sku, int StockQuantity, int LowStockThreshold);

/// <summary>Create or edit a product (staff).</summary>
public class ProductRequest
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Sku { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Range(0, 1_000_000)]
    public decimal Price { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    public int? BrandId { get; set; }

    [Range(0, 1_000_000)]
    public int StockQuantity { get; set; }

    [Range(0, 10_000)]
    public int LowStockThreshold { get; set; } = 5;

    public ProductStatus Status { get; set; } = ProductStatus.Active;

    public List<ProductAttributeValueRequest> Attributes { get; set; } = [];
}

public class ProductAttributeValueRequest
{
    [Range(1, int.MaxValue)]
    public int AttributeId { get; set; }

    [Required]
    [StringLength(100)]
    public string Value { get; set; } = string.Empty;
}

/// <summary>Quick stock change from the admin panel.</summary>
public class StockUpdateRequest
{
    [Range(0, 1_000_000)]
    public int StockQuantity { get; set; }
}
