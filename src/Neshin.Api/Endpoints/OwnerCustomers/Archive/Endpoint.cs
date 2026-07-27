using FastEndpoints;

namespace Neshin.Api.Endpoints.OwnerCustomers.Archive;

file sealed class Endpoint : Endpoint<Command>
{
    public override void Configure()
    {
        Delete("/api/{version}/owner/branches/{branchId:guid}/customers/{customerId:guid}");
        Version(1);
        AllowAnonymous();
        Options(builder => builder.RequireRateLimiting("owner"));
    }

    public override async Task HandleAsync(Command req, CancellationToken ct)
    {
        await req.ExecuteAsync(ct);
        await Send.NoContentAsync(ct);
    }

    private sealed class EndpointSummary : Summary<Endpoint>
    {
        public EndpointSummary()
        {
            Summary = "Archive a customer from branch CRM";
            Description = "Archives the customer relationship for the selected branch.";
            Response(204, "Customer archived");
        }
    }
}
