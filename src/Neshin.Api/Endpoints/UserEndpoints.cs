using Neshin.Application.Users.Queries.GetUserById;

namespace Neshin.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/users").WithTags("Users");

        group.MapGet("/{userId:guid}", GetUserByIdAsync)
            .WithName("GetUserById");

        return endpoints;
    }

    private static async Task<IResult> GetUserByIdAsync(
        Guid userId,
        GetUserByIdQueryHandler handler,
        CancellationToken cancellationToken)
    {
        var user = await handler.HandleAsync(new GetUserByIdQuery(userId), cancellationToken);
        return user is null ? Results.NotFound() : Results.Ok(user);
    }
}
