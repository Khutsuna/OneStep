using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneStep.Domain.Entities;

namespace OneStep.Infrastructure.Persistence.Configurations;

public class CampaignProductConfiguration : IEntityTypeConfiguration<CampaignProduct>
{
    public void Configure(EntityTypeBuilder<CampaignProduct> builder)
    {
        builder.ToTable("CampaignProducts");

        builder.HasKey(cp => new { cp.CampaignId, cp.ProductId });
        builder.HasIndex(cp => cp.ProductId, "IX_CampaignProducts_ProductId");

        builder.HasOne(cp => cp.Campaign)
            .WithMany(c => c.Products)
            .HasForeignKey(cp => cp.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cp => cp.Product)
            .WithMany()
            .HasForeignKey(cp => cp.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
