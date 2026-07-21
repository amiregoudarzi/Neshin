using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neshin.Domain.Identity;

namespace Neshin.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", "identity");
        builder.HasKey(user => user.Id);
        builder.Property(user => user.PhoneNumber).HasMaxLength(11).IsRequired();
        builder.HasIndex(user => user.PhoneNumber).IsUnique();
    }
}
