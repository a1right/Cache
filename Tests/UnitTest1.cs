using Caching;

namespace Tests;

public class UnitTest1
{
    [Fact]
    public async Task Cache_Parallel_Add_Contains_All_Values()
    {
        //arrange
        ICache cache = new Cache();

        var providers = Enumerable
            .Range(1, 5)
            .Select(x => new ValueProvider($"key{x}"))
            .ToList();

        List<Task> tasks = [];

        foreach (var provider in providers)
            tasks.Add(cache.GetOrAdd(provider.Key, provider.ValueFactory));

        await Task.WhenAll(tasks);

        var badProvider = new ValueProvider("bad_key");

        foreach (var provider in providers)
        {
            Assert.Equal(provider.CalledCount, cache[provider.Key]);
            Assert.Null(cache[badProvider.Key]);
            Assert.Equal(providers.Count, cache.Count);
        }
    }

    [Fact]
    public async Task Cache_Add_Contains_All_Values()
    {
        //arrange
        ICache cache = new Cache();

        var providers = Enumerable
            .Range(1, 5)
            .Select(x => new ValueProvider($"key{x}"))
            .ToList();

        foreach (var provider in providers)
            await cache.GetOrAdd(provider.Key, provider.ValueFactory);

        foreach (var provider in providers)
        {
            Assert.Equal(provider.CalledCount, cache[provider.Key]);
            Assert.Equal(providers.Count, cache.Count);
        }
    }
}

public class ValueProvider(string key)
{
    public string Key { get; } = key;
    private int _calledCount;
    public int CalledCount => _calledCount;

    public async Task<int> ValueFactory()
    {
        await Task.Delay(Random.Shared.Next() % 5);
        Interlocked.Increment(ref _calledCount);
        return _calledCount;
    }

    public override string ToString() => $"{nameof(ValueProvider)}{{Key = {Key}; CalledCount = {_calledCount}}}";
}