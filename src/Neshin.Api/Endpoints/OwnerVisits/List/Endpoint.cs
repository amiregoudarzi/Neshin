using FastEndpoints;

namespace Neshin.Api.Endpoints.OwnerVisits.List;

file sealed class Endpoint : Endpoint<Query, IReadOnlyList<Query.Response>>
{
    public override void Configure()
    {
        Get("/api/{version}/owner/branches/{branchId:guid}/visits");
        Version(1);
        AllowAnonymous();
        Options(builder => builder.RequireRateLimiting("owner"));
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
            Summary = "Get active app visits for a branch";
            Description = "Returns recent active application visits for the selected branch.";
            Response<IReadOnlyList<Query.Response>>(200, "Successful");
        }
    }
}
