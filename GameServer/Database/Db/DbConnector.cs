using System.Data;
using System.Reflection;
using System.Threading.Channels;
using Dapper;
using Database.Db.Attribute;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace Database.Db;

public class DbConnector
{
    private static readonly int PoolThreadCount = Environment.ProcessorCount * 4;
    private readonly string _connectionString;
    private readonly ILogger<DbConnector> _logger;

    private readonly Channel<IDbPipelineWork> _queue;

    public DbConnector(ILoggerFactory loggerFactory, string host, int port, string database, string user, string password)
    {
        _logger = loggerFactory.CreateLogger<DbConnector>();
        _connectionString 
            = $"Server={host};Port={port};Database={database};User={user};Password={password};Pooling=true;Max Pool Size={PoolThreadCount};";
            
        _queue = Channel.CreateUnbounded<IDbPipelineWork>(new  UnboundedChannelOptions()
        {
            SingleReader = true, 
            SingleWriter = true
        });

        Task.Run(ProcessQueueAsync);
    }
    
    private async Task ProcessQueueAsync()
    {
        await foreach (var work in _queue.Reader.ReadAllAsync())
        {
            try
            {
                using var conn = GetConnection();
                await ((MySqlConnection)conn).OpenAsync();
                await work.ExecuteAsync(conn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DB work failed");
            }
        }
    }
    
    private string GetTableName<T>()
    {
        var tableNameAttribute = typeof(T).GetCustomAttribute<TableAttribute>();
        return tableNameAttribute?.TableName ?? string.Empty;
    }
    
    public IDbConnection GetConnection()
    {
        return new MySqlConnection(_connectionString);
    }
    
    public Task<T> PipelineAsync<T>(
        Func<IDbConnection, Task<T>> dbProcess)
    {
        var workContext = new DbPipelineContext<T>
        {
            DbProcess = dbProcess,
        };

        if (_queue.Writer.TryWrite(workContext))
        {
            return workContext.CompletionSource.Task;
        }
        
        _logger.LogError($"Could not write to queue {GetTableName<T>()}");
        return Task.FromException<T>(new InvalidOperationException("Failed to enqueue DB work"));
    }

    public Task<T> WithTransactionAsync<T>(Func<DbTransactionScope, Task<T>> work)
    {
        return PipelineAsync(async conn =>
        {
            var tx = conn.BeginTransaction();
            await using var scope = new DbTransactionScope(tx);
            return await work(scope);
        });
    }
    
    public Task<int> InsertAsync<T>(T entity) where T : class
        => PipelineAsync(async conn =>
        {
            var columns = string.Join(", ", typeof(T).GetProperties().Select(p => p.Name));
            var values = string.Join(", ", typeof(T).GetProperties().Select(p => $"@{p.Name}"));
            var insertQuery = $@"INSERT INTO {GetTableName<T>()} ({columns}) VALUES ({values});";
            return await conn.ExecuteAsync(insertQuery, entity);
        });
    
    public Task<int> BulkInsertAsync<T>(IEnumerable<T> entities) where T : class
    {
        var index = 0;
        return PipelineAsync(async conn =>
        {
            var props = typeof(T).GetProperties();
            var columns = string.Join(", ", props.Select(p => p.Name));
            var parameters = new DynamicParameters();
            var valuesList = new List<string>();

            foreach (var entity in entities)
            {
                var valueParts = new List<string>();
                foreach (var prop in props)
                {
                    var paramName = $"@{prop.Name}{index}";
                    valueParts.Add(paramName);
                    parameters.Add(paramName, prop.GetValue(entity));
                }

                valuesList.Add($"({string.Join(", ", valueParts)})");
                index++;
            }

            var valuesSql = string.Join(", ", valuesList);
            var insertQuery = $@"INSERT INTO {GetTableName<T>()} ({columns}) VALUES {valuesSql};";
            return await conn.ExecuteAsync(insertQuery, parameters);
        });
    }

    public Task<int> ExecuteAsync(string sql, object? param = null, IDbTransaction? transaction = null, 
        int? commandTimeout = null, CommandType? commandType = null)
        => PipelineAsync(conn => conn.ExecuteAsync(sql, param, transaction, commandTimeout, commandType));

    public Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, 
        int? commandTimeout = null, CommandType? commandType = null)
        => PipelineAsync(conn => conn.QueryFirstOrDefaultAsync<T>(sql, param, transaction, commandTimeout, commandType));

    public Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null,
        int? commandTimeout = null, CommandType? commandType = null)
        => PipelineAsync(conn => conn.QueryAsync<T>(sql, param, transaction, commandTimeout, commandType));

    public Task<T> ExecuteTransactionAsync<T>(Func<IDbConnection, IDbTransaction, Task<T>> work)
    {
        return PipelineAsync(async conn =>
        {
            using var transaction = conn.BeginTransaction();
            try
            {
                var result = await work(conn, transaction);
                transaction.Commit();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Transaction rolled back");
                transaction.Rollback();
                throw;
            }
        });
    }
}