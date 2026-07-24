using GrainLibrary.Grains.Dto;
using GrainLibrary.Resource;
using GrainLibrary.Resource.Model.Row;
using GrainLibrary.Utility;
using SharedLibrary;

namespace GrainLibrary.Grains;

public interface IPlayerGachaGrain : IGrainWithIntegerKey
{
    Task<GachaResultDto> RollingGachaAsync(int gachaId, int count);
}

public class PlayerGachaGrain(ResourceService resourceService) : Grain, IPlayerGachaGrain
{
    private long PlayerId => this.GetPrimaryKeyLong();

    public async Task<GachaResultDto> RollingGachaAsync(int gachaId, int count)
    {
        if (count <= 0)
        {
            return new GachaResultDto { ResultCode = ResultCode.InvalidParameter };
        }

        var rGacha = resourceService.Gacha.Find(gachaId);
        if (rGacha is null)
        {
            return new GachaResultDto { ResultCode = ResultCode.GachaNotFound };
        }

        var pool = resourceService.GachaUnit.FindAll(gachaId);
        if (pool.Count <= 0)
        {
            return new GachaResultDto { ResultCode = ResultCode.GachaPoolEmpty };
        }

        var walletGrain = GrainFactory.GetGrain<IPlayerWalletGrain>(PlayerId);

        var totalAmount = rGacha.CostAmount * count;
        var isEnoughResult = await walletGrain.IsEnoughAsync(rGacha.CostCurrencyType, totalAmount);
        if (isEnoughResult != ResultCode.Success)
        {
            return new GachaResultDto { ResultCode = isEnoughResult };
        }

        var spendResult = await walletGrain.SpendAsync(rGacha.CostCurrencyType, totalAmount);
        if (spendResult != ResultCode.Success)
        {
            return new GachaResultDto { ResultCode = spendResult };
        }

        var weights = pool
            .Select(p => new IntRandomWeight<RGachaUnit>(p) { Weight = p.Weight })
            .ToList();

        var unitGrain = GrainFactory.GetGrain<IPlayerUnitGrain>(PlayerId);
        var units = new List<GachaUnitResultDto>(count);

        for (var i = 0; i < count; i++)
        {
            var rolledUnit = RandomUtil.GetRandomWeight(weights);

            var addResult = await unitGrain.AddOrUpdateAsync(rolledUnit.UnitId, level: 1);
            if (addResult != ResultCode.Success)
            {
                return new GachaResultDto { ResultCode = addResult, Units = units };
            }

            var unit = await unitGrain.GetAsync(rolledUnit.UnitId);

            units.Add(new GachaUnitResultDto
            {
                UnitId = rolledUnit.UnitId,
                Level = unit?.Level ?? 1,
                Stack = unit?.Stack ?? 0,
            });
        }

        return new GachaResultDto
        {
            ResultCode = ResultCode.Success,
            Units = units,
        };
    }
}
