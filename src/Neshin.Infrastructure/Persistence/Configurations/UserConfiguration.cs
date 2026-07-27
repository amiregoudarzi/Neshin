using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neshin.Domain.Identity;

namespace Neshin.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", PersistenceSchema.Application)
            .HasKey(user => user.Id);

        builder.HasIndex(user => user.PhoneNumber).IsUnique();

        builder.Property(user => user.Id)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("id");

        builder.Property(user => user.PhoneNumber)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("phone_number")
            .HasMaxLength(11)
            .IsRequired();

        builder.Property(user => user.IsPhoneNumberVerified)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("is_phone_number_verified");

        builder.Property(user => user.CreatedAtUtc)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("created_at_utc");

        builder.Property(user => user.PhoneNumberVerifiedAtUtc)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("phone_number_verified_at_utc")
            .IsRequired(false);
    }
}
