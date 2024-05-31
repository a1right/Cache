namespace Caching;

public class CacheOptions
{
    private TimeSpan _expirationTime;

    public TimeSpan ExpirationTime
    {
        get => _expirationTime;
        set
        {
            _expirationTime = value;
            EnsureExpirationTimeValid();
        }
    }

    public CacheOptions()
    {
        EnsureExpirationTimeValid();
    }

    private void EnsureExpirationTimeValid()
    {
        if (ExpirationTime < TimeSpan.Zero)
            throw new InvalidExpirationTimeException(ExpirationTime);
    }
}

public class CacheException(string message) : Exception(message);

public class InvalidExpirationTimeException(TimeSpan expirationTime) : CacheException($"Expiration time {expirationTime} is not valid")
{

}