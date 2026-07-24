namespace GrainLibrary.Utility;

public static class TimeUtil
{
    public static DateTime UtcNow => DateTime.UtcNow;

    public static bool IsExpired(DateTime targetDate)
    {
        if (targetDate == ServerConstants.InfinityTime)
        {
            return false;
        }

        var utcNow = UtcNow;
        return utcNow > targetDate;
    }
}