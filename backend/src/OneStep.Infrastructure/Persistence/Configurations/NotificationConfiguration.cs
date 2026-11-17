using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneStep.Domain.Entities;

namespace OneStep.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications", t =>
            t.HasCheckConstraint("CK_Notifications_Type", "[Type] IN ('LowStock', 'OutOfStock', 'NewOrder')"));

        builder.Property(n => n.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(n => n.Message).HasMaxLength(500);

        // Unread notifications, newest first (the admin bell).
        builder.HasIndex(n => new { n.IsRead, n.CreatedAt }, "IX_Notifications_IsRead_CreatedAt")
            .IsDescending(false, true);

        builder.HasOne(n => n.Product)
            .WithMany()
            .HasForeignKey(n => n.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(n => n.Order)
            .WithMany()
            .HasForeignKey(n => n.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
