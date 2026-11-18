using OneStep.Domain.Common;
using OneStep.Domain.Enums;

namespace OneStep.Domain.Entities;

/// <summary>
/// A discount campaign. It is active only between StartsAt and EndsAt
/// (this is the "scheduled activation"), and only while IsEnabled is true.
/// It targets all products, or specific products and/or categories.
/// </summary>
public class Campaign : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? BannerImagePath { get; set; }

    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }                  // 10 = 10% or 10 GEL

    public bool AppliesToAllProducts { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public bool IsEnabled { get; set; } = true;

    public ICollection<CampaignProduct> Products { get; set; } = new List<CampaignProduct>();
    public ICollection<CampaignCategory> Categories { get; set; } = new List<CampaignCategory>();
}
