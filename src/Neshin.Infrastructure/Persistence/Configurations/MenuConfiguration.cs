using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neshin.Domain.Catalog;

namespace Neshin.Infrastructure.Persistence.Configurations;

internal sealed class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> builder)
    {
        builder.ToTable("menus", PersistenceSchema.Application)
            .HasKey(menu => menu.Id);

        builder.HasIndex(menu => menu.BranchId);

        builder.Property(menu => menu.Id)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("id");

        builder.Property(menu => menu.BranchId)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("branch_id");

        builder.Property(menu => menu.Name)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(menu => menu.IsPublished)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("is_published");

        builder.Property(menu => menu.CreatedAtUtc)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("created_at_utc")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne<Neshin.Domain.Clients.Branch>()
            .WithMany()
            .HasForeignKey(menu => menu.BranchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
