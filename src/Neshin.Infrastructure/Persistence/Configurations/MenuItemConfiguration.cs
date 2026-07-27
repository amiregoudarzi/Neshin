using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neshin.Domain.Catalog;

namespace Neshin.Infrastructure.Persistence.Configurations;

internal sealed class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.ToTable("menu_items", PersistenceSchema.Application).HasKey(item => item.Id);
        builder.HasIndex(item => new { item.MenuId, item.IsAvailable, item.DisplayOrder });

        builder.Property(item => item.Id).HasColumnName("id");
        builder.Property(item => item.MenuId).HasColumnName("menu_id");
        builder.Property(item => item.Name).HasColumnName("name").HasMaxLength(300).IsRequired();
        builder.Property(item => item.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(item => item.CategoryName).HasColumnName("category_name").HasMaxLength(100);
        builder.Property(item => item.ImageUrl).HasColumnName("image_url").HasMaxLength(2000);
        builder.Property(item => item.Price).HasColumnName("price").HasPrecision(18, 0);
        builder.Property(item => item.IsAvailable).HasColumnName("is_available");
        builder.Property(item => item.DisplayOrder).HasColumnName("display_order");
        builder.Property(item => item.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne<Menu>()
            .WithMany()
            .HasForeignKey(item => item.MenuId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
