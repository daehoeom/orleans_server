using System.Data;

namespace Database.Db;

public class DbTransactionScope : IAsyncDisposable
{
    private readonly IDbTransaction _transaction;
    private readonly List<Func<Task>> _onCommitted = new();
    private readonly List<Func<Task>> _onRolledback = new();
    private bool _completed;

    internal DbTransactionScope(IDbTransaction tx) => _transaction = tx;

    public IDbTransaction Transaction => _transaction;

    /// <summary>커밋이 성공한 경우에만 실행 (예: 캐시 반영, 성공 로그)</summary>
    public void OnCommitted(Func<Task> action) => _onCommitted.Add(action);

    /// <summary>롤백된 경우에만 실행 (예: 실패 로그, 알림, 재시도 큐 등록)</summary>
    public void OnRolledBack(Func<Task> action) => _onRolledback.Add(action);
    
    /// <summary>명시적으로 호출해야 커밋됨. 호출 안 하면 Dispose 시점에 자동 롤백.</summary>
    public void Complete() => _completed = true;

    public async ValueTask DisposeAsync()
    {
        if (_completed)
        {
            _transaction.Commit();
            foreach (var action in _onCommitted)
            {
                await action();
            }
        }
        else
        {
            _transaction.Rollback();
            foreach (var action in _onRolledback)
            {
                await action();
            }
        }
        
        _transaction.Dispose();
    }
}