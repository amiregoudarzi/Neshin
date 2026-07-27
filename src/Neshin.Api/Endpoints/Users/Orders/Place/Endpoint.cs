using FastEndpoints;

namespace Neshin.Api.Endpoints.Users.Orders.Place;

file sealed class Endpoint : Endpoint<Command, Command.Response>
{
    public override void Configure()
    {
        Post("/api/{version}/users/orders");
        Version(1);
        AllowAnonymous();
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
            Summary = "Place an order";
            Description = "Creates an order using currently available items from one cafe.";
            Response<Command.Response>(201, "Order placed");
        }
    }
}
