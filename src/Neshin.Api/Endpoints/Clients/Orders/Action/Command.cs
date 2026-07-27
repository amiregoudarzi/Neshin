using FastEndpoints;

namespace Neshin.Api.Endpoints.Clients.Orders.Action;

internal sealed record Command : ICommand<Command.Response>
{
    public Guid CafeId { get; init; }
    public Guid OrderId { get; init; }
    public string Action { get; init; } = string.Empty;
    public string? RejectionReason { get; init; }
    public int ExpectedVersion { get; init; }

    internal sealed record Response(
        Guid OrderId,
        string Status,
        int Version,
        DateTime? AcceptedAtUtc,
        DateTime? ReadyAtUtc,
        DateTime? CompletedAtUtc,
        DateTime? RejectedAtUtc);

    private sealed class Handler(
        IClientExperienceRepository repository,
        IRequestContext requestContext) : ICommandHandler<Command, Response>
    {
        public async Task<Response> ExecuteAsync(Command command, CancellationToken ct)
        {
            var order = await repository.ChangeOrderStatusAsync(
                command.CafeId,
                command.OrderId,
                requestContext.ManagementKey,
                command.Action,
                command.RejectionReason,
                command.ExpectedVersion,
                ct);
            return new Response(
                order.Id,
                order.Status.ToString(),
                order.Version,
                order.AcceptedAtUtc,
                order.ReadyAtUtc,
                order.CompletedAtUtc,
                order.RejectedAtUtc);
        }
    }
}
