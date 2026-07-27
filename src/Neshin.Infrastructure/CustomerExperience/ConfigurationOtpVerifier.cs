using Microsoft.Extensions.Configuration;
using Neshin.Application.Abstractions.Authentication;

namespace Neshin.Infrastructure.CustomerExperience;

public sealed class ConfigurationOtpVerifier(IConfiguration configuration) : IOtpVerifier
{
    public Task<bool> VerifyAsync(
        string phoneNumber,
        string otpCode,
        CancellationToken cancellationToken = default)
    {
        var configuredCode = configuration["Otp:DevelopmentCode"];
        var isValid = !string.IsNullOrWhiteSpace(configuredCode) &&
                      string.Equals(configuredCode, otpCode, StringComparison.Ordinal);
        return Task.FromResult(isValid);
    }
}
