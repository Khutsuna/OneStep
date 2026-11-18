using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneStep.Domain.Entities;

namespace OneStep.Infrastructure.Persistence.Configurations;

public class ProductAttributeValueConfiguration : IEntityTypeConfiguration<ProductAttributeValue>
{
    public void Configure(EntityTypeBuilder<ProductAttributeValue> builder)
    {
        builder.ToTable("ProductAttributeValues");

        // Composite primary key: one value per (product, attribute) pair.
        builder.HasKey(v => new { v.ProductId, v.ProductAttributeId });

        builder.Property(v => v.Value).HasMaxLength(100);

        // Speeds up filters such as "Color = Black".
        builder.HasIndex(v => new { v.ProductAttributeId, v.Value }, "IX_ProductAttributeValues_AttributeId_Value");

        builder.HasOne(v => v.Product)
            .WithMany(p => p.AttributeValues)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.ProductAttribute)
            .WithMany(a => a.Values)
            .HasForeignKey(v => v.ProductAttributeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
