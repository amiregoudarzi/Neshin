using FastEndpoints;

namespace Neshin.Api.Endpoints.OwnerContent.CreateEvent;

file sealed class Endpoint : Endpoint<Command, Command.Response>
{
    public override void Configure()
    {
        Post("/api/{version}/owner/branches/{branchId:guid}/events");
        Version(1);
        AllowAnonymous();
        Options(builder => builder.RequireRateLimiting("owner"));
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
            Summary = "Create a venue event";
            Description = "Creates an event for the selected branch.";
            Response<Command.Response>(201, "Event created");
        }
    }
}
