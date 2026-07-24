using GrainLibrary.Resource;

namespace GrainLibrary.Utility;

public static class TimeUtil
{
    public static DateTime UtcNow => DateTime.UtcNow;

    private static int ResetHourOffset;

    public static void Init(ResourceService resourceService)
    {
        ResetHourOffset = resourceService.Constants.ResetHour;
    }

    public static bool IsExpired(DateTime targetDate)
    {
        if (targetDate == ServerConstants.InfinityTime)
        {
            return false;
        }

        var utcNow = UtcNow;
        return utcNow > targetDate;
    }
    
    public static int GetDayOfValue(DateTime startTime)
    {
        var utcNow = UtcNow;
        var resetOffset = TimeSpan.FromHours(ResetHourOffset);
        var startServerDay = (startTime - resetOffset).Date;
        var nowServerDay = (utcNow - resetOffset).Date;

        var daysElapsed = (nowServerDay - startServerDay).Days;
        var day = daysElapsed + 1;
        if (day < 1)
        {
            day = 1;
        }
        
        return day;
    }
}