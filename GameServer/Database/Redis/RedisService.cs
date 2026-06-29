using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Database.Redis;

public class RedisService(ILogger<RedisService> logger)
{
    private readonly RedisSetting _setting = new();
    private IConnectionMultiplexer _redis = null!;

    public IDatabase GetDatabase(int databaseId) => _redis.GetDatabase(databaseId);

    public async Task ConnectAsync()
    {
        var options = new ConfigurationOptions
        {
            EndPoints = { $"{_setting.Host}:{_setting.Port}" },
            AbortOnConnectFail = true,
            ConnectTimeout = _setting.ConnectTimeout,
            SyncTimeout = _setting.SyncTimeout,
            Password = _setting.Password,
            DefaultDatabase = _setting.DefaultDatabase,
        };

        _redis = await ConnectionMultiplexer.ConnectAsync(options);

        var latency = await _redis.GetDatabase().PingAsync();
        logger.LogInformation($"[Redis] Connection OK. Latency: {latency.TotalMilliseconds:F1}ms");
    }

    public async Task<bool> TryAcquireLockAsync(string key, string value, TimeSpan expiry)
    {
        return await _redis.GetDatabase().StringSetAsync(key, value, expiry, When.NotExists);
    }

    public async Task ReleaseLockAsync(string key, string value)
    {
        const string script = @"if redis.call('get', KEYS[1]) == ARGV[1] then
                                    return redis.call('del', KEYS[1])
                                else
                                    return 0
                                end";

        await _redis.GetDatabase().ScriptEvaluateAsync(script, [(RedisKey)key], [(RedisValue)value]);
    }
}