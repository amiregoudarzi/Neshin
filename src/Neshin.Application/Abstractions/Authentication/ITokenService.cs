namespace Neshin.Application.Abstractions.Authentication;

// Implement this interface in the separate custom token project when authentication is added.
public interface ITokenService
{
    string CreateAccessToken(Guid userId, string phoneNumber);
}
