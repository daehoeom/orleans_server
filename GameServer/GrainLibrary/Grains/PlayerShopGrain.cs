using Database.Db;
using Database.Db.Row;
using GrainLibrary.Resource;
using GrainLibrary.Resource.Model.Row;
using SharedLibrary;

namespace GrainLibrary.Grains;

public interface IPlayerShopGrain : IGrainWithIntegerKey
{
    Task<ResultCode> PurchaseProductAsync(long playerId, int productId, int count);
}

public class PlayerShopGrain(DatabaseService dbService, ResourceService resourceService) : Grain, IPlayerShopGrain
{
    public async Task<ResultCode> PurchaseProductAsync(long playerId, int productId, int count)
    {
        if (count <= 0)
        {
            return ResultCode.InvalidParameter;
        }

        var rProduct = resourceService.ShopProduct.Find(productId);
        if (rProduct is null)
        {
            return ResultCode.ProductNotFound;
        }

        if (rProduct.MaxPurchaseCount < count)
        {
            return ResultCode.PurchaseLimitExceeded;
        }

        var hasLimit = rProduct.LimitCount > 0;
        var retCode = hasLimit
            ? await InternalPurchaseLimitAsync(rProduct, playerId, count)
            : await InternalPurchaseAsync(rProduct, playerId, count);
        
        return retCode;
    }

    private async Task<ResultCode> InternalPurchaseLimitAsync(RShopProduct rProduct, long playerId, int count)
    {
        var limit = await dbService.Game.PurchaseLimit.GetAsync(playerId, rProduct.ProductId);
        var purchasedCount = limit?.purchase_count ?? 0;
        if (purchasedCount + count > rProduct.LimitCount)
        {
            return ResultCode.PurchaseLimitExceeded;
        }

        var walletGrain = GrainFactory.GetGrain<IPlayerWalletGrain>(playerId);
        var totalPrice = (long)rProduct.Price * count;
        var isEnough = await walletGrain.IsEnoughAsync(rProduct.Currency, totalPrice);
        if (isEnough != ResultCode.Success)
        {
            return isEnough;
        }

        var spendResult = await walletGrain.SpendAsync(rProduct.Currency, totalPrice);
        if (spendResult != ResultCode.Success)
        {
            return spendResult;
        }

        try
        {
            if (limit == null)
            {
                await dbService.Game.PurchaseLimit.InsertAsync(new PlayerPurchaseLimitRow
                {
                    player_id = playerId,
                    product_id = rProduct.ProductId,
                    purchase_count = count,
                });
            }
            else
            {
                await dbService.Game.PurchaseLimit.AddAsync(playerId, rProduct.ProductId, count);
            }
            
            var inventoryGrain = GrainFactory.GetGrain<IPlayerInventoryGrain>(playerId);
            var addResult = await inventoryGrain.AddAsync(rProduct.RewardItemId, rProduct.RewardItemCount);
            if (addResult.ResultCode != ResultCode.Success)
            {
                return addResult.ResultCode;
            }
        }
        catch
        {
            return ResultCode.DbUpdateError;
        }

        return ResultCode.Success;
    }

    private async Task<ResultCode> InternalPurchaseAsync(RShopProduct rProduct, long playerId, int count)
    {
        var walletGrain = GrainFactory.GetGrain<IPlayerWalletGrain>(playerId);

        var totalPrice = (long)rProduct.Price * count;
        var isEnoughResult = await walletGrain.IsEnoughAsync(rProduct.Currency, totalPrice);
        if (isEnoughResult != ResultCode.Success)
        {
            return isEnoughResult;
        }
 
        var spendResult = await walletGrain.SpendAsync(rProduct.Currency, totalPrice);
        if (spendResult != ResultCode.Success)
        {
            return spendResult;
        }

        var inventoryGrain = GrainFactory.GetGrain<IPlayerInventoryGrain>(playerId);
        var addResult = await inventoryGrain.AddAsync(rProduct.RewardItemId, rProduct.RewardItemCount);
        if (addResult.ResultCode != ResultCode.Success)
        {
            return addResult.ResultCode;
        }

        return ResultCode.Success;
    }
}
