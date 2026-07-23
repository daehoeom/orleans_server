using GrainLibrary.Grains;
using GrainLibrary.Grains.Dto;
using SharedLibrary;

namespace GrainLibrary.Utility;

public static class RewardHelper
{
    public static async Task<RewardGrantResult> GrantAsync(
        IGrainFactory grainFactory, long playerId,
        IEnumerable<(CurrencyType Type, long Amount)> currencies,
        IEnumerable<(int ItemId, int ItemCount)> items)
    {
        var result = new RewardGrantResult();

        if (currencies.Any())
        {
            var walletGrain = grainFactory.GetGrain<IPlayerWalletGrain>(playerId);
            foreach (var (type, amount) in currencies)
            {
                if (type == CurrencyType.None || amount <= 0)
                {
                    continue;
                }

                var addResult = await walletGrain.AddAsync(type, amount);
                result.CurrencyGrants.Add(new CurrencyGrantResult
                {
                    CurrencyType = type,
                    Requested = addResult.Requested,
                    Granted = addResult.Granted,
                    ResultCode = addResult.ResultCode,
                });
            }
        }

        if (items.Any())
        {
            var inventoryGrain = grainFactory.GetGrain<IPlayerInventoryGrain>(playerId);
            foreach (var (itemId, count) in items)
            {
                if (itemId <= 0 || count <= 0)
                {
                    continue;
                }

                var addResult = await inventoryGrain.AddAsync(itemId, count);
                result.ItemGrants.Add(new ItemGrantResult
                {
                    ItemId = itemId,
                    Requested = addResult.Requested,
                    Granted = addResult.Granted,
                    ResultCode = addResult.ResultCode,
                });
            }
        }

        return result;
    }
}
