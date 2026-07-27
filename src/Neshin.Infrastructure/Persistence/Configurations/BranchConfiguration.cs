using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neshin.Domain.Clients;

namespace Neshin.Infrastructure.Persistence.Configurations;

internal sealed class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("branches", PersistenceSchema.Application)
            .HasKey(branch => branch.Id);

        builder.HasIndex(branch => new { branch.Latitude, branch.Longitude });
        builder.HasIndex(branch => branch.ClientId);

        builder.Property(branch => branch.Id)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("id");

        builder.Property(branch => branch.ClientId)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("client_id");

        builder.Property(branch => branch.Name)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(branch => branch.Latitude)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("latitude")
            .HasPrecision(9, 6);

        builder.Property(branch => branch.Longitude)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("longitude")
            .HasPrecision(9, 6);

        builder.Property(branch => branch.IsActive)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("is_active");

        builder.Property(branch => branch.AcceptsAppOrders)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("accepts_app_orders");

        builder.Property(branch => branch.AllowsPayAtVenue)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("allows_pay_at_venue");

        builder.Property(branch => branch.Description)
            .HasColumnName("description")
            .HasMaxLength(2000);

        builder.Property(branch => branch.Address)
            .HasColumnName("address")
            .HasMaxLength(500);

        builder.Property(branch => branch.PublicPhoneNumber)
            .HasColumnName("public_phone_number")
            .HasMaxLength(30);

        builder.Property(branch => branch.LogoUrl)
            .HasColumnName("logo_url")
            .HasMaxLength(2000);

        builder.Property(branch => branch.CoverImageUrl)
            .HasColumnName("cover_image_url")
            .HasMaxLength(2000);

        builder.Property(branch => branch.CreatedAtUtc)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("created_at_utc")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne<Client>()
            .WithMany()
            .HasForeignKey(branch => branch.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
