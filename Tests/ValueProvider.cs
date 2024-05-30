namespace Tests;

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