using System.Security.Cryptography;
using System.Text;

namespace Neshin.Infrastructure.CustomerExperience;

internal static class TokenHashing
{
    public static string CreateToken()
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        return token.TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    public static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();

    public static bool FixedTimeEquals(string first, string second)
    {
        var firstHash = SHA256.HashData(Encoding.UTF8.GetBytes(first));
        var secondHash = SHA256.HashData(Encoding.UTF8.GetBytes(second));
        return CryptographicOperations.FixedTimeEquals(firstHash, secondHash);
    }
}
