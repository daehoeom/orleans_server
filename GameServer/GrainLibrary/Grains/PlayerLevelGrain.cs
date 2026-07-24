using Database.Db;
using Database.Db.Row;
using GrainLibrary.Resource;
using GrainLibrary.Resource.Model.Row;
using SharedLibrary;

namespace GrainLibrary.Grains;

public interface IPlayerLevelGrain : IGrainWithIntegerKey
{
    Task<int> GetLevelAsync();
    Task<long> GetExpAsync();
    Task<ResultCode> AddExpAsync(long exp);
}

public class PlayerLevelGrain(DatabaseService dbService, ResourceService resourceService) : Grain, IPlayerLevelGrain
{
    private long PlayerId => this.GetPrimaryKeyLong();

    private int _level = 1;
    private long _exp;
    private bool _exists;

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var row = await dbService.Game.Level.GetAsync(PlayerId);
        if (row != null)
        {
            _level = row.level;
            _exp = row.exp;
            _exists = true;
        }

        await base.OnActivateAsync(cancellationToken);
    }

    public Task<int> GetLevelAsync()
    {
        return Task.FromResult(_level);
    }

    public Task<long> GetExpAsync()
    {
        return Task.FromResult(_exp);
    }

    public async Task<ResultCode> AddExpAsync(long exp)
    {
        if (exp <= 0)
        {
            return ResultCode.InvalidParameter;
        }

        if (!_exists)
        {
            var newRow = new PlayerLevelRow
            {
                player_id = PlayerId,
                level = _level,
                exp = 0,
            };

            var insertedRow = await dbService.Game.Level.InsertAsync(newRow);
            if (insertedRow <= 0)
            {
                return ResultCode.DbInsertError;
            }

            _exists = true;
        }

        var level = _level;
        var remainExp = _exp + exp;
        RLevel? rLevel;

        while ((rLevel = resourceService.Level.Find(level)) is not null && remainExp >= rLevel.RequiredExp)
        {
            remainExp -= rLevel.RequiredExp;
            level = rLevel.NextLevel;
        }

        if (rLevel is null)
        {
            remainExp = 0;
        }

        var affectedRow = await dbService.Game.Level.UpdateLevelAsync(PlayerId, level, remainExp);
        if (affectedRow <= 0)
        {
            return ResultCode.DbUpdateError;
        }

        _level = level;
        _exp = remainExp;

        return ResultCode.Success;
    }
}
