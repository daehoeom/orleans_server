using Dapper;
using Database.Db.Context;
using Microsoft.Extensions.Logging;

namespace Database.Db;

public class DatabaseService
{
    private readonly ILogger<DatabaseService> _logger;
    
    public AccountDbContext Account { get; set; }
    public GameDbContext Game { get; set; }

    private readonly DbSetting _dbSetting = new();
    
    public DatabaseService(
        ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<DatabaseService>();
        Account = new AccountDbContext(new DbConnector(loggerFactory, _dbSetting.AccountDbHost,
            _dbSetting.AccountDbPort, _dbSetting.AccountDbDatabase, _dbSetting.AccountDbUser, _dbSetting.AccountDbPassWord));
        Game = new GameDbContext(new DbConnector(loggerFactory, _dbSetting.GameDbHost, _dbSetting.GameDbPort,
            _dbSetting.GameDbDatabase, _dbSetting.GameDbUser, _dbSetting.GameDbPassWord));
    }

    public async Task CheckConnectionAsync()
    {
        using var accountDbConn = Account.Conn.GetConnection();
        
        
        using var gameDbConn = Game.Conn.GetConnection();
        gameDbConn.Open();
        await gameDbConn.QueryFirstOrDefaultAsync<int>("SELECT 1");
        _logger.LogInformation($"[GameDB] Connection Success.");
    }
}