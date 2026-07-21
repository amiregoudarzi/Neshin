using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neshin.Domain.Clients;

namespace Neshin.Infrastructure.Persistence.Configurations;

internal sealed class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("clients", "clients");
        builder.HasKey(client => client.Id);
        builder.Property(client => client.Name).HasMaxLength(200).IsRequired();
    }
}
