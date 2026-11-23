using Microsoft.EntityFrameworkCore;
using OneStep.Application.Abstractions;
using OneStep.Application.Common;
using OneStep.Application.Common.Exceptions;
using OneStep.Application.Features.Categories;
using OneStep.Application.Features.Inventory;
using OneStep.Application.Features.Pricing;
using OneStep.Domain.Entities;
using OneStep.Domain.Enums;
using OneStep.Domain.ReadModels;

namespace OneStep.Application.Features.Products;

public class ProductService : IProductService
{
    private readonly IAppDbContext _db;
    private readonly IPricingService _pricing;
    private readonly ICategoryService _categories;
    private readonly ICurrentUser _currentUser;

    public ProductService(IAppDbContext db, IPricingService pricing, ICategoryService categories, ICurrentUser currentUser)
    {
        _db = db;
        _pricing = pricing;
        _categories = categories;
        _currentUser = currentUser;
    }

    // =====================================================================
    //  [You] The product search. Replace this method with your own code.
    // =====================================================================
    public async Task<PagedResult<ProductListItemDto>> SearchAsync(ProductSearchRequest request, CancellationToken cancellationToken)
    {
        // 1. Start from your SQL view. Nothing runs in the database yet:
        //    we're only describing the query, piece by piece.
        var query = _db.ProductCatalog.AsNoTracking();

        // 2. Customers only see active products. Staff can ask for inactive ones too.
        if (!(request.IncludeInactive && IsStaff()))
        {
            query = query.Where(p => p.Status == ProductStatus.Active);
        }

        // 3. Text search in the product, brand and category name.
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(p =>
                p.Name.Contains(term) ||
                (p.BrandName != null && p.BrandName.Contains(term)) ||
                p.CategoryName.Contains(term));
        }

        // 4. Category, including all of its subcategories.
        if (request.CategoryId is not null)
        {
            var categoryIds = await _categories.GetSelfAndDescendantIdsAsync(request.CategoryId.Value, cancellationToken);
            query = query.Where(p => categoryIds.Contains(p.CategoryId));
        }

        // 5. Brands: the product must be from ANY of the selected brands.
        if (request.BrandIds.Count > 0)
        {
            query = query.Where(p => p.BrandId != null && request.BrandIds.Contains(p.BrandId.Value));
        }

        // 6. Price range.
        if (request.MinPrice is not null)
        {
            query = query.Where(p => p.Price >= request.MinPrice);
        }
        if (request.MaxPrice is not null)
        {
            query = query.Where(p => p.Price <= request.MaxPrice);
        }

        // 7. Only products in stock.
        if (request.InStock)
        {
            query = query.Where(p => p.StockQuantity > 0);
        }

        // 8. Attributes, e.g. "Color:Black". The product must match ALL of them.
        foreach (var filter in request.Attributes)
        {
            var parts = filter.Split(':', 2);
            if (parts.Length != 2)
            {
                throw new BadRequestException($"Attribute filter '{filter}' must look like Name:Value.");
            }

            var name = parts[0].Trim();
            var value = parts[1].Trim();
            query = query.Where(p => _db.ProductAttributeValues.Any(v =>
                v.ProductId == p.ProductId &&
                v.ProductAttribute.Name == name &&
                v.Value == value));
        }

        // 9. Count every match (for the page buttons), before cutting out one page.
        var totalCount = await query.CountAsync(cancellationToken);

        // 10. Sorting. ThenBy(ProductId) makes the order stable, so paging never repeats or skips a product.
        query = request.Sort switch
        {
            ProductSort.PriceAsc => query.OrderBy(p => p.Price).ThenBy(p => p.ProductId),
            ProductSort.PriceDesc => query.OrderByDescending(p => p.Price).ThenBy(p => p.ProductId),
            ProductSort.Rating => query.OrderByDescending(p => p.AverageRating)
                                       .ThenByDescending(p => p.ReviewCount)
                                       .ThenBy(p => p.ProductId),
            ProductSort.Name => query.OrderBy(p => p.Name).ThenBy(p => p.ProductId),
            _ => query.OrderByDescending(p => p.CreatedAt).ThenBy(p => p.ProductId)
        };

