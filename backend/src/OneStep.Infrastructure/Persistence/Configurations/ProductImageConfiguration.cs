using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneStep.Domain.Entities;

namespace OneStep.Infrastructure.Persistence.Configurations;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("ProductImages");

        builder.Property(i => i.FilePath).HasMaxLength(300);
        builder.Property(i => i.AltText).HasMaxLength(200);

        builder.HasIndex(i => new { i.ProductId, i.SortOrder }, "IX_ProductImages_ProductId_SortOrder");

        // At most ONE main image per product.
        builder.HasIndex(i => i.ProductId, "UQ_ProductImages_ProductId_Main")
            .IsUnique()
            .HasFilter("[IsMain] = 1");

        builder.HasOne(i => i.Product)
            .WithMany(p => p.Images)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
