using System.Timers;
using Timer = System.Timers.Timer;

namespace Caching;

internal class CacheEntry<T> : IDisposable
{
    public bool IsExpired { get; private set; }
    private readonly CacheOptions? _options;
    private readonly object _key;
    private readonly Cache<T> _cache;
    private Timer? _timer;
    public T? Value { get; }

    internal CacheEntry(object key, T? value, Cache<T> cache, CacheOptions? options = null)
    {
        Value = value;
        _key = key;
        _cache = cache;
        _options = options;
        RegisterExpiredEventHandler();
    }

    private void RegisterExpiredEventHandler()
    {
        if (_options is null)
            return;

        _timer = new Timer(_options.ExpirationTime);
        _timer.Elapsed += ExpiredHandler;
        _timer.Start();
        _timer.AutoReset = false;
    }

    private void ExpiredHandler(object? sender, ElapsedEventArgs e)
    {
        IsExpired = true;
        _cache.Remove(_key);
    }

    public void Dispose()
    {
        if (_timer is null)
            return;

        _timer.Elapsed -= ExpiredHandler;
        _timer.Dispose();
    }
}