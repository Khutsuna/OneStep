namespace OneStep.Application.Features.Checkout;

/// <summary>One cart line as the calculator sees it: prices per unit and how many.</summary>
public record CheckoutLine(decimal UnitPrice, decimal UnitDiscount, int Quantity);

/// <summary>The money breakdown of an order. Also returned to the client as the checkout summary.</summary>
public record OrderTotals(
    decimal Subtotal,
    decimal DiscountTotal,
    decimal ShippingCost,
    decimal TaxAmount,
    decimal TotalAmount);
