using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neshin.Domain.Ordering;

namespace Neshin.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders", "ordering");
        builder.HasKey(order => order.Id);
        builder.Property(order => order.TotalAmount).HasPrecision(18, 0);
        builder.Property(order => order.PaymentMethod).HasConversion<string>().HasMaxLength(30);
        builder.Property(order => order.Status).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(order => new { order.BranchId, order.CreatedAtUtc });

        builder.HasMany(order => order.Items)
            .WithOne()
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items", "ordering");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Name).HasMaxLength(300).IsRequired();
        builder.Property(item => item.UnitPrice).HasPrecision(18, 0);
    }
}
