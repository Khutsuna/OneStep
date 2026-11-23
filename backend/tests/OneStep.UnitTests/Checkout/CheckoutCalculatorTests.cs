using OneStep.Application.Features.Checkout;

namespace OneStep.UnitTests.Checkout;

/// <summary>
/// Tests for the checkout maths. Each test follows Arrange (set up) - Act (run) - Assert (check).
/// Shipping in these tests: 5.00 GEL, free from 200.00 GEL (like "Standard courier" in the seed data).
/// </summary>
public class CheckoutCalculatorTests
{
    private const decimal Shipping = 5.00m;
    private const decimal FreeFrom = 200.00m;

    [Fact]
    public void Small_order_pays_shipping_and_18_percent_vat()
    {
        // Arrange: one product at 50.00, no discount
        var lines = new[] { new CheckoutLine(50.00m, 0m, 1) };

        // Act
        var totals = CheckoutCalculator.Calculate(lines, Shipping, FreeFrom);

        // Assert
        Assert.Equal(50.00m, totals.Subtotal);
        Assert.Equal(0m, totals.DiscountTotal);
        Assert.Equal(5.00m, totals.ShippingCost);
        Assert.Equal(9.00m, totals.TaxAmount);        // 18% of 50.00
        Assert.Equal(64.00m, totals.TotalAmount);     // 50 + 5 + 9
    }

    [Fact]
    public void Shipping_is_free_from_the_threshold()
    {
        var lines = new[] { new CheckoutLine(100.00m, 0m, 2) };   // exactly 200.00

        var totals = CheckoutCalculator.Calculate(lines, Shipping, FreeFrom);

        Assert.Equal(0m, totals.ShippingCost);
        Assert.Equal(236.00m, totals.TotalAmount);    // 200 + 0 + 36
    }

    [Fact]
    public void Free_shipping_threshold_uses_the_price_after_discounts()
    {
        // 210.00 before discount, but 190.00 after a 20.00 discount -> below 200, so shipping is charged
        var lines = new[] { new CheckoutLine(210.00m, 20.00m, 1) };

        var totals = CheckoutCalculator.Calculate(lines, Shipping, FreeFrom);

        Assert.Equal(20.00m, totals.DiscountTotal);
        Assert.Equal(5.00m, totals.ShippingCost);
    }

    [Fact]
    public void Vat_is_calculated_on_the_discounted_price()
    {
        // iPhone 16 with the 100 GEL "Phone Deals" discount
        var lines = new[] { new CheckoutLine(2899.00m, 100.00m, 1) };

        var totals = CheckoutCalculator.Calculate(lines, Shipping, FreeFrom);

        Assert.Equal(2899.00m, totals.Subtotal);
        Assert.Equal(100.00m, totals.DiscountTotal);
        Assert.Equal(503.82m, totals.TaxAmount);      // 18% of 2799.00
        Assert.Equal(3302.82m, totals.TotalAmount);   // 2799 + 0 + 503.82
    }

    [Fact]
    public void Several_lines_and_quantities_add_up()
    {
        var lines = new[]
        {
            new CheckoutLine(39.00m, 0m, 2),          // 78.00
            new CheckoutLine(349.00m, 69.80m, 1)      // 349.00 - 69.80
        };

        var totals = CheckoutCalculator.Calculate(lines, Shipping, FreeFrom);

        Assert.Equal(427.00m, totals.Subtotal);
        Assert.Equal(69.80m, totals.DiscountTotal);
        Assert.Equal(0m, totals.ShippingCost);        // 357.20 >= 200
        Assert.Equal(64.30m, totals.TaxAmount);       // 18% of 357.20 = 64.296 -> 64.30
        Assert.Equal(421.50m, totals.TotalAmount);
    }

    [Fact]
    public void Tax_is_rounded_to_two_decimals_away_from_zero()
    {
        var lines = new[] { new CheckoutLine(0.25m, 0m, 1) };   // 18% = 0.045

        var totals = CheckoutCalculator.Calculate(lines, 0m, null);

        Assert.Equal(0.05m, totals.TaxAmount);
    }

    [Fact]
    public void Without_a_threshold_shipping_is_always_charged()
    {
        var lines = new[] { new CheckoutLine(1000.00m, 0m, 1) };

        var totals = CheckoutCalculator.Calculate(lines, 12.00m, null);   // like "Express courier"

        Assert.Equal(12.00m, totals.ShippingCost);
    }

    [Fact]
    public void Total_always_equals_subtotal_minus_discount_plus_shipping_plus_tax()
    {
        // The same rule the database enforces with the CK_Orders_Total check constraint.
        var lines = new[] { new CheckoutLine(123.45m, 12.34m, 3), new CheckoutLine(9.99m, 0m, 7) };

        var t = CheckoutCalculator.Calculate(lines, Shipping, FreeFrom);

        Assert.Equal(t.Subtotal - t.DiscountTotal + t.ShippingCost + t.TaxAmount, t.TotalAmount);
    }
}
