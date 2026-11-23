namespace OneStep.Application.Features.Checkout;

/// <summary>
/// Calculates order totals: subtotal, discounts, shipping (free above a threshold), 18% VAT and total.
/// Pure maths with no database, so it can be unit tested (see CheckoutCalculatorTests).
/// </summary>
public static class CheckoutCalculator
{
    public const decimal TaxRate = 0.18m;

    public static OrderTotals Calculate(
        IReadOnlyCollection<CheckoutLine> lines, decimal shippingPrice, decimal? freeShippingFrom)
    {
        // 1. The price of everything before discounts, and the total discount.
        var subtotal = lines.Sum(line => line.UnitPrice * line.Quantity);
        var discountTotal = lines.Sum(line => line.UnitDiscount * line.Quantity);

        // 2. What the goods cost after discounts.
        var goodsTotal = subtotal - discountTotal;

        // 3. Shipping is free when the goods reach the threshold (only if the delivery method has one).
        var shippingCost = freeShippingFrom is not null && goodsTotal >= freeShippingFrom
            ? 0m
            : shippingPrice;

        // 4. 18% VAT on the goods (not on shipping), rounded to cents.
        //    AwayFromZero is normal school rounding: 0.045 -> 0.05.
        var taxAmount = Math.Round(goodsTotal * TaxRate, 2, MidpointRounding.AwayFromZero);

        // 5. The total. The database checks this same formula (CK_Orders_Total).
        var totalAmount = goodsTotal + shippingCost + taxAmount;

        return new OrderTotals(subtotal, discountTotal, shippingCost, taxAmount, totalAmount);
    }
}