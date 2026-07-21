using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neshin.Domain.Clients;

namespace Neshin.Infrastructure.Persistence.Configurations;

internal sealed class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("branches", "clients");
        builder.HasKey(branch => branch.Id);
        builder.Property(branch => branch.Name).HasMaxLength(200).IsRequired();
        builder.Property(branch => branch.Latitude).HasPrecision(9, 6);
        builder.Property(branch => branch.Longitude).HasPrecision(9, 6);
        builder.HasIndex(branch => new { branch.Latitude, branch.Longitude });
        builder.HasIndex(branch => branch.ClientId);
    }
}
