using System.Threading.Channels;

namespace ConsoleApp1.Channels;
public class CacheChannel<T>
{
    private readonly Channel<T> _channel = Channel.CreateUnbounded<T>();

    public ChannelWriter<T> Writer => _channel.Writer;
    public ChannelReader<T> Reader => _channel.Reader;
}