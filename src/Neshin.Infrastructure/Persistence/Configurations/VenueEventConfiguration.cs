using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neshin.Domain.Clients;

namespace Neshin.Infrastructure.Persistence.Configurations;

internal sealed class VenueEventConfiguration : IEntityTypeConfiguration<VenueEvent>
{
    public void Configure(EntityTypeBuilder<VenueEvent> builder)
    {
        builder.ToTable("venue_events", PersistenceSchema.Application).HasKey(venueEvent => venueEvent.Id);
        builder.HasIndex(venueEvent => new
        {
            venueEvent.BranchId,
            venueEvent.IsPublished,
            venueEvent.StartsAtUtc,
            venueEvent.EndsAtUtc
        });

        builder.Property(venueEvent => venueEvent.Id).HasColumnName("id");
        builder.Property(venueEvent => venueEvent.BranchId).HasColumnName("branch_id");
        builder.Property(venueEvent => venueEvent.Title).HasColumnName("title").HasMaxLength(300).IsRequired();
        builder.Property(venueEvent => venueEvent.Description).HasColumnName("description").HasMaxLength(2000);
        builder.Property(venueEvent => venueEvent.ImageUrl).HasColumnName("image_url").HasMaxLength(2000);
        builder.Property(venueEvent => venueEvent.StartsAtUtc).HasColumnName("starts_at_utc");
        builder.Property(venueEvent => venueEvent.EndsAtUtc).HasColumnName("ends_at_utc");
        builder.Property(venueEvent => venueEvent.IsPublished).HasColumnName("is_published");
        builder.Property(venueEvent => venueEvent.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne<Branch>()
            .WithMany()
            .HasForeignKey(venueEvent => venueEvent.BranchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
