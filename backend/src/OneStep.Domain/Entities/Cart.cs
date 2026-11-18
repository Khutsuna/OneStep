using OneStep.Domain.Common;

namespace OneStep.Domain.Entities;

/// <summary>Each customer has at most one cart (a one-to-one relationship with User).</summary>
public class Cart : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
