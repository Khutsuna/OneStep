using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneStep.Domain.Entities;

namespace OneStep.Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems", t =>
        {
            t.HasCheckConstraint("CK_OrderItems_Quantity", "[Quantity] > 0");
            t.HasCheckConstraint("CK_OrderItems_UnitDiscount", "[UnitDiscount] >= 0 AND [UnitDiscount] <= [UnitPrice]");
        });

        builder.Property(i => i.ProductName).HasMaxLength(200);
        builder.Property(i => i.Sku).HasMaxLength(50);
        builder.Property(i => i.UnitPrice).HasPrecision(18, 2);
        builder.Property(i => i.UnitDiscount).HasPrecision(18, 2);

        // Computed by SQL Server and stored ("persisted"), so it can never be wrong.
        builder.Property(i => i.LineTotal)
            .HasPrecision(18, 2)
            .HasComputedColumnSql("([UnitPrice] - [UnitDiscount]) * [Quantity]", stored: true);

        builder.HasIndex(i => new { i.OrderId, i.ProductId }, "UQ_OrderItems_OrderId_ProductId").IsUnique();
        builder.HasIndex(i => i.ProductId, "IX_OrderItems_ProductId");

        builder.HasOne(i => i.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
