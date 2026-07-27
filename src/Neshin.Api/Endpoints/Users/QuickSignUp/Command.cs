using FastEndpoints;

namespace Neshin.Api.Endpoints.Users.QuickSignUp;

internal sealed record Command : ICommand<Command.Response>
{
    public string PhoneNumber { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? OtpCode { get; init; }

    internal sealed record Response
    {
        public Guid UserId { get; init; }
        public Guid CustomerId { get; init; }
        public string AccessToken { get; init; } = string.Empty;
        public DateTime ExpiresAtUtc { get; init; }
        public bool IsNewUser { get; init; }
        public bool OtpWasRequired { get; init; }
    }

    private sealed class Handler(
        IUserExperienceRepository repository,
        IRequestContext requestContext) : ICommandHandler<Command, Response>
    {
        public async Task<Response> ExecuteAsync(Command command, CancellationToken ct)
        {
            requestContext.SetNoStore();
            var result = await repository.QuickSignUpAsync(
                command.PhoneNumber,
                command.Name,
                command.OtpCode,
                ct);
            requestContext.SetCustomerSessionCookie(result.AccessToken, result.ExpiresAtUtc);
            return new Response
            {
                UserId = result.UserId,
                CustomerId = result.CustomerId,
                AccessToken = result.AccessToken,
                ExpiresAtUtc = result.ExpiresAtUtc,
                IsNewUser = result.IsNewUser,
                OtpWasRequired = result.OtpWasRequired
            };
        }
    }
}
