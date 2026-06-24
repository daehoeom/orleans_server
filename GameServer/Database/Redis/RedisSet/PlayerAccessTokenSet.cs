using StackExchange.Redis;

namespace Database.Redis.RedisSet;

public class PlayerAccessTokenSet(IDatabase database)
{
    private const string Prefix = "PlayerAuth";

    public async Task<bool> StringSet(string token, long playerId)
    {
        var key = $"{Prefix}:{token}";
        return await database.StringSetAsync(key, playerId.ToString());
    }

    public async Task<string> StringGet(string token)
    {
        var key = $"{Prefix}:{token}";
        var value = await database.StringGetAsync(key);
        return !value.HasValue ? string.Empty : value.ToString();
    }
}