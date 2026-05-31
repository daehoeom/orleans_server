using System.Data;

namespace Database.Db.Context;

public interface IDbContext
{
    Task<T> ExecuteAsync<T>(
        Func<IDbConnection, Task<T>> dbAction,
        Func<Task<T>>? preAction = null,
        Func<Task<T>>? postAction = null);
}

public class DbContext(DbConnector conn)
{
    public DbConnector Conn => conn;
    
    public async Task<T> ExecuteAsync<T>(
        Func<IDbConnection, Task<T>> dbAction,
        Func<Task<T>>? preAction = null,
        Func<Task<T>>? postAction = null)
    {
        return await conn.PipelineAsync(dbAction, preAction, postAction);
    }
}