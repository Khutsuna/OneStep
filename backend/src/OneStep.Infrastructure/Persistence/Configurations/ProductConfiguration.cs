using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneStep.Domain.Entities;

namespace OneStep.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", t =>
        {
            t.HasCheckConstraint("CK_Products_Price", "[Price] >= 0");
            t.HasCheckConstraint("CK_Products_StockQuantity", "[StockQuantity] >= 0");
            t.HasCheckConstraint("CK_Products_LowStockThreshold", "[LowStockThreshold] >= 0");
            t.HasCheckConstraint("CK_Products_Status", "[Status] IN ('Active', 'Inactive')");

            // Tells EF a trigger exists on this table (added in database/05_triggers.sql),
            // so EF generates SQL that works with it.
            t.HasTrigger("trg_Products_PriceHistory");
        });

        builder.Property(p => p.Name).HasMaxLength(200);
        builder.Property(p => p.Slug).HasMaxLength(220);
        builder.Property(p => p.Sku).HasMaxLength(50);
        // Description has no max length, so it becomes nvarchar(max).
        builder.Property(p => p.Price).HasPrecision(18, 2);
        builder.Property(p => p.LowStockThreshold).HasDefaultValue(5);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);

        // rowversion column: SQL Server changes it on every update (concurrency check).
        builder.Property(p => p.RowVersion).IsRowVersion();

        // Soft delete: every query automatically skips deleted products.
        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.HasIndex(p => p.Sku, "UQ_Products_Sku").IsUnique();
        builder.HasIndex(p => p.Slug, "UQ_Products_Slug").IsUnique();
        builder.HasIndex(p => p.CategoryId, "IX_Products_CategoryId")
            .IncludeProperties(p => new { p.Price, p.Status });
        builder.HasIndex(p => p.Price, "IX_Products_Price");
        builder.HasIndex(p => p.Name, "IX_Products_Name");

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Brand)
            .WithMany(b => b.Products)
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
