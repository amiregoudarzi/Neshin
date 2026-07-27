using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neshin.Domain.Ordering;

namespace Neshin.Infrastructure.Persistence.Configurations;

internal sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices", PersistenceSchema.Application).HasKey(invoice => invoice.Id);
        builder.HasIndex(invoice => new { invoice.CustomerId, invoice.CreatedAtUtc });

        builder.Property(invoice => invoice.Id).HasColumnName("id");
        builder.Property(invoice => invoice.BranchId).HasColumnName("branch_id");
        builder.Property(invoice => invoice.CustomerId).HasColumnName("customer_id");
        builder.Property(invoice => invoice.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30);
        builder.Property(invoice => invoice.TotalAmount)
            .HasColumnName("total_amount")
            .HasPrecision(18, 0);
        builder.Property(invoice => invoice.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp without time zone");
        builder.Property(invoice => invoice.PaidAtUtc)
            .HasColumnName("paid_at_utc")
            .HasColumnType("timestamp without time zone");

        builder.HasOne<Neshin.Domain.Clients.Branch>()
            .WithMany()
            .HasForeignKey(invoice => invoice.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Neshin.Domain.Customers.CustomerProfile>()
            .WithMany()
            .HasForeignKey(invoice => invoice.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(invoice => invoice.Items)
            .WithOne()
            .HasForeignKey(item => item.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.ToTable("invoice_items", PersistenceSchema.Application).HasKey(item => item.Id);
        builder.Property(item => item.Id).HasColumnName("id");
        builder.Property(item => item.InvoiceId).HasColumnName("invoice_id");
        builder.Property(item => item.MenuItemId).HasColumnName("menu_item_id");
        builder.Property(item => item.Title).HasColumnName("title").HasMaxLength(300);
        builder.Property(item => item.UnitPrice).HasColumnName("unit_price").HasPrecision(18, 0);
        builder.Property(item => item.Quantity).HasColumnName("quantity");
        builder.Property(item => item.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp without time zone");
    }
}
