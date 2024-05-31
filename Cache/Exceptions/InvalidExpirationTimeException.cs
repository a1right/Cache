namespace Caching.Exceptions;

public class InvalidExpirationTimeException(TimeSpan expirationTime) : CacheException($"Expiration time {expirationTime} is not valid");