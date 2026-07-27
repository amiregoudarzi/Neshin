using FastEndpoints;

namespace Neshin.Api.Endpoints.OwnerContent.CreateMenuItem;

file sealed class Endpoint : Endpoint<Command, Command.Response>
{
    public override void Configure()
    {
        Post("/api/{version}/owner/branches/{branchId:guid}/menus/{menuId:guid}/items");
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
            Summary = "Create a menu item";
            Description = "Creates an item in the selected menu.";
            Response<Command.Response>(201, "Menu item created");
        }
    }
}
