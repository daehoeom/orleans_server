using Database.Db;
using Database.Db.Row;
using SharedLibrary;

namespace GrainLibrary.Grains;

public interface IPlayerUnitGrain : IGrainWithIntegerKey
{
    Task<PlayerUnitRow?> GetAsync(int unitId);
    Task<IReadOnlyList<PlayerUnitRow>> GetAllAsync();
    Task<ResultCode> UpdateLevelAsync(int unitId, int level);
    Task<ResultCode> UpdateGradeAsync(int unitId, int grade);
}

public class PlayerUnitGrain(DatabaseService dbService) : Grain, IPlayerUnitGrain
{
    private long PlayerId => this.GetPrimaryKeyLong();

    // 활성화 시 DB에서 한 번만 읽어 캐싱하고, 이후에는 Level/Grade 갱신에서 DB와 함께 write-through로 갱신한다.
    private readonly Dictionary<int, PlayerUnitRow> _units = new();

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var units = await dbService.Game.Units.GetsAsync(PlayerId);
        foreach (var unit in units)
        {
            _units[unit.unit_id] = unit;
        }

        await base.OnActivateAsync(cancellationToken);
    }

    public Task<PlayerUnitRow?> GetAsync(int unitId)
    {
        return Task.FromResult(_units.GetValueOrDefault(unitId));
    }

    public Task<IReadOnlyList<PlayerUnitRow>> GetAllAsync()
    {
        return Task.FromResult<IReadOnlyList<PlayerUnitRow>>(_units.Values.ToList());
    }

    public async Task<ResultCode> UpdateLevelAsync(int unitId, int level)
    {
        if (!_units.TryGetValue(unitId, out var unit))
        {
            return ResultCode.NotFoundResource;
        }

        var affectedRow = await dbService.Game.Units.UpdateLevelAsync(PlayerId, unitId, level);
        if (affectedRow <= 0)
        {
            return ResultCode.DbUpdateError;
        }

        unit.level = level;

        return ResultCode.Success;
    }

    public async Task<ResultCode> UpdateGradeAsync(int unitId, int grade)
    {
        if (!_units.TryGetValue(unitId, out var unit))
        {
            return ResultCode.NotFoundResource;
        }

        var affectedRow = await dbService.Game.Units.UpdateGradeAsync(PlayerId, unitId, grade);
        if (affectedRow <= 0)
        {
            return ResultCode.DbUpdateError;
        }
        
        unit.grade = grade;

        return ResultCode.Success;
    }
}
