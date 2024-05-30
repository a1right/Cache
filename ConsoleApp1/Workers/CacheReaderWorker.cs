using Caching;
using ConsoleApp1.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ConsoleApp1.Workers;
internal class CacheReaderWorker(
    ILogger<CacheReaderWorker> logger,
    CacheChannel<int> channel,
    ICache<int> cache) : BackgroundService
{
    private List<int> _receivedKeys = [];
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Task.Run(() => RecheckEntryLifetime(stoppingToken));

        await foreach (var key in channel.Reader.ReadAllAsync(stoppingToken))
        {
            if (cache.TryGet(key, out var value))
                logger.LogInformation("Received key from channel and got value: {@value}", value);
            else
                logger.LogInformation("Received key from channel but don`t have value");

            _receivedKeys.Add(key);
        }
    }

    private async Task RecheckEntryLifetime(CancellationToken stoppingToken)
    {
        while (true)
        {
            await Task.Delay(1000, stoppingToken);
            logger.LogInformation("Recheck entry lifetime");
            foreach (var key in _receivedKeys.ToList())
            {
                if (cache.TryGet(key, out var value))
                    logger.LogInformation("Key: {key}, value: {@value} is in cache", key, value);
                else
                {
                    logger.LogInformation("Key: {key} is not in cache", key);
                    _receivedKeys.Remove(key);
                }
            }
        }
    }
}
