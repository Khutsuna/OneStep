using Microsoft.EntityFrameworkCore;
using OneStep.Application.Abstractions;

namespace OneStep.Application.Features.Pricing;

/// <summary>
/// Applies campaigns. A campaign is active only between StartsAt and EndsAt ("scheduled activation"),
/// so no background job is needed: the time check happens on every price lookup.
/// A campaign can target all products, specific products, or categories
/// (a campaign on "Audio" also covers its subcategories "Headphones" and "Speakers").
/// </summary>
public class PricingService : IPricingService
{
    private readonly IAppDbContext _db;

    public PricingService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyDictionary<int, ProductPrice>> GetPricesAsync(
        IReadOnlyCollection<PricingInput> products, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var campaigns = await _db.Campaigns
            .AsNoTracking()
            .Where(c => c.IsEnabled && c.StartsAt <= now && c.EndsAt > now)
            .Select(c => new
            {
                c.Name,
                c.DiscountType,
                c.DiscountValue,
                c.AppliesToAllProducts,
                ProductIds = c.Products.Select(cp => cp.ProductId).ToList(),
                CategoryIds = c.Categories.Select(cc => cc.CategoryId).ToList()
            })
            .ToListAsync(cancellationToken);

        // Category id -> parent id, to find every ancestor of a product's category.
        var parentOf = campaigns.Count == 0
            ? new Dictionary<int, int?>()
            : await _db.Categories.AsNoTracking().ToDictionaryAsync(c => c.Id, c => c.ParentId, cancellationToken);

        var result = new Dictionary<int, ProductPrice>();
        foreach (var product in products)
        {
            var categoryChain = GetCategoryChain(product.CategoryId, parentOf);
            decimal bestDiscount = 0m;
            string? bestCampaign = null;

            foreach (var campaign in campaigns)
            {
                var applies = campaign.AppliesToAllProducts
                    || campaign.ProductIds.Contains(product.ProductId)
                    || campaign.CategoryIds.Any(categoryChain.Contains);

                if (!applies)
                {
                    continue;
                }

                var discount = DiscountRules.CalculateDiscount(product.Price, campaign.DiscountType, campaign.DiscountValue);
                if (discount > bestDiscount)
                {
                    bestDiscount = discount;
                    bestCampaign = campaign.Name;
                }
            }

            result[product.ProductId] = new ProductPrice(product.Price, bestDiscount, bestCampaign);
        }

        return result;
    }

    /// <summary>The category itself plus all of its parents, e.g. Headphones -> Audio.</summary>
    private static HashSet<int> GetCategoryChain(int categoryId, IReadOnlyDictionary<int, int?> parentOf)
    {
        var chain = new HashSet<int>();
        int? current = categoryId;
        while (current is not null && chain.Add(current.Value))
        {
            current = parentOf.TryGetValue(current.Value, out var parent) ? parent : null;
        }
        return chain;
    }
}
