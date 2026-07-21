using Neshin.Domain.Identity;

namespace Neshin.Domain.UnitTests.Identity;

public sealed class UserTests
{
    [Theory]
    [InlineData("09121234567", "09121234567")]
    [InlineData("+989121234567", "09121234567")]
    [InlineData("00989121234567", "09121234567")]
    public void Create_NormalizesIranianPhoneNumber(string input, string expected)
    {
        var user = User.Create(input, DateTimeOffset.UtcNow);

        Assert.Equal(expected, user.PhoneNumber);
    }
}
