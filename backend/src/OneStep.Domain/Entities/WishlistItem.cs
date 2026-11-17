namespace OneStep.Domain.Entities;

/// <summary>
/// A product saved to a customer's wishlist. Each user has exactly one wishlist,
/// so there is no separate Wishlist table. Primary key: (UserId, ProductId).
/// </summary>
public class WishlistItem
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
