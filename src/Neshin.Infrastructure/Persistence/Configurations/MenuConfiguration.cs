using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neshin.Domain.Catalog;

namespace Neshin.Infrastructure.Persistence.Configurations;

internal sealed class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> builder)
    {
        builder.ToTable("menus", "catalog");
        builder.HasKey(menu => menu.Id);
        builder.Property(menu => menu.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(menu => menu.BranchId);
    }
}
