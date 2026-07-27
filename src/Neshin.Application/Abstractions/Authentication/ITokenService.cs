namespace Neshin.Application.Abstractions.Authentication;

// Implement this interface in the separate custom token project when authentication is added.
public interface ITokenService
{
    public string CreateAccessToken(Guid userId, string phoneNumber);
}
