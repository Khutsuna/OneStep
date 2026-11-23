using OneStep.Application.Features.Pricing;
using OneStep.Domain.Enums;

namespace OneStep.UnitTests.Pricing;

public class DiscountRulesTests
{
    [Fact]
    public void Percentage_discount_is_a_share_of_the_price()
    {
        Assert.Equal(69.80m, DiscountRules.CalculateDiscount(349.00m, DiscountType.Percentage, 20m));
    }

    [Fact]
    public void Percentage_discount_is_rounded_to_cents()
    {
        Assert.Equal(13.95m, DiscountRules.CalculateDiscount(139.49m, DiscountType.Percentage, 10m)); // 13.949
    }

    [Fact]
    public void Fixed_discount_is_taken_as_is()
    {
        Assert.Equal(100m, DiscountRules.CalculateDiscount(2899m, DiscountType.FixedAmount, 100m));
    }

    [Fact]
    public void Discount_never_exceeds_the_price()
    {
        Assert.Equal(39m, DiscountRules.CalculateDiscount(39m, DiscountType.FixedAmount, 100m));
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(100, 0)]
    public void No_discount_for_zero_price_or_zero_value(decimal price, decimal value)
    {
        Assert.Equal(0m, DiscountRules.CalculateDiscount(price, DiscountType.Percentage, value));
    }
}
