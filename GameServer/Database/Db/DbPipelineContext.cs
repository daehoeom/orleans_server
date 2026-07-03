using System.Data;

namespace Database.Db;

public interface IDbPipelineWork
{
    Task ExecuteAsync(IDbConnection connection);
}

public class DbPipelineContext<T> : IDbPipelineWork
{
    public Func<IDbConnection, Task<T>> DbProcess { get; init; } = null!;

    public TaskCompletionSource<T> CompletionSource { get; }
        = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public async Task ExecuteAsync(IDbConnection conn)
    {
        try
        {
            var result = await DbProcess(conn);
            CompletionSource.SetResult(result);
        }
        catch (Exception ex)
        {
            CompletionSource.SetException(ex);
        }
    }
}