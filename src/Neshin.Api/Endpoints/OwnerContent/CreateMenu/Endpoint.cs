using FastEndpoints;

namespace Neshin.Api.Endpoints.OwnerContent.CreateMenu;

file sealed class Endpoint : Endpoint<Command, Command.Response>
{
    public override void Configure()
    {
        Post("/api/{version}/owner/branches/{branchId:guid}/menus");
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
            Summary = "Create a branch menu";
            Description = "Creates a menu for the selected branch.";
            Response<Command.Response>(201, "Menu created");
        }
    }
}
