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
    private bool _disposed;
    public T? Value { get; }

    internal CacheEntry(object key, T? value, Cache<T> cache, CacheOptions? options = null)
    {
        Value = value;
        _key = key;
        _cache = cache;
        _options = options;
        RegisterExpiredEventHandler();
    }

    ~CacheEntry() => Dispose(false);

    private void RegisterExpiredEventHandler()
    {
        if (_options is null || _options.ExpirationTime == TimeSpan.Zero)
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
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (_timer != null)
        {
            _timer.Elapsed -= ExpiredHandler;
            _timer.Dispose();
        }

        _disposed = true;
    }
}