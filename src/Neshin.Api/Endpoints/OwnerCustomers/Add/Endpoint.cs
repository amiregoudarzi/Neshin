using FastEndpoints;

namespace Neshin.Api.Endpoints.OwnerCustomers.Add;

file sealed class Endpoint : Endpoint<Command, Command.Response>
{
    public override void Configure()
    {
        Put("/api/{version}/owner/branches/{branchId:guid}/customers/{customerId:guid}");
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
            Summary = "Add or restore a customer in branch CRM";
            Description = "Adds a customer to the branch CRM or restores an archived relationship.";
            Response<Command.Response>(200, "Successful");
        }
    }
}
