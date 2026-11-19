namespace OneStep.Application.Features.Pricing;

public interface IPricingService
{
    /// <summary>Current prices (after the best active campaign) for many products at once, keyed by product id.</summary>
    Task<IReadOnlyDictionary<int, ProductPrice>> GetPricesAsync(
        IReadOnlyCollection<PricingInput> products, CancellationToken cancellationToken);
}
