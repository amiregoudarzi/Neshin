using Neshin.Domain.Ordering;

namespace Neshin.Application.Abstractions.Persistence;

public interface ICustomerOrderRepository
{
    public Task<Order> PlaceOrderAsync(
        string sessionToken,
        Guid branchId,
        string idempotencyKey,
        string paymentMethodName,
        IReadOnlyList<(Guid MenuItemId, int Quantity)> items,
        string? displayName,
        string? contactPhoneNumber,
        bool allowPhoneContact,
        CancellationToken cancellationToken = default);

    public Task<Order?> GetOrderAsync(
        string sessionToken,
        Guid orderId,
        CancellationToken cancellationToken = default);
}
