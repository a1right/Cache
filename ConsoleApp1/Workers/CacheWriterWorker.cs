using Caching;
using ConsoleApp1.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ConsoleApp1.Workers;
internal class CacheWriterWorker(
    ILogger<CacheWriterWorker> logger,
    CacheChannel<int> channel,
    ICache<int> cache) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tasks = new List<Task>();
        for (var i = 0; i < 10; i++)
        {
            var key = i;
            tasks.Add(cache.GetOrAdd(key, () => GetAsyncValue(key)));
            logger.LogInformation("Sending {key} to channel", key);
            await channel.Writer.WriteAsync(key, stoppingToken);
        }

        await Task.WhenAll(tasks);
    }

    private async Task<int> GetAsyncValue(int value)
    {
        await Task.Delay(100);
        return value;
    }
}
