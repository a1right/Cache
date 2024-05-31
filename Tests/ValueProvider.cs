namespace Tests;

public class ValueProvider<T>
{
    public object Key { get; private set; } = null!;
    public Func<ValueProvider<T>, T?> ValueFactoryFunc { get; private set; } = _ => default(T);
    public TimeSpan Delay { get; private set; } = TimeSpan.Zero;
    private int _calledCount;
    public int CalledCount => _calledCount;

    public async Task<T?> ValueFactory()
    {
        await Task.Delay(Delay);
        Interlocked.Increment(ref _calledCount);
        return ValueFactoryFunc(this);
    }

    public ValueProvider<T> SetDelay(TimeSpan delay)
    {
        Delay = delay;
        return this;
    }

    public ValueProvider<T> SetKey(object key)
    {
        Key = key;
        return this;
    }

    public ValueProvider<T> SetReturnValue(Func<ValueProvider<T>, T?> valueFactory)
    {
        ValueFactoryFunc = valueFactory;
        return this;
    }

    public override string ToString() => $"{typeof(ValueProvider<>).Name}{{Key = {Key}; CalledCount = {_calledCount}}}";
}