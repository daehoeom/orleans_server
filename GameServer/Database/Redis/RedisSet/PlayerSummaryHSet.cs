using StackExchange.Redis;

namespace Database.Redis.RedisSet;

public class PlayerSummaryHSet(IDatabase database)
{
    private static readonly string _prefix = "PlayerSummary:";
    
}