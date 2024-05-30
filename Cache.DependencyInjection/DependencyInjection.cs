using Caching;
using Microsoft.Extensions.DependencyInjection;

namespace Cache.DependencyInjection;
public static class DependencyInjection
{
    public static IServiceCollection AddCache(this IServiceCollection services, Action<CacheOptions>? optionsConfigurator = null)
    {
        services
            .AddCacheOptions(optionsConfigurator)
            .AddSingleton(typeof(ICache<>), typeof(Cache<>));

        return services;
    }

    private static IServiceCollection AddCacheOptions(this IServiceCollection services, Action<CacheOptions>? optionsConfigurator = null)
    {
        if (optionsConfigurator is null)
            return services;

        var options = new CacheOptions();
        optionsConfigurator?.Invoke(options);

        if (options.ExpirationTime <= TimeSpan.Zero)
            throw new ArgumentException("Provide non default cache entry lifetime value");

        services.AddSingleton(options);
        return services;
    }
}
