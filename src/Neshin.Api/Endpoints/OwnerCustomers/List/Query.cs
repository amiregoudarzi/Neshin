using FastEndpoints;
using Neshin.Domain.Customers;

namespace Neshin.Api.Endpoints.OwnerCustomers.List;

internal sealed record Query : ICommand<IReadOnlyList<Query.Response>>
{
    public Guid BranchId { get; init; }
    public bool IncludeArchived { get; init; }
    private sealed class Handler(
        IOwnerExperienceRepository repository,
        IRequestContext requestContext)
        : ICommandHandler<Query, IReadOnlyList<Response>>
    {
        public async Task<IReadOnlyList<Response>> ExecuteAsync(Query query, CancellationToken ct)
        {
            requestContext.SetNoStore();
            var customers = await repository.GetCustomersAsync(
                query.BranchId, requestContext.ManagementKey, query.IncludeArchived, ct);
            return customers
                .Select(item => ToResponse(item.Relation, item.Profile))
                .ToList();
        }

        private static Response ToResponse(BranchCustomer relation, CustomerProfile profile) => new()
        {
            CustomerId = profile.Id,
            DisplayName = profile.DisplayName,
            ContactPhoneNumber = relation.ContactPhoneNumber,
            IsPhoneNumberVerified = profile.IsPhoneNumberVerified,
            Source = relation.Source,
            Notes = relation.Notes,
            IsArchived = relation.IsArchived,
            AddedAtUtc = relation.AddedAtUtc
        };
    }

    internal sealed record Response
    {
        public Guid CustomerId { get; init; }
        public string? DisplayName { get; init; }
        public string? ContactPhoneNumber { get; init; }
        public bool IsPhoneNumberVerified { get; init; }
        public string Source { get; init; } = string.Empty;
        public string? Notes { get; init; }
        public bool IsArchived { get; init; }
        public DateTime AddedAtUtc { get; init; }
    }
}
