using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neshin.Domain.Ordering;

namespace Neshin.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders", PersistenceSchema.Application)
            .HasKey(order => order.Id);

        builder.HasIndex(order => new { order.BranchId, order.CreatedAtUtc });

        builder.Property(order => order.Id)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("id");

        builder.Property(order => order.BranchId)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("branch_id");

        builder.Property(order => order.CustomerId)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("customer_id");

        builder.Property(order => order.UserId)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("user_id")
            .IsRequired(false);

        builder.Property(order => order.PaymentMethod)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("payment_method")
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(order => order.Status)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(order => order.TotalAmount)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("total_amount")
            .HasPrecision(18, 0);

        builder.Property(order => order.CreatedAtUtc)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("created_at_utc");

        builder.Property(order => order.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(order => order.CustomerDisplayName)
            .HasColumnName("customer_display_name")
            .HasMaxLength(100);

        builder.Property(order => order.ContactPhoneNumber)
            .HasColumnName("contact_phone_number")
            .HasMaxLength(30);

        builder.Property(order => order.AllowsPhoneContact).HasColumnName("allows_phone_contact");
        builder.Property(order => order.RejectionReason).HasColumnName("rejection_reason").HasMaxLength(500);
        builder.Property(order => order.SubmittedAtUtc).HasColumnName("submitted_at_utc");
        builder.Property(order => order.AcceptedAtUtc).HasColumnName("accepted_at_utc");
        builder.Property(order => order.ReadyAtUtc).HasColumnName("ready_at_utc");
        builder.Property(order => order.CompletedAtUtc).HasColumnName("completed_at_utc");
        builder.Property(order => order.RejectedAtUtc).HasColumnName("rejected_at_utc");
        builder.Property(order => order.Version).HasColumnName("version").IsConcurrencyToken();

        builder.HasIndex(order => new { order.CustomerId, order.IdempotencyKey }).IsUnique();
        builder.HasIndex(order => new { order.BranchId, order.Status, order.CreatedAtUtc });

        builder.HasOne<Neshin.Domain.Clients.Branch>()
            .WithMany()
            .HasForeignKey(order => order.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Neshin.Domain.Customers.CustomerProfile>()
            .WithMany()
            .HasForeignKey(order => order.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Neshin.Domain.Identity.User>()
            .WithMany()
            .HasForeignKey(order => order.UserId)
            .OnDelete(DeleteBehavior.Restrict);

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
        builder.ToTable("order_items", PersistenceSchema.Application)
            .HasKey(item => item.Id);

        builder.Property(item => item.Id)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("id");

        builder.Property(item => item.OrderId)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("order_id");

        builder.Property(item => item.MenuItemId)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("menu_item_id");

        builder.Property(item => item.Name)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("name")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(item => item.UnitPrice)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("unit_price")
            .HasPrecision(18, 0);

        builder.Property(item => item.Quantity)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("quantity");

        builder.Property(item => item.CreatedAtUtc)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("created_at_utc")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Menu item data is snapshotted on the order. No foreign key is used so
        // historical orders survive catalog cleanup or replacement.
    }
}
