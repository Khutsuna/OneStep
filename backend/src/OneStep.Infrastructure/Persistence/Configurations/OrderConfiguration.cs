using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneStep.Domain.Entities;

namespace OneStep.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders", t =>
        {
            t.HasCheckConstraint("CK_Orders_Status",
                "[Status] IN ('Pending', 'Paid', 'Shipped', 'Delivered', 'Cancelled')");
            t.HasCheckConstraint("CK_Orders_PaymentMethod", "[PaymentMethod] IN ('CashOnDelivery', 'Card')");
            t.HasCheckConstraint("CK_Orders_Amounts",
                "[Subtotal] >= 0 AND [DiscountTotal] >= 0 AND [ShippingCost] >= 0 AND [TaxAmount] >= 0");
            // The database itself guarantees the total always adds up.
            t.HasCheckConstraint("CK_Orders_Total",
                "[TotalAmount] = [Subtotal] - [DiscountTotal] + [ShippingCost] + [TaxAmount]");
        });

        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.PaymentMethod).HasConversion<string>().HasMaxLength(30);
        builder.Property(o => o.Subtotal).HasPrecision(18, 2);
        builder.Property(o => o.DiscountTotal).HasPrecision(18, 2);
        builder.Property(o => o.ShippingCost).HasPrecision(18, 2);
        builder.Property(o => o.TaxAmount).HasPrecision(18, 2);
        builder.Property(o => o.TotalAmount).HasPrecision(18, 2);
        builder.Property(o => o.CustomerNote).HasMaxLength(500);

        // Complex type: ShippingAddress is not its own table; its fields become
        // columns of Orders (ShipRecipientName, ShipCity, ...).
        builder.ComplexProperty(o => o.ShippingAddress, a =>
        {
            a.Property(x => x.RecipientName).HasColumnName("ShipRecipientName").HasMaxLength(150);
            a.Property(x => x.Phone).HasColumnName("ShipPhone").HasMaxLength(30);
            a.Property(x => x.Line1).HasColumnName("ShipLine1").HasMaxLength(200);
            a.Property(x => x.Line2).HasColumnName("ShipLine2").HasMaxLength(200);
            a.Property(x => x.City).HasColumnName("ShipCity").HasMaxLength(100);
            a.Property(x => x.Region).HasColumnName("ShipRegion").HasMaxLength(100);
            a.Property(x => x.PostalCode).HasColumnName("ShipPostalCode").HasMaxLength(20);
            a.Property(x => x.CountryCode).HasColumnName("ShipCountryCode").HasMaxLength(2).IsUnicode(false);
        });

        // Customer's order history, newest first.
        builder.HasIndex(o => new { o.UserId, o.CreatedAt }, "IX_Orders_UserId_CreatedAt")
            .IsDescending(false, true);
        // Admin order list filtered by status.
        builder.HasIndex(o => new { o.Status, o.CreatedAt }, "IX_Orders_Status_CreatedAt");
        // Sales reports by date.
        builder.HasIndex(o => o.CreatedAt, "IX_Orders_CreatedAt")
            .IncludeProperties(o => new { o.TotalAmount, o.Status });

        // Restrict: a user or delivery method with orders can't be deleted (history must survive).
        builder.HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.DeliveryMethod)
            .WithMany()
            .HasForeignKey(o => o.DeliveryMethodId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
