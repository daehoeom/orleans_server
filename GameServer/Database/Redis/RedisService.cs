using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Database.Redis;

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
}