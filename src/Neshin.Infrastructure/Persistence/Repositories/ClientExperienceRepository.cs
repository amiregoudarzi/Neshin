using Microsoft.EntityFrameworkCore;
using Neshin.Application.Abstractions.Persistence;
using Neshin.Application.Common;
using Neshin.Domain.Catalog;
using Neshin.Domain.Clients;
using Neshin.Domain.Common;
using Neshin.Domain.Customers;
using Neshin.Domain.Ordering;
using Neshin.Infrastructure.CustomerExperience;

namespace Neshin.Infrastructure.Persistence.Repositories;

public sealed class ClientExperienceRepository(
    NeshinWriteDbContext writeDbContext,
    NeshinReadDbContext readDbContext,
    TimeProvider timeProvider) : IClientExperienceRepository
{
    private static readonly OrderStatus[] SellingStatuses =
        [OrderStatus.Accepted, OrderStatus.Preparing, OrderStatus.Ready, OrderStatus.Completed];

    public async Task<CafeRegistrationResult> RegisterCafeAsync(
        string clientName,
        string cafeName,
        decimal latitude,
        decimal longitude,
        string? description,
        string? address,
        string? publicPhoneNumber,
        IReadOnlyList<string>? photoUrls,
        CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var client = Client.Create(clientName, now);
        client.Activate();
        var cafe = Branch.Create(client.Id, cafeName, latitude, longitude, now);
        cafe.Activate();
        cafe.SetAppOrdering(true);
        cafe.SetPayAtVenue(true);
        var managementKey = TokenHashing.CreateToken();
        cafe.SetManagementKeyHash(TokenHashing.Hash(managementKey));
        cafe.SetPhotos(photoUrls);
        cafe.UpdatePublicProfile(
            description,
            address,
            publicPhoneNumber,
            photoUrls?.FirstOrDefault(),
            photoUrls?.Skip(1).FirstOrDefault());

        writeDbContext.Clients.Add(client);
        writeDbContext.Branches.Add(cafe);
        await writeDbContext.SaveChangesAsync(cancellationToken);
        return new CafeRegistrationResult(client.Id, cafe.Id, managementKey);
    }

    public async Task<MenuCreationResult> CreateMenuAsync(
        Guid cafeId,
        string managementKey,
        string title,
        bool publish,
        IReadOnlyList<MenuItemInput> items,
        CancellationToken cancellationToken = default)
    {
        await AuthorizeAsync(cafeId, managementKey, true, cancellationToken);
        var menuCount = await writeDbContext.Menus.CountAsync(
            menu => menu.BranchId == cafeId,
            cancellationToken);
        if (menuCount >= 5) throw new RequestConflictException("A cafe can have at most five menus.");
        if (items.Count > 100) throw new DomainException("A menu can contain at most 100 items.");

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var menu = Menu.Create(cafeId, title, now);
        if (publish) menu.Publish();
        writeDbContext.Menus.Add(menu);

        var entities = items.Select(input => CreateMenuItem(menu.Id, input, now)).ToList();
        writeDbContext.MenuItems.AddRange(entities);
        await writeDbContext.SaveChangesAsync(cancellationToken);
        return new MenuCreationResult(menu.Id, entities.Select(item => item.Id).ToList());
    }

    public async Task UpdateMenuItemAsync(
        Guid cafeId,
        Guid menuId,
        Guid itemId,
        string managementKey,
        MenuItemInput item,
        CancellationToken cancellationToken = default)
    {
        await AuthorizeAsync(cafeId, managementKey, false, cancellationToken);
        var entity = await (
                from menuItem in writeDbContext.MenuItems
                join menu in writeDbContext.Menus on menuItem.MenuId equals menu.Id
                where menuItem.Id == itemId && menu.Id == menuId && menu.BranchId == cafeId
                select menuItem)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new ResourceNotFoundException("The menu item was not found.");
        entity.Update(
            item.Title,
            item.Caption,
            item.Category,
            item.PhotoUrl,
            item.Price,
            item.IsAvailable,
            item.DisplayOrder);
        await writeDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<(BranchCustomer Relation, CustomerProfile Customer)>> GetCustomersAsync(
        Guid cafeId,
        string managementKey,
        CancellationToken cancellationToken = default)
    {
        await AuthorizeAsync(cafeId, managementKey, true, cancellationToken);
        var rows = await (
                from relation in readDbContext.BranchCustomers
                join customer in readDbContext.CustomerProfiles on relation.CustomerId equals customer.Id
                where relation.BranchId == cafeId && !relation.IsArchived
                orderby relation.AddedAtUtc descending
                select new { Relation = relation, Customer = customer })
            .Take(500)
            .ToListAsync(cancellationToken);
        return rows.Select(row => (row.Relation, row.Customer)).ToList();
    }

    public async Task<IReadOnlyList<SaleResult>> GetSalesAsync(
        Guid cafeId,
        string managementKey,
        DateTime? fromUtc,
        DateTime? toUtc,
        Guid? customerId,
        Guid? menuItemId,
        CancellationToken cancellationToken = default)
    {
        await AuthorizeAsync(cafeId, managementKey, true, cancellationToken);
        if (fromUtc.HasValue && toUtc.HasValue && fromUtc > toUtc)
            throw new DomainException("The from date must be before the to date.");

        var query = readDbContext.Orders
            .Include(order => order.Items)
            .Where(order => order.BranchId == cafeId && SellingStatuses.Contains(order.Status));
        if (fromUtc.HasValue) query = query.Where(order => order.CreatedAtUtc >= fromUtc.Value);
        if (toUtc.HasValue) query = query.Where(order => order.CreatedAtUtc <= toUtc.Value);
        if (customerId.HasValue) query = query.Where(order => order.CustomerId == customerId.Value);
        if (menuItemId.HasValue) query = query.Where(order => order.Items.Any(item => item.MenuItemId == menuItemId));

        var cafeName = await readDbContext.Branches
            .Where(cafe => cafe.Id == cafeId)
            .Select(cafe => cafe.Name)
            .SingleAsync(cancellationToken);
        var orders = await query
            .OrderByDescending(order => order.CreatedAtUtc)
            .Take(500)
            .ToListAsync(cancellationToken);
        return orders.Select(order => new SaleResult(order, cafeName)).ToList();
    }

    public async Task<Order> ChangeOrderStatusAsync(
        Guid cafeId,
        Guid orderId,
        string managementKey,
        string action,
        string? rejectionReason,
        int expectedVersion,
        CancellationToken cancellationToken = default)
    {
        await AuthorizeAsync(cafeId, managementKey, false, cancellationToken);
        var order = await writeDbContext.Orders.Include(candidate => candidate.Items).SingleOrDefaultAsync(
            candidate => candidate.Id == orderId && candidate.BranchId == cafeId,
            cancellationToken) ?? throw new ResourceNotFoundException("The order was not found.");
        if (order.Version != expectedVersion)
            throw new RequestConflictException("The order changed. Refresh before retrying.");

        var normalizedAction = action.Trim().ToLowerInvariant();
        var now = timeProvider.GetUtcNow().UtcDateTime;
        try
        {
            switch (normalizedAction)
            {
                case "accept":
                    order.Accept(now);
                    await AddCustomerOnAcceptanceAsync(order, now, cancellationToken);
                    break;
                case "reject":
                    order.Reject(rejectionReason ?? string.Empty, now);
                    break;
                case "prepare":
                    order.StartPreparing();
                    break;
                case "ready":
                    order.MarkReady(now);
                    break;
                case "complete":
                    order.Complete(now);
                    break;
                default:
                    throw new DomainException("Action must be accept, reject, prepare, ready, or complete.");
            }
        }
        catch (DomainException exception)
        {
            throw new RequestConflictException(exception.Message);
        }

        try
        {
            await writeDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new RequestConflictException("The order was changed by another client.");
        }
        return order;
    }

    private async Task AddCustomerOnAcceptanceAsync(
        Order order,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var exists = await writeDbContext.BranchCustomers.AnyAsync(
            relation => relation.BranchId == order.BranchId && relation.CustomerId == order.CustomerId,
            cancellationToken);
        if (exists) return;
        var relation = BranchCustomer.Add(
            order.BranchId,
            order.CustomerId,
            "AcceptedOrder",
            order.AllowsPhoneContact ? order.ContactPhoneNumber : null,
            now);
        writeDbContext.BranchCustomers.Add(relation);
    }

    private async Task AuthorizeAsync(
        Guid cafeId,
        string managementKey,
        bool readOnly,
        CancellationToken cancellationToken)
    {
        var hash = readOnly
            ? await readDbContext.Branches.Where(cafe => cafe.Id == cafeId)
                .Select(cafe => cafe.ManagementKeyHash)
                .SingleOrDefaultAsync(cancellationToken)
            : await writeDbContext.Branches.Where(cafe => cafe.Id == cafeId)
                .Select(cafe => cafe.ManagementKeyHash)
                .SingleOrDefaultAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(hash))
            throw new ResourceNotFoundException("The cafe was not found.");
        if (string.IsNullOrWhiteSpace(managementKey) ||
            !TokenHashing.FixedTimeEquals(hash, TokenHashing.Hash(managementKey)))
            throw new RequestUnauthorizedException("The cafe management key is invalid.");
    }

    private static MenuItem CreateMenuItem(Guid menuId, MenuItemInput input, DateTime now)
    {
        var item = MenuItem.Create(menuId, input.Title, input.Price, now, input.Category, input.DisplayOrder);
        item.Update(
            input.Title,
            input.Caption,
            input.Category,
            input.PhotoUrl,
            input.Price,
            input.IsAvailable,
            input.DisplayOrder);
        return item;
    }
}
