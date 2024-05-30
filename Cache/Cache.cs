using System.Collections;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests", AllInternalsVisible = true)]
namespace Caching;

internal class Cache : ICache, IEnumerable
{
    public object? this[object key] => _values.TryGetValue(key, out var cached) ? cached.Value : null;
    private readonly CacheOptions? _options;
    private readonly ConcurrentDictionary<object, CacheEntry> _values = [];
    private readonly ConcurrentDictionary<object, SemaphoreSlim> _semaphores = [];
    private readonly object _lock = new();
    public int Count => _values.Count;
    public IReadOnlyDictionary<object, CacheEntry> Values => _values;
    public IReadOnlyDictionary<object, SemaphoreSlim> Semaphores => _semaphores;

    public Cache() { }

    public Cache(CacheOptions options)
    {
        _options = options;
    }

    public async Task<TValue?> GetOrAdd<TKey, TValue>(TKey key, Func<Task<TValue>> valueFactory, CacheOptions? options = null) where TKey : notnull
    {
        if (_values.TryGetValue(key, out var value))
            return (TValue?)value.Value;

        var semaphore = _semaphores.GetOrAdd(key, new SemaphoreSlim(1));

        try
        {
            await semaphore.WaitAsync();

            if (_values.TryGetValue(key, out value))
                return (TValue?)value.Value;

            value = new CacheEntry(key, await valueFactory(), this, options ?? _options);
            _values.TryAdd(key, value);
            semaphore.Release();
            return (TValue?)value.Value;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public bool TryGet<TKey, TValue>(TKey key, out TValue? value) where TKey : notnull
    {
        var semaphore = _semaphores.GetOrAdd(key, new SemaphoreSlim(1));

        try
        {
            semaphore.Wait();

            var result = _values.TryGetValue(key, out var cacheEntry);
            value = (TValue?)cacheEntry?.Value;
            return result;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public bool Remove<TKey>(TKey key) where TKey : notnull
    {
        var semaphore = _semaphores.GetOrAdd(key, new SemaphoreSlim(1));

        try
        {
            semaphore.Wait();
            return _values.TryRemove(key, out _);
        }
        finally
        {
            _semaphores.Remove(key, out _);
            semaphore.Release();
        }
    }

    public IEnumerator GetEnumerator()
    {
        return _values.GetEnumerator();
    }
}