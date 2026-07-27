using FastEndpoints;

namespace Neshin.Api.Endpoints.OwnerOrders.Action;

file sealed class Endpoint : Endpoint<Command, Command.Response>
{
    public override void Configure()
    {
        Post("/api/{version}/owner/branches/{branchId:guid}/orders/{orderId:guid}/actions");
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
            Summary = "Apply an order workflow action";
            Description = "Supported actions: accept, reject, start-preparing, ready, complete.";
            Response<Command.Response>(200, "Successful");
        }
    }
}
