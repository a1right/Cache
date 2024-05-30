namespace Caching;

public interface ICache
{
    object this[object key] { get; }
    Task<TValue?> GetOrAdd<TKey, TValue>(TKey key, Func<Task<TValue>> valueFactory, CacheOptions? options = null) where TKey : notnull;
    bool TryGet<TKey, TValue>(TKey key, out TValue? value) where TKey : notnull;
    bool Remove<TKey>(TKey key) where TKey : notnull;
    IReadOnlyDictionary<object, CacheEntry> Values { get; }
    IReadOnlyDictionary<object, SemaphoreSlim> Semaphores { get; }
    int Count { get; }
}