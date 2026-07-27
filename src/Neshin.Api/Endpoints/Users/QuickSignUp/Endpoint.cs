using FastEndpoints;

namespace Neshin.Api.Endpoints.Users.QuickSignUp;

file sealed class Endpoint : Endpoint<Command, Command.Response>
{
    public override void Configure()
    {
        Post("/api/{version}/users/quick-sign-up");
        Version(1);
        AllowAnonymous();
        Options(builder => builder.RequireRateLimiting("customer-session"));
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
            Summary = "Quick user sign up";
            Description = "Creates or renews a user login. OTP is required after the weekly login expires.";
            Response<Command.Response>(201, "User signed in");
            Response(401, "Weekly OTP is required or invalid");
        }
    }
}
