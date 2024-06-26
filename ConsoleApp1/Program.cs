﻿using Cache.DependencyInjection;
using ConsoleApp1.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ConsoleApp1;

internal class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddCache(c => c.ExpirationTime = TimeSpan.FromSeconds(5));

        builder.Services.AddSingleton(typeof(CacheChannel<>));

        var host = builder.Build();
        await host.RunAsync();
    }
}
