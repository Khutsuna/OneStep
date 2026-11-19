namespace OneStep.Application.Features.Pricing;

/// <summary>A product's price before any discount, used as input for pricing.</summary>
public record PricingInput(int ProductId, int CategoryId, decimal Price);

/// <summary>The price a customer pays right now.</summary>
public record ProductPrice(decimal Price, decimal Discount, string? CampaignName)
{
    public decimal FinalPrice => Price - Discount;
}
