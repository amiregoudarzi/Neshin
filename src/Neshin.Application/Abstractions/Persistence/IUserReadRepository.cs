namespace Neshin.Application.Abstractions.Persistence;

public interface IUserReadRepository
{
    Task<UserReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed record UserReadModel(
    Guid Id,
    string PhoneNumber,
    bool IsPhoneNumberVerified,
    DateTimeOffset CreatedAtUtc);
