using FastEndpoints;

namespace Neshin.Api.Endpoints.CustomerSessions.Create;

internal sealed record Command : ICommand<Command.Response>
{
    internal sealed record Response
    {
        public Guid CustomerId { get; init; }
        public string AccessToken { get; init; } = string.Empty;
        public DateTime ExpiresAtUtc { get; init; }
    }

    private sealed class Handler(
        IPublicExperienceRepository repository,
        IRequestContext requestContext)
        : ICommandHandler<Command, Response>
    {
        public async Task<Response> ExecuteAsync(Command command, CancellationToken ct)
        {
            requestContext.SetNoStore();
            var result = await repository.CreateSessionAsync(ct);
            requestContext.SetCustomerSessionCookie(result.AccessToken, result.Session.ExpiresAtUtc);
            return new Response
            {
                CustomerId = result.Customer.Id,
                AccessToken = result.AccessToken,
                ExpiresAtUtc = result.Session.ExpiresAtUtc
            };
        }
    }
}
