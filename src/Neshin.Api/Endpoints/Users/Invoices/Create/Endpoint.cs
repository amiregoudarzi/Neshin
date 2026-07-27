using FastEndpoints;

namespace Neshin.Api.Endpoints.Users.Invoices.Create;

file sealed class Endpoint : Endpoint<Command, Command.Response>
{
    public override void Configure()
    {
        Post("/api/{version}/users/invoices");
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
            Summary = "Create an invoice";
            Description = "Snapshots current item names and prices into a pending invoice.";
            Response<Command.Response>(201, "Invoice created");
        }
    }
}
