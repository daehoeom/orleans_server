namespace ServerLibrary.Utility;

#region Random Weight

public interface IRandomWeight<TValue, TWeight>
    where TWeight : struct, IComparable
{
    public TValue Value { get; set; }
    public TWeight Weight { get; set; }
}

public class IntRandomWeight<TValue>(TValue value)
    : IRandomWeight<TValue, int>
{
    public TValue Value { get; set; } = value;
    public int Weight { get; set; }
}

public class LongRandomWeight<TValue>(TValue value)
    : IRandomWeight<TValue, long>
{
    public TValue Value { get; set; } = value;
    public long Weight { get; set; }
}

#endregion

public static class RandomUtil
{
    private static readonly Random Random = new Random();

    public static int Next(int max)
        => Random.Next(max);

    public static int Next(int min, int max)
        => Random.Next(min, max);

    public static long NextInt64(long max)
        => Random.NextInt64(max);

    public static long NextInt64(long min, long max)
        => Random.NextInt64(min, max);

    public static double NextDouble()
        => Random.NextDouble();

    public static TValue GetRandomWeight<TValue>(List<IntRandomWeight<TValue>> source)
        where TValue : class
    {
        if (source is not { Count: > 0 })
        {
            throw new ArgumentException("Empty random weight source.");
        }

        var totalWeight = source.Sum(p => p.Weight);
        var randomNum = Random.Shared.Next(0, totalWeight);

        var currentSum = 0;
        foreach (var item in source)
        {
            currentSum += item.Weight;
            if (randomNum < currentSum)
            {
                return item.Value;
            }
        }

        return source[^1].Value;
    }

    public static TValue GetRandomWeight<TValue>(List<LongRandomWeight<TValue>> source)
        where TValue : class
    {
        if (source is not { Count: > 0 })
        {
            throw new ArgumentException("Empty random weight source.");
        }

        var totalWeight = source.Sum(p => p.Weight);
        var randomNum = Random.Shared.NextInt64(0, totalWeight);

        var currentSum = 0L;
        foreach (var item in source)
        {
            currentSum += item.Weight;
            if (randomNum < currentSum)
            {
                return item.Value;
            }
        }

        return source[^1].Value;
    }
}