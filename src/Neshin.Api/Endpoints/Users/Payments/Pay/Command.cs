using FastEndpoints;
using Neshin.Application.Common;

namespace Neshin.Api.Endpoints.Users.Payments.Pay;

internal sealed record Command : ICommand<Command.Response>
{
    public Guid InvoiceId { get; init; }

    internal sealed record Response(Guid InvoiceId, string Status);

    private sealed class Handler : ICommandHandler<Command, Response>
    {
        public Task<Response> ExecuteAsync(Command command, CancellationToken ct) =>
            throw new FeatureNotAvailableException(
                "Online payment is not configured yet. Connect the third-party payment provider first.");
    }
}
