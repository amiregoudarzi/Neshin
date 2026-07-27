using Neshin.Domain.Customers;
using Neshin.Domain.Ordering;

namespace Neshin.Application.Abstractions.Persistence;

public interface IClientExperienceRepository
{
    public Task<CafeRegistrationResult> RegisterCafeAsync(
        string clientName,
        string cafeName,
        decimal latitude,
        decimal longitude,
        string? description,
        string? address,
        string? publicPhoneNumber,
        IReadOnlyList<string>? photoUrls,
        CancellationToken cancellationToken = default);

    public Task<MenuCreationResult> CreateMenuAsync(
        Guid cafeId,
        string managementKey,
        string title,
        bool publish,
        IReadOnlyList<MenuItemInput> items,
        CancellationToken cancellationToken = default);

    public Task UpdateMenuItemAsync(
        Guid cafeId,
        Guid menuId,
        Guid itemId,
        string managementKey,
        MenuItemInput item,
        CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<(BranchCustomer Relation, CustomerProfile Customer)>> GetCustomersAsync(
        Guid cafeId,
        string managementKey,
        CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<SaleResult>> GetSalesAsync(
        Guid cafeId,
        string managementKey,
        DateTime? fromUtc,
        DateTime? toUtc,
        Guid? customerId,
        Guid? menuItemId,
        CancellationToken cancellationToken = default);

    public Task<Order> ChangeOrderStatusAsync(
        Guid cafeId,
        Guid orderId,
        string managementKey,
        string action,
        string? rejectionReason,
        int expectedVersion,
        CancellationToken cancellationToken = default);
}

public sealed record CafeRegistrationResult(
    Guid ClientId,
    Guid CafeId,
    string ManagementKey);

public sealed record MenuCreationResult(Guid MenuId, IReadOnlyList<Guid> ItemIds);

public sealed record MenuItemInput(
    string Title,
    string? Caption,
    string? Category,
    string? PhotoUrl,
    decimal Price,
    bool IsAvailable,
    int DisplayOrder);

public sealed record SaleResult(Order Order, string CafeName);
