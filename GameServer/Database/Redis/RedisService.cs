using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Database.Redis;

public interface IRedisService
{
    Task<bool> TryAcquireLockAsync(string key, string value, TimeSpan expiry);
    Task ReleaseLockAsync(string key, string value);
    IDatabase GetDatabase();
}

public class RedisService
{
    private readonly ILogger<RedisService> _logger;
    private IConnectionMultiplexer _redis;
    
    public IDatabase GetDatabase(int databaseId) => _redis.GetDatabase(databaseId);
    
    public void Connect(RedisSetting setting)
    {
        try
        {
            var redisOption = new ConfigurationOptions()
            {
                EndPoints = { $"{setting.Host}:{setting.Port}" },
                AbortOnConnectFail = false,
                ConnectTimeout = setting.ConnectTimeout,
                SyncTimeout = setting.SyncTimeout,
                Password = setting.Password,
                DefaultDatabase = setting.DefaultDatabase,
            };
            _redis = ConnectionMultiplexer.Connect(redisOption);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            throw;
        }
    }

    public async Task<bool> TryAcquireLockAsync(string key, string value, TimeSpan expiry)
    {
        return await _redis.GetDatabase().StringSetAsync(key, value, expiry, When.NotExists);
    }

    public async Task ReleaseLockAsync(string key, string value)
    {
        var script = @"if redis.call('get', KEYS[1]) == ARGV[1] then
                    return redis.call('del', KEYS[1])
                    else
                        return 0
                    end";

        await _redis.GetDatabase().ScriptEvaluateAsync(script, [key], [value]);
    }
}