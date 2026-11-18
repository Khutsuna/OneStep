using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneStep.Domain.Entities;

namespace OneStep.Infrastructure.Persistence.Configurations;

public class ProductPriceHistoryConfiguration : IEntityTypeConfiguration<ProductPriceHistory>
{
    public void Configure(EntityTypeBuilder<ProductPriceHistory> builder)
    {
        builder.ToTable("ProductPriceHistory");

        builder.Property(h => h.OldPrice).HasPrecision(18, 2);
        builder.Property(h => h.NewPrice).HasPrecision(18, 2);

        builder.HasIndex(h => new { h.ProductId, h.CreatedAt }, "IX_ProductPriceHistory_ProductId_CreatedAt");

        builder.HasOne(h => h.Product)
            .WithMany()
            .HasForeignKey(h => h.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
