using Microsoft.EntityFrameworkCore;
using Neshin.Application.Abstractions.Persistence;
using Neshin.Application.Common;
using Neshin.Domain.Customers;
using Neshin.Domain.Ordering;
using Neshin.Infrastructure.Persistence;
using Npgsql;

namespace Neshin.Infrastructure.CustomerExperience;

public sealed class CustomerOrderRepository(
    NeshinWriteDbContext writeDbContext,
    NeshinReadDbContext readDbContext,
    TimeProvider timeProvider) : ICustomerOrderRepository
{
    public async Task<Order> PlaceOrderAsync(
        string sessionToken,
        Guid branchId,
        string idempotencyKey,
        string paymentMethodName,
        IReadOnlyList<(Guid MenuItemId, int Quantity)> items,
        string? displayName,
        string? contactPhoneNumber,
        bool allowPhoneContact,
        CancellationToken cancellationToken = default)
    {
        var request = new OrderRequest(
            branchId,
            idempotencyKey,
            paymentMethodName,
            items.Select(item => new OrderItemRequest(item.MenuItemId, item.Quantity)).ToList(),
            displayName,
            contactPhoneNumber,
            allowPhoneContact);
        request = NormalizeAndValidateOrderRequest(request);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var session = await ResolveWriteSessionAsync(sessionToken, now, cancellationToken);

        var existing = await writeDbContext.Orders
            .Include(order => order.Items)
            .SingleOrDefaultAsync(
                order => order.CustomerId == session.CustomerId &&
                         order.IdempotencyKey == request.IdempotencyKey,
                cancellationToken);

        if (existing is not null)
        {
            EnsureIdempotentReplay(existing, request);
            return existing;
        }

        var branch = await (
                from candidate in writeDbContext.Branches
                join client in writeDbContext.Clients on candidate.ClientId equals client.Id
                where candidate.Id == request.BranchId && candidate.IsActive && client.IsActive
                select candidate)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new ResourceNotFoundException("The venue was not found.");

        if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, true, out var paymentMethod) ||
            !Enum.IsDefined(paymentMethod))
            throw new Domain.Common.DomainException("The payment method is invalid.");

        if (paymentMethod == PaymentMethod.Online)
            throw new Domain.Common.DomainException(
                "Online payment is not available until payment verification is configured.");

        var hasActiveVisit = await writeDbContext.VenueVisits.AnyAsync(
            visit => visit.CustomerId == session.CustomerId &&
                     visit.BranchId == request.BranchId &&
                     visit.EndedAtUtc == null &&
                     visit.LastSeenAtUtc >= now.Subtract(TimeSpan.FromMinutes(15)),
            cancellationToken);
        if (!hasActiveVisit)
            throw new Domain.Common.DomainException("An active in-venue visit is required to place an order.");

        var requestedItemIds = request.Items.Select(item => item.MenuItemId).ToList();
        var menuItems = await (
                from item in writeDbContext.MenuItems
                join menu in writeDbContext.Menus on item.MenuId equals menu.Id
                where requestedItemIds.Contains(item.Id) &&
                      item.IsAvailable &&
                      menu.IsPublished &&
                      menu.BranchId == request.BranchId
                select item)
            .ToListAsync(cancellationToken);

        if (menuItems.Count != requestedItemIds.Count)
            throw new Domain.Common.DomainException("One or more menu items are unavailable for this venue.");

        var order = Order.Create(
            request.BranchId,
            session.CustomerId,
            paymentMethod,
            request.IdempotencyKey,
            branch.AcceptsAppOrders,
            branch.AllowsPayAtVenue,
            now,
            null);

        foreach (var requestedItem in request.Items)
        {
            var menuItem = menuItems.Single(item => item.Id == requestedItem.MenuItemId);
            order.AddItem(menuItem.Id, menuItem.Name, menuItem.Price, requestedItem.Quantity);
        }

        order.SetContact(request.DisplayName, request.ContactPhoneNumber, request.AllowPhoneContact);
        order.SubmitForPayment(now);

        if (!string.IsNullOrWhiteSpace(request.DisplayName) || !string.IsNullOrWhiteSpace(request.ContactPhoneNumber))
        {
            var profile = await writeDbContext.CustomerProfiles.SingleAsync(
                customer => customer.Id == session.CustomerId,
                cancellationToken);
            profile.SetOptionalContact(request.DisplayName, request.ContactPhoneNumber);
        }

        session.Touch(now);
        writeDbContext.Orders.Add(order);
        try
        {
            await writeDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            writeDbContext.Entry(order).State = EntityState.Detached;
            foreach (var item in order.Items) writeDbContext.Entry(item).State = EntityState.Detached;

            var winner = await writeDbContext.Orders
                .Include(candidate => candidate.Items)
                .SingleAsync(
                    candidate => candidate.CustomerId == session.CustomerId &&
                                 candidate.IdempotencyKey == request.IdempotencyKey,
                    cancellationToken);
            EnsureIdempotentReplay(winner, request);
            return winner;
        }

        return order;
    }

    public async Task<Order?> GetOrderAsync(
        string sessionToken,
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var customerId = await ResolveReadCustomerIdAsync(sessionToken, cancellationToken);
        var order = await readDbContext.Orders
            .Include(candidate => candidate.Items)
            .SingleOrDefaultAsync(
                candidate => candidate.Id == orderId && candidate.CustomerId == customerId,
                cancellationToken);

        return order;
    }

    private async Task<CustomerSession> ResolveWriteSessionAsync(
        string token,
        DateTime now,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new RequestUnauthorizedException("A customer session token is required.");

        var hash = TokenHashing.Hash(token);
        var session = await writeDbContext.CustomerSessions.SingleOrDefaultAsync(
            candidate => candidate.TokenHash == hash,
            cancellationToken);

        if (session is null || !session.IsValidAt(now))
            throw new RequestUnauthorizedException("The customer session is invalid or expired.");

        return session;
    }

    private async Task<Guid> ResolveReadCustomerIdAsync(string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new RequestUnauthorizedException("A customer session token is required.");

        var hash = TokenHashing.Hash(token);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var session = await readDbContext.CustomerSessions.SingleOrDefaultAsync(
            candidate => candidate.TokenHash == hash &&
                         candidate.RevokedAtUtc == null &&
                         candidate.ExpiresAtUtc > now,
            cancellationToken);

        return session?.CustomerId ??
            throw new RequestUnauthorizedException("The customer session is invalid or expired.");
    }

    private static OrderRequest NormalizeAndValidateOrderRequest(OrderRequest request)
    {
        if (request.BranchId == Guid.Empty) throw new Domain.Common.DomainException("Branch is required.");
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey) || request.IdempotencyKey.Length > 100)
            throw new Domain.Common.DomainException("A valid idempotency key is required.");
        if (request.Items.Count is < 1 or > 30)
            throw new Domain.Common.DomainException("An order must contain between 1 and 30 items.");
        if (request.Items.Any(item => item.MenuItemId == Guid.Empty || item.Quantity is < 1 or > 20))
            throw new Domain.Common.DomainException("Each item requires a valid id and a quantity between 1 and 20.");
        if (request.Items.Select(item => item.MenuItemId).Distinct().Count() != request.Items.Count)
            throw new Domain.Common.DomainException("Duplicate menu items must be combined into one line.");

        var normalizedPhoneNumber = string.IsNullOrWhiteSpace(request.ContactPhoneNumber)
            ? null
            : Domain.Identity.User.NormalizePhoneNumber(request.ContactPhoneNumber);

        return request with
        {
            IdempotencyKey = request.IdempotencyKey.Trim(),
            PaymentMethod = request.PaymentMethod.Trim(),
            DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? null : request.DisplayName.Trim(),
            ContactPhoneNumber = normalizedPhoneNumber,
            AllowPhoneContact = request.AllowPhoneContact && normalizedPhoneNumber is not null
        };
    }

    private static void EnsureIdempotentReplay(Order existing, OrderRequest request)
    {
        var sameItems = existing.Items.Count == request.Items.Count &&
                        request.Items.All(requested => existing.Items.Any(existingItem =>
                            existingItem.MenuItemId == requested.MenuItemId &&
                            existingItem.Quantity == requested.Quantity));

        if (existing.BranchId != request.BranchId ||
            !string.Equals(existing.PaymentMethod.ToString(), request.PaymentMethod, StringComparison.OrdinalIgnoreCase) ||
            !sameItems ||
            !string.Equals(existing.CustomerDisplayName, request.DisplayName, StringComparison.Ordinal) ||
            !string.Equals(existing.ContactPhoneNumber, request.ContactPhoneNumber, StringComparison.Ordinal) ||
            existing.AllowsPhoneContact != request.AllowPhoneContact)
            throw new RequestConflictException("The idempotency key was already used for a different order.");
    }

    private sealed record OrderRequest(
        Guid BranchId,
        string IdempotencyKey,
        string PaymentMethod,
        IReadOnlyList<OrderItemRequest> Items,
        string? DisplayName,
        string? ContactPhoneNumber,
        bool AllowPhoneContact);

    private sealed record OrderItemRequest(Guid MenuItemId, int Quantity);
}
