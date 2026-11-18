using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneStep.Domain.Entities;

namespace OneStep.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories", t =>
            t.HasCheckConstraint("CK_Categories_NotOwnParent", "[ParentId] IS NULL OR [ParentId] <> [Id]"));

        builder.Property(c => c.Name).HasMaxLength(100);
        builder.Property(c => c.Slug).HasMaxLength(120);
        builder.Property(c => c.Description).HasMaxLength(1000);
        builder.Property(c => c.ImagePath).HasMaxLength(300);

        builder.HasIndex(c => c.Slug, "UQ_Categories_Slug").IsUnique();

        // Two subcategories of the same parent can't share a name.
        // HasFilter(null) removes EF's automatic "ParentId IS NOT NULL" filter,
        // so top-level categories (ParentId = NULL) can't share a name either.
        builder.HasIndex(c => new { c.ParentId, c.Name }, "UQ_Categories_ParentId_Name")
            .IsUnique()
            .HasFilter(null);

        // Self-referencing relationship: a category's parent is another category.
        // Restrict = you can't delete a category that still has subcategories.
        builder.HasOne(c => c.Parent)
            .WithMany(c => c.Children)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
