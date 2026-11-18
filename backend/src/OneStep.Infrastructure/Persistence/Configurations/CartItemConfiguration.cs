using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneStep.Domain.Entities;

namespace OneStep.Infrastructure.Persistence.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems", t =>
            t.HasCheckConstraint("CK_CartItems_Quantity", "[Quantity] > 0"));

        // Adding the same product again raises the quantity instead of creating a second row.
        builder.HasIndex(i => new { i.CartId, i.ProductId }, "UQ_CartItems_CartId_ProductId").IsUnique();
        builder.HasIndex(i => i.ProductId, "IX_CartItems_ProductId");

        builder.HasOne(i => i.Cart)
            .WithMany(c => c.Items)
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
