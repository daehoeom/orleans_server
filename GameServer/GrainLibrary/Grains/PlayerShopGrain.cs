using Database.Db;
using Database.Db.Row;
using GrainLibrary.Resource;
using SharedLibrary;

namespace GrainLibrary.Grains;

public interface IPlayerShopGrain : IGrainWithIntegerKey
{
    Task<(ResultCode, int)> PurchaseProductAsync(long playerId, int productId, int count);
}

public class PlayerShopGrain(DatabaseService dbService, ResourceLoader resourceLoader) : Grain, IPlayerShopGrain
{
    public async Task<(ResultCode, int)> PurchaseProductAsync(long playerId, int productId, int count)
    {
        if (count <= 0)
        {
            return (ResultCode.InvalidParameter, 0);
        }

        var rProduct = resourceLoader.ShopProduct.Find(productId);
        if (rProduct is null)
        {
            return (ResultCode.ProductNotFound, 0);
        }

        var limit = await dbService.Game.PurchaseLimit.GetAsync(playerId, productId);
        var purchasedCount = limit?.purchase_count ?? 0;
        if (rProduct.LimitCount > 0 && purchasedCount + count > rProduct.LimitCount)
        {
            return (ResultCode.PurchaseLimitExceeded, 0);
        }

        var walletGrain = GrainFactory.GetGrain<IPlayerWalletGrain>(playerId);

        var totalPrice = (long)rProduct.Price * count;
        var isEnough = await walletGrain.IsEnough(rProduct.Currency, totalPrice);
        if (isEnough != ResultCode.Success)
        {
            return (isEnough, 0);
        }

        var spendResult = await walletGrain.SpendAsync(rProduct.Currency, totalPrice);
        if (spendResult != ResultCode.Success)
        {
            return (spendResult, 0);
        }

        try
        {
            if (limit == null)
            {
                await dbService.Game.PurchaseLimit.InsertAsync(new PlayerPurchaseLimitRow
                {
                    player_id = playerId,
                    product_id = productId,
                    purchase_count = count,
                });
            }
            else
            {
                await dbService.Game.PurchaseLimit.AddAsync(playerId, productId, count);
            }
        }
        catch
        {
            // 구매기록 갱신 실패 시 차감한 재화를 보상 환불
            var refunded = await walletGrain.AddAsync(rProduct.Currency, totalPrice);
            return (ResultCode.DbUpdateError, (int)refunded);
        }

        var remain = await walletGrain.GetBalanceAsync(rProduct.Currency);
        return (ResultCode.Success, (int)remain);
    }
}