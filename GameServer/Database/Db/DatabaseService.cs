using Database.Db.Context;
using Microsoft.Extensions.Logging;

namespace Database.Db;

public class DatabaseService
{
    private readonly ILogger<DatabaseService> _logger;
    
    public GameDbContext Game { get; set; }

    private readonly DbSetting _dbSetting = new();
    
    public DatabaseService(
        ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<DatabaseService>();
        Game = new GameDbContext(new DbConnector(loggerFactory, _dbSetting.GameDbHost, _dbSetting.GameDbPort,
            _dbSetting.GameDbDatabase, _dbSetting.GameDbUser, _dbSetting.GameDbPassWord));
    }
}