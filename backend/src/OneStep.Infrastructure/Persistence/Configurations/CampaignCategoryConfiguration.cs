using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneStep.Domain.Entities;

namespace OneStep.Infrastructure.Persistence.Configurations;

public class CampaignCategoryConfiguration : IEntityTypeConfiguration<CampaignCategory>
{
    public void Configure(EntityTypeBuilder<CampaignCategory> builder)
    {
        builder.ToTable("CampaignCategories");

        builder.HasKey(cc => new { cc.CampaignId, cc.CategoryId });
        builder.HasIndex(cc => cc.CategoryId, "IX_CampaignCategories_CategoryId");

        builder.HasOne(cc => cc.Campaign)
            .WithMany(c => c.Categories)
            .HasForeignKey(cc => cc.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cc => cc.Category)
            .WithMany()
            .HasForeignKey(cc => cc.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
