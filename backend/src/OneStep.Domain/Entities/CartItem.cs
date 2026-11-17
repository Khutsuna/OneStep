using OneStep.Domain.Common;

namespace OneStep.Domain.Entities;

/// <summary>
/// A product in a cart. There is no price here on purpose:
/// the cart always shows the product's current price.
/// </summary>
public class CartItem : BaseEntity
{
    public int CartId { get; set; }
    public Cart Cart { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }
}
