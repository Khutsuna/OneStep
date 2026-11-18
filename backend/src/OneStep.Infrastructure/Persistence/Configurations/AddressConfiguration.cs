using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneStep.Domain.Entities;

namespace OneStep.Infrastructure.Persistence.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Addresses", t =>
            t.HasCheckConstraint("CK_Addresses_CountryCode", "LEN([CountryCode]) = 2"));

        builder.Property(a => a.Label).HasMaxLength(50);
        builder.Property(a => a.RecipientName).HasMaxLength(150);
        builder.Property(a => a.Phone).HasMaxLength(30);
        builder.Property(a => a.Line1).HasMaxLength(200);
        builder.Property(a => a.Line2).HasMaxLength(200);
        builder.Property(a => a.City).HasMaxLength(100);
        builder.Property(a => a.Region).HasMaxLength(100);
        builder.Property(a => a.PostalCode).HasMaxLength(20);
        builder.Property(a => a.CountryCode).HasMaxLength(2).IsFixedLength().IsUnicode(false);

        builder.HasIndex(a => a.UserId, "IX_Addresses_UserId");

        // Filtered unique index: at most ONE default address per user.
        builder.HasIndex(a => a.UserId, "UQ_Addresses_UserId_Default")
            .IsUnique()
            .HasFilter("[IsDefault] = 1");

        builder.HasOne(a => a.User)
            .WithMany(u => u.Addresses)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
