using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneStep.Domain.Entities;

namespace OneStep.Infrastructure.Persistence.Configurations;

public class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("Brands");

        builder.Property(b => b.Name).HasMaxLength(100);
        builder.Property(b => b.Slug).HasMaxLength(120);
        builder.Property(b => b.LogoPath).HasMaxLength(300);

        builder.HasIndex(b => b.Name, "UQ_Brands_Name").IsUnique();
        builder.HasIndex(b => b.Slug, "UQ_Brands_Slug").IsUnique();
    }
}
