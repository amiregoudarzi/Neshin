using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neshin.Domain.Clients;

namespace Neshin.Infrastructure.Persistence.Configurations;

internal sealed class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("clients", PersistenceSchema.Application)
            .HasKey(client => client.Id);

        builder.Property(client => client.Id)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("id");

        builder.Property(client => client.Name)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(client => client.IsActive)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("is_active");

        builder.Property(client => client.CreatedAtUtc)
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasColumnName("created_at_utc")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
