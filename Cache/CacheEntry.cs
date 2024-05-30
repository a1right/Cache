﻿using System.Timers;
using Timer = System.Timers.Timer;

namespace Caching;

public class CacheEntry
{
    public bool IsExpired { get; private set; }
    private readonly CacheOptions? _options;
    private readonly object _key;
    private readonly Cache _cache;
    public object? Value { get; }

    internal CacheEntry(object key, object? value, Cache cache, CacheOptions? options = null)
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

        var timer = new Timer(_options.ExpirationTime);
        timer.Elapsed += ExpiredHandler;
        timer.Start();
        timer.AutoReset = false;
    }

    private void ExpiredHandler(object? sender, ElapsedEventArgs e)
    {
        IsExpired = true;
        _cache.Remove(_key);
    }
}