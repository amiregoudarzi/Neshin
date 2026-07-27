using FastEndpoints;

namespace Neshin.Api.Endpoints.Clients.Orders.Action;

file sealed class Endpoint : Endpoint<Command, Command.Response>
{
    public override void Configure()
    {
        Post("/api/{version}/clients/cafes/{cafeId}/orders/{orderId}/actions");
        Version(1);
        AllowAnonymous();
        Options(builder => builder.RequireRateLimiting("owner"));
    }

    public override async Task HandleAsync(Command req, CancellationToken ct)
    {
        var response = await req.ExecuteAsync(ct);
        await Send.OkAsync(response, ct);
    }

    private sealed class EndpointSummary : Summary<Endpoint>
    {
        public EndpointSummary()
        {
            Summary = "Change an order status";
            Description = "Accepts, rejects, prepares, marks ready, or completes an order.";
            Response<Command.Response>(200, "Order status changed");
            Response(409, "Order version or status conflict");
        }
    }
}
