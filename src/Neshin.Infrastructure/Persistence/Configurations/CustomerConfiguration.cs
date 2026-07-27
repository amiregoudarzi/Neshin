using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neshin.Domain.Customers;

namespace Neshin.Infrastructure.Persistence.Configurations;

internal sealed class CustomerProfileConfiguration : IEntityTypeConfiguration<CustomerProfile>
{
    public void Configure(EntityTypeBuilder<CustomerProfile> builder)
    {
        builder.ToTable("customer_profiles", PersistenceSchema.Application).HasKey(customer => customer.Id);
        builder.HasIndex(customer => customer.UserId).IsUnique();

        builder.Property(customer => customer.Id).HasColumnName("id");
        builder.Property(customer => customer.UserId).HasColumnName("user_id").IsRequired(false);
        builder.Property(customer => customer.DisplayName).HasColumnName("display_name").HasMaxLength(100);
        builder.Property(customer => customer.ContactPhoneNumber).HasColumnName("contact_phone_number").HasMaxLength(30);
        builder.Property(customer => customer.IsPhoneNumberVerified).HasColumnName("is_phone_number_verified");
        builder.Property(customer => customer.CreatedAtUtc).HasColumnName("created_at_utc");

        builder.HasOne<Neshin.Domain.Identity.User>()
            .WithOne()
            .HasForeignKey<CustomerProfile>(customer => customer.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class CustomerSessionConfiguration : IEntityTypeConfiguration<CustomerSession>
{
    public void Configure(EntityTypeBuilder<CustomerSession> builder)
    {
        builder.ToTable("customer_sessions", PersistenceSchema.Application).HasKey(session => session.Id);
        builder.HasIndex(session => session.TokenHash).IsUnique();
        builder.HasIndex(session => new { session.CustomerId, session.ExpiresAtUtc });

        builder.Property(session => session.Id).HasColumnName("id");
        builder.Property(session => session.CustomerId).HasColumnName("customer_id");
        builder.Property(session => session.TokenHash).HasColumnName("token_hash").HasMaxLength(64).IsFixedLength();
        builder.Property(session => session.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(session => session.LastSeenAtUtc).HasColumnName("last_seen_at_utc");
        builder.Property(session => session.ExpiresAtUtc).HasColumnName("expires_at_utc");
        builder.Property(session => session.RevokedAtUtc).HasColumnName("revoked_at_utc").IsRequired(false);

        builder.HasOne<CustomerProfile>()
            .WithMany()
            .HasForeignKey(session => session.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class VenueVisitConfiguration : IEntityTypeConfiguration<VenueVisit>
{
    public void Configure(EntityTypeBuilder<VenueVisit> builder)
    {
        builder.ToTable("venue_visits", PersistenceSchema.Application).HasKey(visit => visit.Id);
        builder.HasIndex(visit => new { visit.BranchId, visit.LastSeenAtUtc });
        builder.HasIndex(visit => new { visit.CustomerId, visit.BranchId, visit.EndedAtUtc });

        builder.Property(visit => visit.Id).HasColumnName("id");
        builder.Property(visit => visit.BranchId).HasColumnName("branch_id");
        builder.Property(visit => visit.CustomerId).HasColumnName("customer_id");
        builder.Property(visit => visit.DistanceMeters).HasColumnName("distance_meters").HasPrecision(10, 2);
        builder.Property(visit => visit.AccuracyMeters).HasColumnName("accuracy_meters").HasPrecision(10, 2);
        builder.Property(visit => visit.StartedAtUtc).HasColumnName("started_at_utc");
        builder.Property(visit => visit.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(visit => visit.LastSeenAtUtc).HasColumnName("last_seen_at_utc");
        builder.Property(visit => visit.EndedAtUtc).HasColumnName("ended_at_utc").IsRequired(false);

        builder.HasOne<Neshin.Domain.Clients.Branch>()
            .WithMany()
            .HasForeignKey(visit => visit.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<CustomerProfile>()
            .WithMany()
            .HasForeignKey(visit => visit.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class BranchCustomerConfiguration : IEntityTypeConfiguration<BranchCustomer>
{
    public void Configure(EntityTypeBuilder<BranchCustomer> builder)
    {
        builder.ToTable("branch_customers", PersistenceSchema.Application).HasKey(customer => customer.Id);
        builder.HasIndex(customer => new { customer.BranchId, customer.CustomerId }).IsUnique();

        builder.Property(customer => customer.Id).HasColumnName("id");
        builder.Property(customer => customer.BranchId).HasColumnName("branch_id");
        builder.Property(customer => customer.CustomerId).HasColumnName("customer_id");
        builder.Property(customer => customer.Source).HasColumnName("source").HasMaxLength(30);
        builder.Property(customer => customer.Notes).HasColumnName("notes").HasMaxLength(1000);
        builder.Property(customer => customer.ContactPhoneNumber)
            .HasColumnName("contact_phone_number")
            .HasMaxLength(30);
        builder.Property(customer => customer.IsArchived).HasColumnName("is_archived");
        builder.Property(customer => customer.AddedAtUtc).HasColumnName("added_at_utc");
        builder.Property(customer => customer.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(customer => customer.ArchivedAtUtc).HasColumnName("archived_at_utc").IsRequired(false);

        builder.HasOne<Neshin.Domain.Clients.Branch>()
            .WithMany()
            .HasForeignKey(customer => customer.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<CustomerProfile>()
            .WithMany()
            .HasForeignKey(customer => customer.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