        // 11. Paging: skip the earlier pages and take one page. Only NOW does the SQL run.
        var rows = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // 12. Add the current campaign prices and turn the rows into DTOs.
        var items = await ToListItemsAsync(rows, cancellationToken);
        return new PagedResult<ProductListItemDto>(items, request.Page, request.PageSize, totalCount);
    }

    // =====================================================================
    //  Everything below is ready to use.
    // =====================================================================

    public async Task<ProductDetailsDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var product = await _db.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id, p.Name, p.Slug, p.Sku, p.Description, p.Price, p.StockQuantity, p.LowStockThreshold,
                p.Status, p.CategoryId, CategoryName = p.Category.Name, p.BrandId,
                BrandName = p.Brand != null ? p.Brand.Name : null, p.CreatedAt,
                Images = p.Images
                    .OrderBy(i => i.SortOrder)
                    .Select(i => new { i.Id, i.FilePath, i.AltText, i.SortOrder, i.IsMain })
                    .ToList(),
                Attributes = p.AttributeValues
                    .OrderBy(v => v.ProductAttribute.Name)
                    .Select(v => new ProductAttributeValueDto(v.ProductAttributeId, v.ProductAttribute.Name, v.Value))
                    .ToList(),
                AverageRating = p.Reviews.Average(r => (decimal?)r.Rating) ?? 0m,
                ReviewCount = p.Reviews.Count
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Product {id} was not found.");

        // Customers can't open inactive products; staff can.
        if (product.Status == ProductStatus.Inactive && !IsStaff())
        {
            throw new NotFoundException($"Product {id} was not found.");
        }

        var prices = await _pricing.GetPricesAsync(
            [new PricingInput(product.Id, product.CategoryId, product.Price)], cancellationToken);
        var price = prices[product.Id];

        return new ProductDetailsDto(
            product.Id, product.Name, product.Slug, product.Sku, product.Description,
            price.Price, price.FinalPrice, price.Discount, price.CampaignName,
            product.StockQuantity, product.LowStockThreshold, product.StockQuantity > 0, product.Status,
            product.CategoryId, product.CategoryName, product.BrandId, product.BrandName,
            Math.Round(product.AverageRating, 2), product.ReviewCount,
            product.Images.Select(i => new ProductImageDto(i.Id, MediaUrl.From(i.FilePath)!, i.AltText, i.SortOrder, i.IsMain)).ToList(),
            product.Attributes,
            product.CreatedAt);
    }

    public async Task<ProductFiltersDto> GetFiltersAsync(int? categoryId, CancellationToken cancellationToken)
    {
        var products = _db.ProductCatalog.AsNoTracking().Where(p => p.Status == ProductStatus.Active);

        if (categoryId is not null)
        {
            var categoryIds = await _categories.GetSelfAndDescendantIdsAsync(categoryId.Value, cancellationToken);
            products = products.Where(p => categoryIds.Contains(p.CategoryId));
        }

        var brands = await products
            .Where(p => p.BrandId != null)
            .GroupBy(p => new { p.BrandId, p.BrandName })
            .OrderBy(g => g.Key.BrandName)
            .Select(g => new BrandFilterOption(g.Key.BrandId!.Value, g.Key.BrandName!, g.Count()))
            .ToListAsync(cancellationToken);

        var hasProducts = await products.AnyAsync(cancellationToken);
        var minPrice = hasProducts ? await products.MinAsync(p => p.Price, cancellationToken) : 0m;
        var maxPrice = hasProducts ? await products.MaxAsync(p => p.Price, cancellationToken) : 0m;

        var productIds = products.Select(p => p.ProductId);
        var attributeValues = await _db.ProductAttributeValues
            .AsNoTracking()
            .Where(v => productIds.Contains(v.ProductId))
            .Select(v => new { v.ProductAttribute.Name, v.Value })
            .Distinct()
            .ToListAsync(cancellationToken);

        var attributes = attributeValues
            .GroupBy(a => a.Name)
            .OrderBy(g => g.Key)
            .Select(g => new AttributeFilterOption(g.Key, g.Select(a => a.Value).OrderBy(v => v).ToList()))
            .ToList();

        return new ProductFiltersDto(brands, minPrice, maxPrice, attributes);
    }

    public async Task<IReadOnlyList<ProductListItemDto>> GetSimilarAsync(int id, int count, CancellationToken cancellationToken)
    {
        var product = await _db.Products.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new { p.CategoryId, p.Price })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Product {id} was not found.");

        // "Similar" = same category, closest in price.
        var rows = await _db.ProductCatalog.AsNoTracking()
            .Where(p => p.CategoryId == product.CategoryId && p.ProductId != id && p.Status == ProductStatus.Active)
            .OrderBy(p => Math.Abs(p.Price - product.Price))
            .Take(Math.Clamp(count, 1, 12))
            .ToListAsync(cancellationToken);

        return await ToListItemsAsync(rows, cancellationToken);
    }

    public async Task<IReadOnlyList<LowStockProductDto>> GetLowStockAsync(CancellationToken cancellationToken)
    {
        return await _db.Products.AsNoTracking()
            .Where(p => p.StockQuantity <= p.LowStockThreshold)
            .OrderBy(p => p.StockQuantity).ThenBy(p => p.Name)
            .Select(p => new LowStockProductDto(p.Id, p.Name, p.Sku, p.StockQuantity, p.LowStockThreshold))
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductDetailsDto> CreateAsync(ProductRequest request, CancellationToken cancellationToken)
    {
        await ValidateReferencesAsync(request, productId: null, cancellationToken);

        var product = new Product
        {
            Name = request.Name.Trim(),
            Sku = request.Sku.Trim(),
            Slug = await CreateUniqueSlugAsync(request.Name, productId: null, cancellationToken),
            Description = request.Description.Trim(),
            Price = request.Price,
            CategoryId = request.CategoryId,
            BrandId = request.BrandId,
            StockQuantity = request.StockQuantity,
            LowStockThreshold = request.LowStockThreshold,
            Status = request.Status
        };
        SetAttributes(product, request.Attributes);

        _db.Products.Add(product);
        AddStockNotificationIfNeeded(product, previousQuantity: int.MaxValue);
        await _db.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(product.Id, cancellationToken);
    }

    public async Task<ProductDetailsDto> UpdateAsync(int id, ProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _db.Products
            .Include(p => p.AttributeValues)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Product {id} was not found.");

        await ValidateReferencesAsync(request, id, cancellationToken);

        if (product.Name != request.Name.Trim())
        {
            product.Slug = await CreateUniqueSlugAsync(request.Name, id, cancellationToken);
        }

        var previousQuantity = product.StockQuantity;
        product.Name = request.Name.Trim();
        product.Sku = request.Sku.Trim();
        product.Description = request.Description.Trim();
        product.Price = request.Price;            // a price change is logged by the SQL trigger
        product.CategoryId = request.CategoryId;
        product.BrandId = request.BrandId;
        product.StockQuantity = request.StockQuantity;
        product.LowStockThreshold = request.LowStockThreshold;
        product.Status = request.Status;
        SetAttributes(product, request.Attributes);

        AddStockNotificationIfNeeded(product, previousQuantity);
        await _db.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task UpdateStockAsync(int id, StockUpdateRequest request, CancellationToken cancellationToken)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Product {id} was not found.");

        var previousQuantity = product.StockQuantity;
        product.StockQuantity = request.StockQuantity;
        AddStockNotificationIfNeeded(product, previousQuantity);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Product {id} was not found.");

        // Soft delete: old orders still point to this product, so the row stays.
        product.IsDeleted = true;
        await _db.SaveChangesAsync(cancellationToken);
    }

    // ---------- helpers ----------

    /// <summary>Adds the current (discounted) prices to rows read from the view.</summary>
    private async Task<List<ProductListItemDto>> ToListItemsAsync(
        IReadOnlyCollection<ProductCatalogEntry> rows, CancellationToken cancellationToken)
    {
        var prices = await _pricing.GetPricesAsync(
            rows.Select(r => new PricingInput(r.ProductId, r.CategoryId, r.Price)).ToList(), cancellationToken);

        return rows.Select(r =>
        {
            var price = prices[r.ProductId];
            return new ProductListItemDto(
                r.ProductId, r.Name, r.Slug, price.Price, price.FinalPrice, price.Discount, price.CampaignName,
                r.BrandName, r.CategoryName, MediaUrl.From(r.MainImagePath), r.AverageRating, r.ReviewCount,
                r.StockQuantity, r.StockQuantity > 0, r.Status);
        }).ToList();
    }

    private bool IsStaff()
    {
        return _currentUser.Role is "Admin" or "StoreManager";
    }

    private async Task ValidateReferencesAsync(ProductRequest request, int? productId, CancellationToken cancellationToken)
    {
        if (!await _db.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken))
        {
            throw new BadRequestException($"Category {request.CategoryId} doesn't exist.");
        }

        if (request.BrandId is not null && !await _db.Brands.AnyAsync(b => b.Id == request.BrandId, cancellationToken))
        {
            throw new BadRequestException($"Brand {request.BrandId} doesn't exist.");
        }

        var sku = request.Sku.Trim();
        if (await _db.Products.IgnoreQueryFilters().AnyAsync(p => p.Sku == sku && p.Id != productId, cancellationToken))
        {
            throw new ConflictException($"Another product already uses SKU '{sku}'.");
        }

        var attributeIds = request.Attributes.Select(a => a.AttributeId).Distinct().ToList();
        if (attributeIds.Count != request.Attributes.Count)
        {
            throw new BadRequestException("Each attribute can only be given once.");
        }

        var existing = await _db.ProductAttributes.CountAsync(a => attributeIds.Contains(a.Id), cancellationToken);
        if (existing != attributeIds.Count)
        {
            throw new BadRequestException("One or more attributes don't exist.");
        }
    }

    private Task<string> CreateUniqueSlugAsync(string name, int? productId, CancellationToken cancellationToken)
    {
        return SlugHelper.CreateUniqueAsync(name,
            slug => _db.Products.IgnoreQueryFilters().AnyAsync(p => p.Slug == slug && p.Id != productId, cancellationToken));
    }

    private static void SetAttributes(Product product, IEnumerable<ProductAttributeValueRequest> attributes)
    {
        product.AttributeValues.Clear();
        foreach (var attribute in attributes)
        {
            product.AttributeValues.Add(new ProductAttributeValue
            {
                ProductAttributeId = attribute.AttributeId,
                Value = attribute.Value.Trim()
            });
        }
    }

    /// <summary>Adds a staff notification when stock crosses the low-stock line (rule in StockAlerts).</summary>
    private void AddStockNotificationIfNeeded(Product product, int previousQuantity)
    {
        var notification = StockAlerts.CreateIfNeeded(product, previousQuantity);
        if (notification is not null)
        {
            _db.Notifications.Add(notification);
        }
    }
}
