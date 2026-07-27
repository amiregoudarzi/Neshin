using FastEndpoints;

namespace Neshin.Api.Endpoints.CustomerOrders.Create;

file sealed class Endpoint : Endpoint<Command, Command.Response>
{
    public override void Configure()
    {
        Post("/api/{version}/customer/orders");
        Version(1);
        AllowAnonymous();
        Version(1);
        Options(builder => builder.RequireRateLimiting("customer-write"));
    }

    public override async Task HandleAsync(Command req, CancellationToken ct)
    {
        var response = await req.ExecuteAsync(ct);
        await Send.ResponseAsync(response, StatusCodes.Status201Created, ct);
    }

    private sealed class EndpointSummary : Summary<Endpoint>
    {
        public EndpointSummary()
        {
            Summary = "Place an anonymous customer order";
            Description = "Prices and availability are read from the write database; client prices are never accepted.";
            Response<Command.Response>(201, "Order created");
        }
    }
}
