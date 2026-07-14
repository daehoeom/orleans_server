using Database.Db;
using Database.Db.Row;
using GrainLibrary.Grains.Dto;
using SharedLibrary;

namespace GrainLibrary.Grains;

public interface IPlayerUnitGrain : IGrainWithIntegerKey
{
    Task<UnitDto?> GetAsync(int unitId);
    Task<IReadOnlyList<UnitDto>> GetAllAsync();
    Task<ResultCode> AcquireAsync(int unitId, int level, int grade);
    Task<ResultCode> UpdateLevelAsync(int unitId, int level);
    Task<ResultCode> UpdateGradeAsync(int unitId, int grade);
}

public class PlayerUnitGrain(DatabaseService dbService) : Grain, IPlayerUnitGrain
{
    private long PlayerId => this.GetPrimaryKeyLong();

    // 활성화 시 DB Row를 DTO로 변환해 캐싱하고, 이후에는 Acquire/Level/Grade 갱신에서 DB와 함께 write-through로 갱신한다.
    private readonly Dictionary<int, UnitDto> _units = new();

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var units = await dbService.Game.Units.GetsAsync(PlayerId);
        foreach (var unit in units)
        {
            _units[unit.unit_id] = new UnitDto
            {
                UnitId = unit.unit_id,
                Level = unit.level,
                Grade = unit.grade,
            };
        }

        await base.OnActivateAsync(cancellationToken);
    }

    public Task<UnitDto?> GetAsync(int unitId)
    {
        return Task.FromResult(_units.GetValueOrDefault(unitId));
    }

    public Task<IReadOnlyList<UnitDto>> GetAllAsync()
    {
        return Task.FromResult<IReadOnlyList<UnitDto>>(_units.Values.ToList());
    }

    public async Task<ResultCode> AcquireAsync(int unitId, int level, int grade)
    {
        if (_units.ContainsKey(unitId))
        {
            return ResultCode.AlreadyOwnedUnit;
        }

        var insertedRow = await dbService.Game.Units.InsertAsync(new PlayerUnitRow
        {
            player_id = PlayerId,
            unit_id = unitId,
            level = level,
            grade = grade,
        });
        if (insertedRow <= 0)
        {
            return ResultCode.DbInsertError;
        }

        _units[unitId] = new UnitDto
        {
            UnitId = unitId,
            Level = level,
            Grade = grade,
        };

        return ResultCode.Success;
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

        unit.Level = level;

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

        unit.Grade = grade;

        return ResultCode.Success;
    }
}
