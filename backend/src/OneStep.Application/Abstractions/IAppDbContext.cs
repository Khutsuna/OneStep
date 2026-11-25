using Microsoft.EntityFrameworkCore;
using OneStep.Domain.Entities;
using OneStep.Domain.ReadModels;

namespace OneStep.Application.Abstractions;

/// <summary>
/// The database, as the Application layer sees it.
/// Application can't reference Infrastructure (dependencies point inward),
/// so it defines this interface and Infrastructure's AppDbContext implements it.
/// Services ask for IAppDbContext; dependency injection hands them the real AppDbContext.
/// </summary>
public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<PasswordResetToken> PasswordResetTokens { get; }
    DbSet<Address> Addresses { get; }
    DbSet<Category> Categories { get; }
    DbSet<Brand> Brands { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductImage> ProductImages { get; }
    DbSet<ProductDocument> ProductDocuments { get; }
    DbSet<ProductAttribute> ProductAttributes { get; }
    DbSet<ProductAttributeValue> ProductAttributeValues { get; }
    DbSet<ProductPriceHistory> ProductPriceHistory { get; }
    DbSet<Review> Reviews { get; }
    DbSet<WishlistItem> WishlistItems { get; }
    DbSet<Cart> Carts { get; }
    DbSet<CartItem> CartItems { get; }
    DbSet<DeliveryMethod> DeliveryMethods { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<OrderStatusHistory> OrderStatusHistory { get; }
    DbSet<Campaign> Campaigns { get; }
    DbSet<CampaignProduct> CampaignProducts { get; }
    DbSet<CampaignCategory> CampaignCategories { get; }
    DbSet<Notification> Notifications { get; }

    /// <summary>Read-only rows from the SQL view dbo.vw_ProductCatalog.</summary>
    DbSet<ProductCatalogEntry> ProductCatalog { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
