namespace Caching;

public interface ICache<TValue>
{
    TValue? this[object key] { get; }
    Task<TValue?> GetOrAdd<TKey>(TKey key, Func<Task<TValue>> valueFactory, CacheOptions? options = null) where TKey : notnull;
    bool TryGet<TKey>(TKey key, out TValue? value) where TKey : notnull;
    bool Remove<TKey>(TKey key) where TKey : notnull;
    int Count { get; }
}