namespace Tests;

public class ValueProvider
{
    public object Key { get; private set; }
    public TimeSpan Delay { get; private set; } = TimeSpan.Zero;
    private int _calledCount;
    public int CalledCount => _calledCount;

    public async Task<int> ValueFactory()
    {
        await Task.Delay(Random.Shared.Next() % 5);
        Interlocked.Increment(ref _calledCount);
        return _calledCount;
    }

    public ValueProvider SetDelay(TimeSpan delay)
    {
        Delay = delay;
        return this;
    }

    public ValueProvider SetKey(object key)
    {
        Key = key;
        return this;
    }

    public override string ToString() => $"{nameof(ValueProvider)}{{Key = {Key}; CalledCount = {_calledCount}}}";
}