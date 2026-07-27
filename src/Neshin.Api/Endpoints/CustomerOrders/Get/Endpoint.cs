using FastEndpoints;

namespace Neshin.Api.Endpoints.CustomerOrders.Get;

file sealed class Endpoint : Endpoint<Query, Query.Response>
{
    public override void Configure()
    {
        Get("/api/{version}/customer/orders/{orderId:guid}");
        Version(1);
        AllowAnonymous();
        Options(builder => builder.RequireRateLimiting("public-read"));
    }

    public override async Task HandleAsync(Query req, CancellationToken ct)
    {
        var response = await req.ExecuteAsync(ct);
        await Send.OkAsync(response, ct);
    }

    private sealed class EndpointSummary : Summary<Endpoint>
    {
        public EndpointSummary()
        {
            Summary = "Get the current customer's order";
            Description = "The opaque customer session token restricts access to the customer's own order.";
            Response<Query.Response>(200, "Successful");
            Response(404, "Order not found");
        }
    }
}
