using System.Data;

namespace Database.Db;

public interface IDbPipelineWork
{
    Task ExecuteAsync(IDbConnection connection);
}

public class DbPipelineContext<T> : IDbPipelineWork
{
    public Func<Task>? PreProcess { get; init; }
    public Func<IDbConnection, Task<T>> DbProcess { get; init; } = null!;
    public Func<Task>? PostProcess { get; init; }

    public TaskCompletionSource<T> CompletionSource { get; }
        = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public async Task ExecuteAsync(IDbConnection conn)
    {
        try
        {
            if (PreProcess is not null)
            {
                await PreProcess();
            }

            var result = await DbProcess(conn);

            if (PostProcess is not null)
            {
                await PostProcess();
            }

            CompletionSource.SetResult(result);
        }
        catch (Exception ex)
        {
            CompletionSource.SetException(ex);
        }
    }
}