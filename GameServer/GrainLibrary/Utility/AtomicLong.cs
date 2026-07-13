namespace GrainLibrary.Utility;

public class AtomicLong(long initialValue = 0)
{
    private long _value = initialValue;

    public long Get()                           
        => Interlocked.Read(ref _value);
    public void Set(long value)                 
        => Interlocked.Exchange(ref _value, value);
    public long GetAndSet(long value)           
        => Interlocked.Exchange(ref _value, value);
    public long IncrementAndGet()               
        => Interlocked.Increment(ref _value);
    public long DecrementAndGet()               
        => Interlocked.Decrement(ref _value);
    public long GetAndIncrement()               
        => Interlocked.Increment(ref _value) - 1;
    public long GetAndDecrement()               
        => Interlocked.Decrement(ref _value) + 1;
    public long AddAndGet(long delta)           
        => Interlocked.Add(ref _value, delta);
    public long GetAndAdd(long delta)           
        => Interlocked.Add(ref _value, delta) - delta;
    public bool CompareAndSet(long expect, long update)
        => Interlocked.CompareExchange(ref _value, update, expect) == expect;
}