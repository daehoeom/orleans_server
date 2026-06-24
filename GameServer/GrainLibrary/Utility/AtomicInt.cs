namespace GrainLibrary.Utility;

public class AtomicInt(int initializeValue = 0)
{
    private volatile int _value = initializeValue;

    public int Get()                          => _value;
    public void Set(int value)                => Interlocked.Exchange(ref _value, value);
    public int GetAndSet(int value)           => Interlocked.Exchange(ref _value, value);
    public int IncrementAndGet()              => Interlocked.Increment(ref _value);
    public int DecrementAndGet()              => Interlocked.Decrement(ref _value);
    public int GetAndIncrement()              => Interlocked.Increment(ref _value) - 1;
    public int GetAndDecrement()              => Interlocked.Decrement(ref _value) + 1;
    public int AddAndGet(int delta)           => Interlocked.Add(ref _value, delta);
    public int GetAndAdd(int delta)           => Interlocked.Add(ref _value, delta) - delta;
    public bool CompareAndSet(int expect, int update)
        => Interlocked.CompareExchange(ref _value, update, expect) == expect;
}