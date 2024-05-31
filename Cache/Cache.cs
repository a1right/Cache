using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests", AllInternalsVisible = true)]
[assembly: InternalsVisibleTo("Cache.DependencyInjection", AllInternalsVisible = true)]

namespace Caching;

internal class Cache<TValue> : ICache<TValue>
{
    public TValue? this[object key] => _values.TryGetValue(key, out var cached) ? cached.Value : default;
    private readonly CacheOptions? _options;
    private readonly ConcurrentDictionary<object, CacheEntry<TValue>> _values = [];
    private readonly ConcurrentDictionary<object, SemaphoreSlim> _semaphores = [];
    public int Count => _values.Count;

    public Cache()
    {
    }

    public Cache(CacheOptions options)
    {
        _options = options;
    }

    public async Task<TValue?> GetOrAdd<TKey>(TKey key, Func<Task<TValue>> valueFactory, CacheOptions? options = null)
        where TKey : notnull
    {
        if (_values.TryGetValue(key, out var value))
            return value.Value;

        using var semaphore = _semaphores.GetOrAdd(key, new SemaphoreSlim(1));

        Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} entered method");

        try
        {
            await semaphore.WaitAsync();

            Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} entered critical section");

            if (_values.TryGetValue(key, out value))
                return value.Value;

            value = new CacheEntry<TValue>(key, await valueFactory(), this, options ?? _options);
            _values.TryAdd(key, value);
            return value.Value;
        }
        finally
        {
            semaphore.Release();
            if (_semaphores.Remove(key, out var storedSemaphore))
                storedSemaphore.Dispose();
        }
    }

    public bool TryGet<TKey>(TKey key, out TValue? value) where TKey : notnull
    {
        var result = _values.TryGetValue(key, out var cacheEntry);
        value = cacheEntry is null ? default : cacheEntry.Value;
        return result;
    }

    public bool Remove<TKey>(TKey key) where TKey : notnull
    {
        var result = _values.TryRemove(key, out var entry);
        if (result)
            entry?.Dispose();

        return result;
    }
}