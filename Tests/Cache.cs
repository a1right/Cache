using Caching;

namespace Tests;

public class CacheTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public async Task Cache_Parallel_Add_Contains_All_Values(int count)
    {
        //arrange
        ICache<int> cache = new Cache<int>();

        var providers = Enumerable
            .Range(1, count)
            .Select(x => new ValueProvider<int>().SetKey($"key{x}").SetReturnValue(valueProvider => valueProvider.CalledCount))
            .ToList();

        List<Task> tasks = [];

        //act
        foreach (var provider in providers)
            tasks.Add(cache.GetOrAdd(provider.Key, provider.ValueFactory));

        await Task.WhenAll(tasks);

        //assert
        foreach (var provider in providers)
        {
            Assert.Equal(provider.CalledCount, cache[provider.Key]);
            Assert.Equal(providers.Count, cache.Count);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task Cache_Add_Contains_All_Values(int count)
    {
        //arrange
        ICache<int> cache = new Cache<int>();

        var providers = Enumerable
            .Range(1, count)
            .Select(x => new ValueProvider<int>().SetKey($"key{x}").SetReturnValue(provider => provider.CalledCount))
            .ToList();

        //act
        foreach (var provider in providers)
            await cache.GetOrAdd(provider.Key, provider.ValueFactory);

        //assert
        foreach (var provider in providers)
        {
            Assert.Equal(provider.CalledCount, cache[provider.Key]);
            Assert.Equal(providers.Count, cache.Count);
        }
    }

    [Fact]
    public async Task Add_Value_Without_Delay_Factory_Called_Exactly_Once()
    {
        //arrange
        ICache<int> cache = new Cache<int>();

        var provider = new ValueProvider<int>()
            .SetKey($"key")
            .SetDelay(TimeSpan.Zero)
            .SetReturnValue(x => x.CalledCount);

        List<Task> tasks = [];

        //act
        foreach (var _ in Enumerable.Range(1, 10))
            tasks.Add(cache.GetOrAdd(provider.Key, provider.ValueFactory));

        await Task.WhenAll(tasks);

        //assert
        Assert.Equal(1, provider.CalledCount);
        Assert.Equal(1, cache.Count);
    }

    [Fact]
    public async Task Add_Value_With_Delay_Factory_Called_Exactly_Once()
    {
        //arrange
        ICache<int> cache = new Cache<int>();

        var provider = new ValueProvider<int>()
            .SetKey($"key")
            .SetDelay(TimeSpan.FromMilliseconds(1000))
            .SetReturnValue(x => x.CalledCount);

        List<Task> tasks = [];

        //act
        foreach (var _ in Enumerable.Range(1, 10))
            tasks.Add(cache.GetOrAdd(provider.Key, provider.ValueFactory));

        await Task.WhenAll(tasks);

        //assert
        Assert.Equal(1, provider.CalledCount);
        Assert.Equal(1, cache.Count);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData("key", null)]
    public async Task Removed_Item_Is_Removed<T>(T? value, T? defaultValue)
    {
        //arrange
        ICache<T?> cache = new Cache<T?>();
        var localValue = value;
        var provider = new ValueProvider<T>().SetKey("key").SetReturnValue(_ => localValue);
        _ = await cache.GetOrAdd(provider, provider.ValueFactory);

        //act
        cache.Remove(provider.Key);

        //assert
        Assert.False(cache.TryGet(provider.Key, out value));
        Assert.Equal(defaultValue, value);
    }

    [Fact]
    public async Task Removed_Item_Expiration_Timer_Is_Not_Active()
    {
        //arrange
        ICache<int> cache = new Cache<int>(new CacheOptions() { ExpirationTime = TimeSpan.FromMilliseconds(10) });
        var provider = new ValueProvider<int>().SetKey("key").SetDelay(TimeSpan.Zero);

        //act
        await cache.GetOrAdd(provider.Key, provider.ValueFactory);
        cache.Remove(provider.Key);
        var addedValue = await cache.GetOrAdd(provider.Key, provider.ValueFactory, new CacheOptions() { ExpirationTime = TimeSpan.FromSeconds(10) });
        await Task.Delay(50);

        //assert
        Assert.True(cache.TryGet(provider.Key, out var value));
        Assert.Equal(addedValue, value);
    }
}