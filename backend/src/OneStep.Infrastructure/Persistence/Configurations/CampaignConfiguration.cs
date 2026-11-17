using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneStep.Domain.Entities;

namespace OneStep.Infrastructure.Persistence.Configurations;

public class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.ToTable("Campaigns", t =>
        {
            t.HasCheckConstraint("CK_Campaigns_DiscountType", "[DiscountType] IN ('Percentage', 'FixedAmount')");
            t.HasCheckConstraint("CK_Campaigns_DiscountValue",
                "[DiscountValue] > 0 AND ([DiscountType] <> 'Percentage' OR [DiscountValue] <= 100)");
            t.HasCheckConstraint("CK_Campaigns_Dates", "[EndsAt] > [StartsAt]");
        });

        builder.Property(c => c.Name).HasMaxLength(150);
        builder.Property(c => c.Description).HasMaxLength(1000);
        builder.Property(c => c.BannerImagePath).HasMaxLength(300);
        builder.Property(c => c.DiscountType).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.DiscountValue).HasPrecision(18, 2);
        builder.Property(c => c.IsEnabled).HasDefaultValue(true);

        // Finds the currently active campaigns quickly.
        builder.HasIndex(c => new { c.IsEnabled, c.StartsAt, c.EndsAt }, "IX_Campaigns_Active");
    }
}
