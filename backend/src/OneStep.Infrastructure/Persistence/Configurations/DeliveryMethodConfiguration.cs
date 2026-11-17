using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneStep.Domain.Entities;

namespace OneStep.Infrastructure.Persistence.Configurations;

public class DeliveryMethodConfiguration : IEntityTypeConfiguration<DeliveryMethod>
{
    public void Configure(EntityTypeBuilder<DeliveryMethod> builder)
    {
        builder.ToTable("DeliveryMethods", t =>
        {
            t.HasCheckConstraint("CK_DeliveryMethods_Price", "[Price] >= 0");
            t.HasCheckConstraint("CK_DeliveryMethods_Days", "[MinDays] <= [MaxDays]");
        });

        builder.Property(d => d.Name).HasMaxLength(100);
        builder.Property(d => d.Description).HasMaxLength(300);
        builder.Property(d => d.Price).HasPrecision(18, 2);
        builder.Property(d => d.FreeOverAmount).HasPrecision(18, 2);
        builder.Property(d => d.IsActive).HasDefaultValue(true);

        builder.HasIndex(d => d.Name, "UQ_DeliveryMethods_Name").IsUnique();
    }
}
