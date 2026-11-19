using OneStep.Domain.Enums;

namespace OneStep.Application.Features.Pricing;

/// <summary>
/// Pure discount maths, with no database: easy to unit test.
/// Rule from the plan: campaigns never stack, the single biggest discount wins.
/// </summary>
public static class DiscountRules
{
    /// <summary>The discount one campaign gives on one unit, never more than the price itself.</summary>
    public static decimal CalculateDiscount(decimal price, DiscountType type, decimal value)
    {
        if (price <= 0 || value <= 0)
        {
            return 0m;
        }

        var discount = type == DiscountType.Percentage
            ? Math.Round(price * value / 100m, 2, MidpointRounding.AwayFromZero)
            : value;

        return Math.Min(discount, price);
    }
}
