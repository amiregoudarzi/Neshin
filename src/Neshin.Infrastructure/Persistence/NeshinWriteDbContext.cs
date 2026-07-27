using Microsoft.EntityFrameworkCore;
using Neshin.Domain.Catalog;
using Neshin.Domain.Clients;
using Neshin.Domain.Customers;
using Neshin.Domain.Identity;
using Neshin.Domain.Ordering;
using Neshin.Infrastructure.Persistence.Configurations;

namespace Neshin.Infrastructure.Persistence;

public sealed class NeshinWriteDbContext(DbContextOptions<NeshinWriteDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Client> Clients { get; set; } = null!;
    public DbSet<Branch> Branches { get; set; } = null!;
    public DbSet<Menu> Menus { get; set; } = null!;
    public DbSet<MenuItem> MenuItems { get; set; } = null!;
    public DbSet<VenueEvent> VenueEvents { get; set; } = null!;
    public DbSet<CustomerProfile> CustomerProfiles { get; set; } = null!;
    public DbSet<CustomerSession> CustomerSessions { get; set; } = null!;
    public DbSet<VenueVisit> VenueVisits { get; set; } = null!;
    public DbSet<BranchCustomer> BranchCustomers { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<Invoice> Invoices { get; set; } = null!;
    public DbSet<InvoiceItem> InvoiceItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        ApplyConfigurations(builder);
        base.OnModelCreating(builder);
    }

    internal static void ApplyConfigurations(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new UserConfiguration());
        builder.ApplyConfiguration(new ClientConfiguration());
        builder.ApplyConfiguration(new BranchConfiguration());
        builder.ApplyConfiguration(new MenuConfiguration());
        builder.ApplyConfiguration(new MenuItemConfiguration());
        builder.ApplyConfiguration(new VenueEventConfiguration());
        builder.ApplyConfiguration(new CustomerProfileConfiguration());
        builder.ApplyConfiguration(new CustomerSessionConfiguration());
        builder.ApplyConfiguration(new VenueVisitConfiguration());
        builder.ApplyConfiguration(new BranchCustomerConfiguration());
        builder.ApplyConfiguration(new OrderConfiguration());
        builder.ApplyConfiguration(new OrderItemConfiguration());
        builder.ApplyConfiguration(new InvoiceConfiguration());
        builder.ApplyConfiguration(new InvoiceItemConfiguration());
    }
}
