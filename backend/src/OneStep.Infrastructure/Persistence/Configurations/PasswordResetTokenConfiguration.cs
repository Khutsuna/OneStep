using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneStep.Domain.Entities;

namespace OneStep.Infrastructure.Persistence.Configurations;

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("PasswordResetTokens", t =>
            t.HasCheckConstraint("CK_PasswordResetTokens_Expiry", "[ExpiresAt] > [CreatedAt]"));

        builder.Property(p => p.TokenHash).HasMaxLength(64).IsFixedLength().IsUnicode(false);
        builder.HasIndex(p => p.TokenHash, "UQ_PasswordResetTokens_TokenHash").IsUnique();

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
