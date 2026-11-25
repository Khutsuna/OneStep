using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneStep.Domain.Entities;

namespace OneStep.Infrastructure.Persistence.Configurations;

public class ProductDocumentConfiguration : IEntityTypeConfiguration<ProductDocument>
{
    public void Configure(EntityTypeBuilder<ProductDocument> builder)
    {
        builder.ToTable("ProductDocuments", t =>
            t.HasCheckConstraint("CK_ProductDocuments_SizeBytes", "[SizeBytes] > 0"));

        builder.Property(d => d.Title).HasMaxLength(150);
        builder.Property(d => d.FileName).HasMaxLength(255);
        builder.Property(d => d.FilePath).HasMaxLength(300);

        // The product page loads a product's documents by ProductId.
        builder.HasIndex(d => d.ProductId, "IX_ProductDocuments_ProductId");

        builder.HasOne(d => d.Product)
            .WithMany(p => p.Documents)
            .HasForeignKey(d => d.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
