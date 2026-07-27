using FastEndpoints;
using Neshin.Domain.Customers;

namespace Neshin.Api.Endpoints.OwnerCustomers.Add;

internal sealed record Command : ICommand<Command.Response>
{
    public Guid BranchId { get; init; }
    public Guid CustomerId { get; init; }
    public string? Notes { get; init; }
    private sealed class Handler(
        IOwnerExperienceRepository repository,
        IRequestContext requestContext)
        : ICommandHandler<Command, Response>
    {
        public async Task<Response> ExecuteAsync(Command command, CancellationToken ct)
        {
            requestContext.SetNoStore();
            var result = await repository.AddCustomerAsync(
                command.BranchId,
                command.CustomerId,
                requestContext.ManagementKey,
                command.Notes,
                ct);
            return ToResponse(result.Relation, result.Profile);
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
