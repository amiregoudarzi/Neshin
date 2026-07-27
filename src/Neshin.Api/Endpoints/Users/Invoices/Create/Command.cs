using FastEndpoints;

namespace Neshin.Api.Endpoints.Users.Invoices.Create;

internal sealed record Command : ICommand<Command.Response>
{
    public Guid CafeId { get; init; }
    public IReadOnlyList<Item> Items { get; init; } = [];

    internal sealed record Item(Guid MenuItemId, int Quantity);

    internal sealed record Response
    {
        public Guid InvoiceId { get; init; }
        public string Status { get; init; } = string.Empty;
        public decimal TotalAmount { get; init; }
        public DateTime CreatedAtUtc { get; init; }
        public IReadOnlyList<LineResponse> Lines { get; init; } = [];
    }

    internal sealed record LineResponse(
        Guid MenuItemId,
        string Title,
        decimal UnitPrice,
        int Quantity,
        decimal LineTotal);

    private sealed class Handler(
        IUserExperienceRepository repository,
        IRequestContext requestContext) : ICommandHandler<Command, Response>
    {
        public async Task<Response> ExecuteAsync(Command command, CancellationToken ct)
        {
            requestContext.SetNoStore();
            var invoice = await repository.CreateInvoiceAsync(
                requestContext.CustomerSessionToken,
                command.CafeId,
                command.Items.Select(item => (item.MenuItemId, item.Quantity)).ToList(),
                ct);
            return new Response
            {
                InvoiceId = invoice.Id,
                Status = invoice.Status.ToString(),
                TotalAmount = invoice.TotalAmount,
                CreatedAtUtc = invoice.CreatedAtUtc,
                Lines = invoice.Items.Select(item => new LineResponse(
                    item.MenuItemId,
                    item.Title,
                    item.UnitPrice,
                    item.Quantity,
                    item.UnitPrice * item.Quantity)).ToList()
            };
        }
    }
}
