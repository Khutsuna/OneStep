using System.ComponentModel.DataAnnotations;

namespace OneStep.Application.Features.Cart;

public record CartItemDto(
    int Id,
    int ProductId,
    string Name,
    string? ImageUrl,
    decimal UnitPrice,
    decimal UnitFinalPrice,
    int Quantity,
    int StockQuantity,
    decimal LineTotal);

/// <summary>The cart with current prices. Shipping and VAT are added at checkout.</summary>
public record CartDto(
    IReadOnlyList<CartItemDto> Items,
    int ItemCount,
    decimal Subtotal,
    decimal DiscountTotal,
    decimal Total);

public class AddCartItemRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, 99)]
    public int Quantity { get; set; } = 1;
}

public class UpdateCartItemRequest
{
    [Range(1, 99)]
    public int Quantity { get; set; }
}
