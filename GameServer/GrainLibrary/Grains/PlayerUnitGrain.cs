using Database.Db;
using Database.Db.Row;
using GrainLibrary.Grains.Dto;
using GrainLibrary.Resource;
using SharedLibrary;
using SharedLibrary.Packet.Data;

namespace GrainLibrary.Grains;

public interface IPlayerUnitGrain : IGrainWithIntegerKey
{
    Task<UnitDto?> GetAsync(int unitId);
    Task<IReadOnlyList<UnitDto>> GetAllAsync();
    Task<List<UnitInfo>> GetAllInfoAsync();
    Task<ResultCode> AddOrUpdateAsync(int unitId, int level);
    Task<ResultCode> LevelUpAsync(int unitId);
}

public class PlayerUnitGrain(DatabaseService dbService, ResourceLoader resourceLoader) : Grain, IPlayerUnitGrain
{
    private long PlayerId => this.GetPrimaryKeyLong();

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
                Stack = unit.stack,
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

    public Task<List<UnitInfo>> GetAllInfoAsync()
    {
        var result = _units.Select(p => new UnitInfo
        {
            UnitId = p.Value.UnitId,
            Stack = p.Value.Stack,
        }).ToList();

        return Task.FromResult(result);
    }

    public async Task<ResultCode> AddOrUpdateAsync(int unitId, int level)
    {
        if (_units.TryGetValue(unitId, out var unit))
        {
            if (unit.Stack >= SharedConstant.MAX_UNIT_STACK)
            {
                return ResultCode.MaxUnitStack;
            }
            
            var affectedRow = await dbService.Game.Units.AddStackAsync(PlayerId, unitId);
            if (affectedRow <= 0)
            {
                return ResultCode.DbUpdateError;
            }

            _units[unitId].Stack++;
        }
        else
        {
            var insertedRow = await dbService.Game.Units.InsertAsync(new PlayerUnitRow
            {
                player_id = PlayerId,
                unit_id = unitId,
                level = level,
            });
            if (insertedRow <= 0)
            {
                return ResultCode.DbInsertError;
            }

            _units[unitId] = new UnitDto
            {
                UnitId = unitId,
                Level = level,
                Stack = 0,
            };
        }

        return ResultCode.Success;
    }

    public async Task<ResultCode> LevelUpAsync(int unitId)
    {
        if (!_units.TryGetValue(unitId, out var unit))
        {
            return ResultCode.NotFoundUnit;
        }

        var rUnitLevel = resourceLoader.UnitLevel.Find(unitId, unit.Level);
        if (rUnitLevel is null)
        {
            return ResultCode.MaxLevelUnit;
        }

        if (rUnitLevel.RequireStack > unit.Stack)
        {
            return ResultCode.NotEnoughUnitStack;
        }

        var walletGrain = GrainFactory.GetGrain<IPlayerWalletGrain>(PlayerId);
        var retCode = await walletGrain.IsEnoughAsync(rUnitLevel.RequireCurrencyType, rUnitLevel.RequireCurrencyAmount);
        if (retCode != ResultCode.Success)
        {
            return retCode;
        }

        retCode = await walletGrain.SpendAsync(rUnitLevel.RequireCurrencyType, rUnitLevel.RequireCurrencyAmount);
        if (retCode != ResultCode.Success)
        {
            return retCode;
        }

        var level = unit.Level + 1;
        var affectedRow = await dbService.Game.Units.UpdateLevelAsync(PlayerId, unitId, level);
        if (affectedRow <= 0)
        {
            return ResultCode.DbUpdateError;
        }

        unit.Level = level;

        return ResultCode.Success;
    }
}
