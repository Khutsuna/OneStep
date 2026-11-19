using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneStep.Domain.ReadModels;

namespace OneStep.Infrastructure.Persistence.Configurations;

public class ProductCatalogEntryConfiguration : IEntityTypeConfiguration<ProductCatalogEntry>
{
    public void Configure(EntityTypeBuilder<ProductCatalogEntry> builder)
    {
        // Keyless + ToView: EF reads from the view and never tries to create or update it.
        // The view itself is created by database/03_views.sql.
        builder.HasNoKey();
        builder.ToView("vw_ProductCatalog");

        builder.Property(p => p.Price).HasPrecision(18, 2);
        builder.Property(p => p.AverageRating).HasPrecision(3, 2);
        builder.Property(p => p.Status).HasConversion<string>();
    }
}
