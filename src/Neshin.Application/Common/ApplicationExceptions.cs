namespace Neshin.Application.Common;

public sealed class RequestUnauthorizedException(string message) : Exception(message);
public sealed class ResourceNotFoundException(string message) : Exception(message);
public sealed class RequestConflictException(string message) : Exception(message);
public sealed class FeatureNotAvailableException(string message) : Exception(message);
